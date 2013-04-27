using System;
using System.IO;
using Platform.IO;
using Platform.Text;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text
{
	public partial class TextNetworkFileSystemClient
	{
		internal class TextRandomAccessNetworkFileSystemStream
			: Stream, IStreamWithEvents
		{			
			private readonly bool canSeek;			
			private readonly FileShare fileShare;
			private readonly FileAccess fileAccess;
			private readonly TextNetworkFileSystemClient client;

			public TextRandomAccessNetworkFileSystemStream(TextNetworkFileSystemClient client, FileAccess fileAccess, FileShare fileShare, bool canSeek, long length)
			{
				this.client = client;
				this.canSeek = canSeek;
				this.fileShare = fileShare;
				this.fileAccess = fileAccess;
				this.length = length;

				this.writeResponsesQueue = new InvocationQueue();
			}

			public override bool CanRead
			{
				get
				{
					return (this.fileAccess & FileAccess.Read) == FileAccess.Read;
				}
			}

			public override bool CanSeek
			{
				get
				{
					return this.canSeek;
				}
			}

			public override bool CanWrite
			{
				get
				{
					return (this.fileAccess & FileAccess.Write) == FileAccess.Write;
				}
			}

			public override long Length
			{
				get
				{
					if (!CanSeek)
					{
						throw new NotSupportedException();
					}

					if (((this.fileShare & FileShare.Write) == FileShare.Write && (this.readBytesLeft == 0) && (this.writeResponsesQueue.TaskState != TaskState.Running))
						|| this.length == -1)
					{
						this.length = LoadLength();
					}
					
					return this.length;
				}
			}
			private long length = -1;

			protected virtual long LoadLength()
			{
				StabilizeClientState();

				var response = this.client.SendCommand(1, "getlength").ProcessError();
				
				var localLength = Int64.Parse(response.ResponseTuples["length"]);

				this.client.ReadReady();

				return localLength;
			}

			private bool closed = false;

			public override void Close()
			{
				lock (this)
				{
					if (!this.closed)
					{
						StabilizeClientState();

						Flush();

						OnBeforeClose(EventArgs.Empty);

						if (ActionUtils.IgnoreExceptions(delegate
						{
							this.client.SendCommand(1, "exit");

							this.client.ReadReady();

							this.closed = true;
						}) != null)
						{
							ActionUtils.IgnoreExceptions(() => this.client.Disconnect());
						}
						
						OnAfterClose(EventArgs.Empty);
					}
				}
			}

			public override long Position
			{
				get
				{
					if (!CanSeek)
					{
						throw new NotSupportedException();
					}

					return this.position;
				}
				set
				{
					if (!CanSeek)
					{
						throw new NotSupportedException();
					}

					Seek(value, SeekOrigin.Begin);
				}
			}
			private long position = 0;

			private int readBytesLeft = 0;
			
			[ThreadStatic]
			private byte[] tempBuffer;

			private void StabilizeClientState(bool forWrite = false)
			{
				lock (this.client.SyncLock)
				{
					if (this.readBytesLeft > 0)
					{
						this.client.WriteStream.Flush();

						if (this.tempBuffer == null)
						{
							this.tempBuffer = new byte[64 * 1024];
						}

						while (this.readBytesLeft > 0)
						{
							this.readBytesLeft -= this.client.ReadStream.Read(this.tempBuffer, 0, Math.Min(this.readBytesLeft, this.tempBuffer.Length));
						}

						this.client.ReadReady();
					}
				}

				if (!forWrite)
				{
					bool wait = false;

					if (this.writeResponsesQueue.TaskState == TaskState.Running)
					{
						this.writeResponsesQueue.Enqueue(delegate
						{
							this.writeResponsesQueue.Stop();
						});

						this.client.WriteStream.Flush();

						wait = true;
					}

					if (wait)
					{
						this.writeResponsesQueue.WaitForAnyTaskState(PredicateUtils.ObjectNotEquals(TaskState.Running));

						this.writeResponsesQueue.Reset();
					}
				}

				if (this.errorResponse != null)
				{
					var localErrorResponse = this.errorResponse;

					this.errorResponse = null;

					localErrorResponse.ProcessError();
				}
			}

			private CommandResponse errorResponse;
			private readonly InvocationQueue writeResponsesQueue;

			public override void Write(byte[] buffer, int offset, int count)
			{
				StabilizeClientState(true);

				lock (this.client.SyncLock)
				{					
					this.client.SendCommandWithoutResponse("write {0}", count);

					this.client.WriteStream.Write(buffer, offset, count);
					//client.WriteStream.Flush();
				}

				var dateTime = DateTime.Now;

				this.writeResponsesQueue.Enqueue(delegate
				{
					//lock (writeResponsesQueue.SyncLock)
					{
						var response = this.client.ReadResponse();

						this.client.ReadReady();

						if (response.ResponseType != ResponseCodes.OK)
						{
							if (this.errorResponse != null)
							{
								this.errorResponse = response;
							}
						}
						else if (this.writeResponsesQueue.Count == 0)
						{
							var now = DateTime.Now;

							if (now < dateTime || (now - dateTime > TimeSpan.FromSeconds(60)))
							{
								this.writeResponsesQueue.Stop();
							}
						}
					}
				});

				if (this.writeResponsesQueue.TaskState != TaskState.Running
					&& this.writeResponsesQueue.Count > 0)
				{
					this.writeResponsesQueue.TaskAsynchronisity = TaskAsynchronisity.AsyncWithSystemPoolThread;

					this.writeResponsesQueue.Start();
				}

				this.position += count;

				if (this.position > this.length)
				{
					this.length = this.position;
				}
			}

			public override void Flush()
			{
				this.client.writeStream.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				int localLength;
				int readLength;

				if (this.readBytesLeft == 0)
				{
					StabilizeClientState();

					var response = this.client.SendCommand(1, "read {0}", count).ProcessError();

					localLength = Convert.ToInt32(response.ResponseTuples["length"]);
					this.readBytesLeft = localLength;
				}
				else
				{
					localLength = this.readBytesLeft;
				}
				
				using (this.client.AquireBinaryReadContext())
				{					
					readLength = this.client.ReadStream.Read(buffer, offset, Math.Min(localLength, count));
				}

				this.readBytesLeft -= readLength;

				if (this.readBytesLeft == 0)
				{
					this.client.ReadReady();
				}

				this.position += readLength;

				if (this.position > this.length)
				{
					this.length = this.position;
				}

				return readLength;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				if ((this.fileShare & FileShare.Write) != FileShare.Write)
				{
					switch (origin)
					{
						case SeekOrigin.Begin:
							if (this.position == offset)
							{
								return this.position;
							}
							break;
						case SeekOrigin.Current:
							if (offset == 0)
							{
								return this.position;
							}
							break;
					}
				}

				StabilizeClientState();

				var response = this.client.SendCommand(1, "seek -o={0} \"{1}\"", origin.ToString().ToLower(), offset).ProcessError();

				this.position = Convert.ToInt64(response.ResponseTuples["position"]);
				this.length = Convert.ToInt64(response.ResponseTuples["length"]);
				
				this.client.ReadReady();

				return this.position;
			}

			public virtual HashValue ComputeHash(string algorithm, long offset, long length)
			{
				StabilizeClientState();

				var response = this.client.SendCommand(DefaultTryCount, @"computehash -o=""{0}"" -l=""{1}"" -a=""{2}""", offset, length, algorithm).ProcessError();

				length = Convert.ToInt64(response.ResponseTuples["length"]);
				var hash = TextConversion.FromBase64String(response.ResponseTuples["hash"]);

				this.position = Convert.ToInt64(response.ResponseTuples["stream-position"]);

				this.client.ReadReady();

				return new HashValue(hash, algorithm, offset, length);
			}

			public override void SetLength(long value)
			{
				if (!CanSeek)
				{
					throw new NotSupportedException();
				}

				StabilizeClientState();

				var response = this.client.SendCommand(1, "setlength {0}", value).ProcessError();

				this.position = Convert.ToInt64(response.ResponseTuples["position"]);
				this.length = Convert.ToInt64(response.ResponseTuples["length"]);

				this.client.ReadReady();
			}

			public virtual event EventHandler BeforeClose;

			public virtual void OnBeforeClose(EventArgs eventArgs)
			{
				if (BeforeClose != null)
				{
					BeforeClose(this, eventArgs);
				}
			}

			public virtual event EventHandler AfterClose;

			public virtual void OnAfterClose(EventArgs eventArgs)
			{
				if (AfterClose != null)
				{
					AfterClose(this, eventArgs);
				}
			}
		}
	}
}
