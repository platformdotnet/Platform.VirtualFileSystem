using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Platform;
using Platform.IO;
using Platform.Text;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class FileSystemCommandConnection
		: CommandConnection, ITextConnection
	{
		public new FileSystemNetworkServer NetworkServer
		{
			get
			{
				return (FileSystemNetworkServer)base.NetworkServer;
			}
			set
			{
				base.NetworkServer = value;
			}
		}

		public virtual IFileSystemManager FileSystemManager { get; private set; }

		public virtual Stream RawReadStream
        {
            get
            {
				if (this.rawReadStream == null)
				{
					var stream = base.ReadStream;

					this.rawReadStream = new MeteringStream(base.ReadStream);

					this.rawReadStream.ReadMeter.ValueChanged += delegate(object sender, MeterEventArgs eventArgs)
					{
						
					};
				}

				return this.rawReadStream;
            }
        }
		private MeteringStream rawReadStream;

        public virtual Stream RawWriteStream
        {
            get
            {
				if (this.rawWriteStream == null)
				{					
					this.rawWriteStream = new MeteringStream(base.WriteStream);

					this.rawWriteStream.WriteMeter.ValueChanged += delegate(object sender, MeterEventArgs eventArgs)
					{
						
					};
				}

				return this.rawWriteStream;
            }
        }
		private MeteringStream rawWriteStream;

		public override Stream ReadStream
		{
			get
			{
				if (this.readStream == null)
				{
					this.ReadStream = this.RawWriteStream;
				}

				return this.readStream;
			}
			set
			{
				if (value != this.readStream)
				{					
					this.readStream = new ChunkingStream(value, 4096, Encoding.ASCII.GetBytes("\n"));

					this.readStream.ReadMeter.ValueChanged += delegate(object sender, MeterEventArgs eventArgs)
					{
						var ratio = ((double)(long)this.rawReadStream.ReadMeter.Value) / ((double)(long)eventArgs.Value) * 100.0d;

						ProtocolTrafficLogger.LogTraffic
						(
							String.Format("type=read, compressed={0}, uncompressed={1}, compressed-percent={2:F2}%", this.rawReadStream.ReadMeter.Value, eventArgs.Value, ratio)
						);
					};

					this.reader = new StreamReader(this.readStream);
				}
			}
		}
		private ChunkingStream readStream = null;

		private TextReader reader;

		public override Stream WriteStream
		{
			get
			{
				if (this.writeStream == null)
				{
					this.WriteStream = this.RawWriteStream;
				}

				return this.writeStream;
			}
			set
			{
				if (value != this.writeStream)
				{
					this.writeStream = new MeteringStream(value);

					this.writeStream.WriteMeter.ValueChanged += delegate(object sender, MeterEventArgs eventArgs)
					{
						double ratio;

						ratio = ((double)(long)this.rawWriteStream.WriteMeter.Value) / ((double)(long)eventArgs.Value) * 100.0d;

						ProtocolTrafficLogger.LogTraffic
						(
							String.Format("type=write, compressed={0}, uncompressed={1}, compressed-percent={2:F2}%", this.rawWriteStream.WriteMeter.Value, eventArgs.Value, ratio)
						);
					};

					this.Writer = new StreamWriter(this.writeStream, new UTF8Encoding(false));
				}
			}
		}
		private MeteringStream writeStream = null;

		public class DeterministicBinaryReadContext
			: IDisposable
		{
			private bool acquired = false;
			private readonly FileSystemCommandConnection connection;

			internal DeterministicBinaryReadContext(FileSystemCommandConnection connection)
			{
				this.acquired = false;
				this.connection = connection;				
			}

			internal void Aquire()
			{
				if (this.acquired)
				{
					throw new InvalidOperationException();
				}

				this.acquired = true;
				this.connection.readStream.ChunkingEnabled = false;
			}

			internal void Release()
			{
				if (!this.acquired)
				{
					throw new InvalidOperationException();
				}

				this.acquired = false;
				this.connection.readStream.ChunkingEnabled = true;
			}

			public void Dispose()
			{
				Release();
			}
		}

		private readonly DeterministicBinaryReadContext binaryReadContext;

		public virtual DeterministicBinaryReadContext AquireBinaryReadContext()
		{
			this.binaryReadContext.Aquire();

			return this.binaryReadContext;
		}

		public virtual IDictionary<object, object> ConnectionState
		{
			get
			{
				return this.connectionState;
			}
		}
		private readonly IDictionary<object, object> connectionState = new Dictionary<object, object>();

		public virtual TextWriter Writer { get; private set; }

		protected ProtocolReadLog ProtocolReadLogger { get; private set; }

		protected ProtocolWriteLog ProtocolWriteLogger { get; private set; }

		public virtual void WriteTextPartialBlock(char[] buffer, int index, int count)
		{
			if (ProtocolWriteLogger.IsEnabled)
			{
				ProtocolWriteLogger.LogWritePartial(buffer, index, count);
			}

			this.Writer.Write(buffer, index, count);
		}

		public virtual void WriteTextPartialBlock(string text)
		{
			if (ProtocolWriteLogger.IsEnabled)
			{
				ProtocolWriteLogger.LogWritePartial(text);
			}
			
			this.Writer.Write(text);
		}

		public virtual void WriteTextBlock(string text)
		{
			if (ProtocolWriteLogger.IsEnabled)
			{
				ProtocolWriteLogger.LogWrite(text);
			}

			this.Writer.Write(text);
			this.Writer.Write("\r\n");
		}

		public virtual void WriteTextBlock(string format, params object[] args)
		{
			if (ProtocolWriteLogger.IsEnabled)
			{
				ProtocolWriteLogger.LogWrite(format, args);
			}

			this.Writer.Write(format, args);
			this.Writer.Write("\r\n");
		}

		public virtual string ReadTextBlock()
		{
			bool overflow;

			var retval = this.reader.ReadLine(10 * 1024 * 1024, out overflow);
			retval = retval.Trim(' ', '\r', '\n');

			if (overflow)
			{
				throw new OverflowException("Input Too Long");
			}

			if (ProtocolReadLogger.IsEnabled)
			{
				ProtocolReadLogger.LogRead(retval);
			}

			return retval;
		}

		private static int connectionIdCount = 0;

		private readonly int connectionId;

		public ProtocolTrafficLog ProtocolTrafficLogger { get; private set; }

		public FileSystemCommandConnection(NetworkServer networkServer, Socket socket, IFileSystemManager fileSystemManager)
			: base(networkServer, socket)
		{
			this.connectionId = Interlocked.Increment(ref connectionIdCount);

			this.ProtocolReadLogger = new ProtocolReadLog(this.connectionId.ToString());
			this.ProtocolWriteLogger = new ProtocolWriteLog(this.connectionId.ToString());
			this.ProtocolTrafficLogger = new ProtocolTrafficLog(this.connectionId.ToString());
			
			this.FileSystemManager = fileSystemManager;

            this.ReadStream = RawReadStream;
			this.WriteStream = RawWriteStream;

			System.Reflection.AssemblyName name;

			name = GetType().Assembly.GetName();

			WriteTextBlock("NETVFS {0}.{1}", name.Version.Major, name.Version.Minor);
			
			this.binaryReadContext = new DeterministicBinaryReadContext(this);
		}

		public override void Run()
		{
			this.RunLevel = HandshakeRunLevel.Default;

			base.Run();
		}

		public virtual void WriteOk()
		{
			WriteTextBlock(ResponseCodes.OK);
		}

		public virtual void WriteOk(params object[] extraArgs)
		{
			if (extraArgs.Length % 2 != 0)
			{
				throw new ArgumentException();
			}

			if (extraArgs.Length > 0)
			{
				var text = new StringBuilder(255);

				text.Append(ResponseCodes.OK);

				if (extraArgs.Length > 0)
				{
					text.Append(' ');

					for (var i = 0; i < extraArgs.Length; i += 2)
					{
						text.Append(extraArgs[i].ToString());
						text.Append('=');
						text.Append('\"');
						text.Append(extraArgs[i + 1].ToString());
						text.Append('\"');

						text.Append(" ");
					}

					text.Length--;
				}

				WriteTextBlock(text.ToString());
			}
			else
			{
				WriteTextBlock(ResponseCodes.OK);
			}

			Flush();
		}

		public override void Flush()
		{
            try
            {
                this.Writer.Flush();

                base.Flush();
            }
            catch (IOException)
            {
                throw;
            }
            catch (Exception)
            {
                this.RunLevel = DisconnectedRunLevel.Default;
            }
		}

		public virtual void WriteReady()
		{
			WriteTextBlock(ResponseCodes.READY);
			Flush();
		}

		public virtual void WriteError(string errorCode)
		{
			WriteError(errorCode, "");
		}

		public virtual void WriteError(string errorCode, string messageFormat, params object[] args)
		{
			var s = String.Format(messageFormat, args);

			if (s.Length > 0)
			{
				WriteTextBlock("{0} CODE={1} MESSAGE=\"{2}\"", ResponseCodes.ERROR, errorCode, TextConversion.ToEscapedHexString(s));
			}
			else
			{
				WriteTextBlock("{0} CODE={1}", ResponseCodes.ERROR, errorCode);
			}

			Flush();
		}

		public virtual void ReadReady()
		{
			string s = this.ReadTextBlock();

			if (!s.EqualsIgnoreCase(ResponseCodes.READY))
			{
				throw new FileSystemServerException(ErrorCodes.UNEXPECTED_COMMAND, s);
			}
		}

		protected override void BuildAndProcessSingleCommand()
		{
			WriteReady();

			base.BuildAndProcessSingleCommand();
		}

		protected override void UnhandledExceptionFromSingleCommand(Exception e)
		{
			try
			{
				Predicate<char> isEscapeChar = (c) => c == '\n' || c == '\"' || c == '\r';

				if (e is DirectoryNodeNotFoundException)
				{
					WriteTextBlock("{0} CODE={1} URI=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.DIRECTORY_NOT_FOUND, ((NodeNotFoundException)e).NodeAddress.Uri);

					Flush();
				}
				else if (e is FileNodeNotFoundException)
				{
					WriteTextBlock("{0} CODE={1} URI=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.FILE_NOT_FOUND, ((NodeNotFoundException)e).NodeAddress.Uri);

					Flush();
				}
				else if (e is NodeNotFoundException)
				{
					WriteTextBlock("{0} CODE={1} URI=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.FILE_NOT_FOUND, ((NodeNotFoundException)e).NodeAddress.Uri);

					Flush();
				}
				else if (e is ObjectDisposedException)
				{
					this.RunLevel = DisconnectedRunLevel.Default;
				}
				else if (e is IOException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}: {3}\"",
						ResponseCodes.ERROR, ErrorCodes.IO_ERROR, e.GetType().Name, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is FileSystemServerException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ((FileSystemServerException)e).ErrorCode, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is CommandNotSupportedException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.INVALID_COMMAND, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is ProcessNextCommandException)
				{
					return;	
				}
				else if (e is MalformedUriException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.MALFORMED_URI, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is NotSupportedException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.NOT_SUPPORTED, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is UnauthorizedAccessException)
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.UNAUTHORISED, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();
				}
				else if (e is DisconnectedException)
				{
					RunLevel = DisconnectedRunLevel.Default;
				}
				else
				{
					WriteTextBlock("{0} CODE={1} DETAILS=\"{2}\"",
						ResponseCodes.ERROR, ErrorCodes.UNEXPECTED, TextConversion.ToEscapedHexString(e.ToString(), isEscapeChar));

					Flush();

					RunLevel = DisconnectedRunLevel.Default;
				}
			}
			catch (IOException)
			{
				RunLevel = DisconnectedRunLevel.Default;
			}
		}
	}
}
