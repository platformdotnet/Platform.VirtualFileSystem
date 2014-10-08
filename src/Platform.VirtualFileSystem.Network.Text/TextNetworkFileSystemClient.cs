using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams.Interactive;
using Platform.IO;
using Platform.Text;
using Platform.VirtualFileSystem.Network.Client;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text
{
	public partial class TextNetworkFileSystemClient
		: AbstractNetworkFileSystemClient
	{
		public static readonly int DefaultTryCount = 4;
		public static readonly int DefaultPort = TextNetworkProtocol.DefaultPort;
				
		protected override Stream WriteStream
		{
			get
			{
				if (this.writeStream == null)
				{
					this.writeStream = base.WriteStream;
				}

				return this.writeStream;
			}
			set
			{
				if (value != this.writeStream)
				{
					this.writeStream = new MeteringStream(value);

					this.writer = new StreamWriter(this.writeStream);
				}
			}
		}		
		private Stream writeStream;
		private TextWriter writer;

		public class DeterministicBinaryReadContext
			: IDisposable
		{
			private bool acquired = false;
			private TextNetworkFileSystemClient client;

			internal DeterministicBinaryReadContext(TextNetworkFileSystemClient client)
			{
				this.acquired = false;
				this.client = client;
			}

			internal void Aquire()
			{
				lock (this)
				{
					if (this.acquired)
					{
						throw new InvalidOperationException();
					}
										
					this.acquired = true;
					this.client.readStream.ChunkingEnabled = false;
				}

				System.Threading.Monitor.Enter(this.client.SyncLock);
			}

			internal void Release()
			{
				lock (this)
				{
					if (!this.acquired)
					{
						throw new InvalidOperationException();
					}

					this.acquired = false;
					this.client.readStream.ChunkingEnabled = true;
				}

				System.Threading.Monitor.Exit(this.client.SyncLock);
			}

			public void Dispose()
			{
				Release();
			}
		}
		private DeterministicBinaryReadContext binaryReadContext;

		public virtual DeterministicBinaryReadContext AquireBinaryReadContext()
		{
			this.binaryReadContext.Aquire();

			return this.binaryReadContext;
		}

		protected override Stream ReadStream
		{
			get
			{
				if (this.readStream == null)
				{
					this.ReadStream = base.ReadStream;
				}

				return this.readStream;
			}
			set
			{
				if (value != this.readStream)
				{
					this.readStream = new ChunkingStream(value, 1024 * 512, Encoding.ASCII.GetBytes("\n"));

					this.reader = new StreamReader(this.readStream);
				}				
			}
		}
		private TextReader reader;
		private ChunkingStream readStream;

		protected virtual TextReader Reader
		{
			get
			{
				return this.reader;
			}
		}

		protected virtual TextWriter Writer
		{
			get
			{
				return this.writer;
			}
		}

		public override void Disconnect()
		{
			try
			{
				Writer.Flush();

                base.Disconnect();
			}
			catch (IOException)
			{
			}
			catch (ObjectDisposedException)
			{
			}
		}

		~TextNetworkFileSystemClient()
		{
			ActionUtils.IgnoreExceptions(this.Disconnect);
		}

		public TextNetworkFileSystemClient(string address)
			: base(address, DefaultPort)
		{
			Initialize();
		}

		public TextNetworkFileSystemClient(string address, int port)
			: base(address, port)
		{
			Initialize();
		}

		public TextNetworkFileSystemClient(IPAddress serverAddress)
			: this(serverAddress, TextNetworkProtocol.DefaultPort)
		{
		}

		public TextNetworkFileSystemClient(IPAddress serverAddress, int port)
			: this(new IPEndPoint(serverAddress, port))
		{
		}

		public TextNetworkFileSystemClient(IPEndPoint serverEndPoint)
			: base(serverEndPoint)
		{
			Initialize();
		}

		private void Initialize()
		{
			this.binaryReadContext = new DeterministicBinaryReadContext(this);
		}

		public override void Connect()
		{
			this.TcpClient.ReceiveBufferSize = 256 * 1024;
			this.TcpClient.ReceiveTimeout = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;
			this.TcpClient.SendTimeout = (int)TimeSpan.FromSeconds(60).TotalMilliseconds;

			base.Connect();

			this.ReadStream = base.ReadStream;
			this.WriteStream = base.WriteStream;

			ReadWelcomeText();

			//NegotiateEncryption();

			Login();	
		}

		public virtual void Login()
		{
			using (this.AcquireCommandContext())
			{
				SendCommand(DefaultTryCount, "login");
			}
		}

		public virtual void ReadReady()
		{
			Exception e = null;

			while (true)
			{
				var line = ReadNextLine();

				if (line.StartsWith(ResponseCodes.READY))
				{
					break;
				}
				else if (line.StartsWith(ResponseCodes.ERROR))
				{
					e = ParseResponse(line).GetErrorException() ?? e;
				}
			}

			if (e != null)
			{
				throw e;
			}
		}

		public virtual void ReadWelcomeText(TextWriter welcomeTextWriter)
		{
			for (;;)
			{
				var line = this.reader.ReadLine();

				if (line.StartsWith(ResponseCodes.READY, StringComparison.CurrentCultureIgnoreCase))
				{
					break;
				}

				//WriteTextBlock(line);
			}
		}

		public virtual string ReadWelcomeText()
		{
			var builder = new StringBuilder(64);

			ReadWelcomeText(new StringWriter(builder));

			return builder.ToString();			
		}

		private string lastLineRead;

		protected virtual string ReadNextLine()
		{
			lock (this)
			{
				string s;

				for (;;)
				{
					s = this.reader.ReadLine();

					if (s == null)
					{
						throw new IOException();
					}

					if (s.Trim(' ', '\r', '\n').Length > 0)
					{
						break;
					}
				}

				return this.lastLineRead = s;
			}
		}

		public virtual CommandResponse ReadResponse()
		{
			return ParseResponse(ReadNextLine());
		}

		public virtual CommandResponse ParseResponse(string line)
		{
			lock (this.SyncLock)
			{
				int x;
				string s;
				string responseType;
				string responseTupleString;
				CommandResponse retval;

				s = line;

				x = s.IndexOf(' ');

				if (x < 0)
				{
					responseType = s;
					responseTupleString = "";
				}
				else
				{
					responseType = s.Substring(0, x);
					responseTupleString = s.Substring(x + 1);
				}

				if (responseType.EqualsIgnoreCase(ResponseCodes.OK)
					|| responseType.EqualsIgnoreCase(ResponseCodes.ERROR))
				{
					retval = new CommandResponse(responseType, responseTupleString);
				}
				else
				{
					throw new RemoteVirtualFileSystemException(ErrorCodes.UNEXPECTED);
				}

				return retval;
			}
		}

		public virtual CommandResponse SendCommand(int trycount, string command)
		{
			return SendCommand(trycount, command, new object[0]);
		}

		public virtual CommandResponse SendCommand(int trycount, string format, params object[] args)
		{
			lock (this.SyncLock)
			{
				var tries = trycount;
				var delay = TimeSpan.Zero;

				for (var i = 0; i < tries; i++)
				{
					try
					{
						SendCommandWithoutResponse(format, args);

						return ReadResponse();
					}
					catch (IOException)
					{
						if (i != tries - 1)
						{
							System.Threading.Thread.Sleep(delay);

							if (delay == TimeSpan.Zero)
							{
								delay = TimeSpan.FromMilliseconds(50);
							}
							else
							{
								delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
							}

							if (delay > TimeSpan.FromSeconds(5))
							{
								delay = TimeSpan.FromSeconds(5);
							}

							Reconnect();

							continue;
						}
						else
						{
							throw;
						}
					}
					catch
					{
					}
				}

				throw new ApplicationException("Unreachable state");
			}
		}

		public virtual void SendCommandWithoutResponse(string command)
		{
			WriteTextBlock(command);
		}

		public virtual void SendCommandWithoutResponse(string format, params object[] args)
		{
			WriteTextBlock(format, args);
		}

		public virtual void WriteTextBlock(string block)
		{
			this.writer.Write(block);
			this.writer.Write("\r\n");
			this.writer.Flush();
		}

		public virtual void WriteTextBlock(string format, params object[] args)
		{
			string s;

			s = String.Format(format, args);

			this.writer.Write(String.Format(format, args));
			this.writer.Write("\r\n");
			this.writer.Flush();
		}

		#region CommandContext

		protected class CommandContext
			: IDisposable
		{
			private readonly bool readReady;
			private readonly TextNetworkFileSystemClient client;

			internal CommandContext(TextNetworkFileSystemClient client, bool readReady, bool aquire)
			{
				this.client = client;
				this.readReady = readReady;

				if (aquire)
				{
					Aquire();
				}
			}

			public virtual void Aquire()
			{
				System.Threading.Monitor.Enter(this.client);
			}

			public virtual void Dispose()
			{
				try
				{
					if (this.readReady)
					{
						try
						{
							this.client.ReadReady();
						}
						catch (TextNetworkProtocolException)
						{
							this.client.connected = false;

							throw;
						}
						catch (IOException)
						{
							this.client.connected = false;
						}
					}
				}
				finally
				{
					System.Threading.Monitor.Exit(this.client);
				}
			}
		}

		protected virtual CommandContext AcquireCommandContext()
		{
			return this.AcquireCommandContext(true);
		}

		protected virtual CommandContext AcquireCommandContext(bool readReadyAtEnd)
		{
			return new CommandContext(this, readReadyAtEnd, true);
		}

		#endregion

		public virtual void NegotiateEncryption()
		{
			using (this.AcquireCommandContext())
			{
				var response = this.SendCommand(DefaultTryCount, "ADHOCENCRYPTION -w -compress");

				response.ProcessError();

				this.ReadStream = new InteractiveInflaterInputStream(base.ReadStream, new Inflater(true), 1024 * 256);
				this.WriteStream = new InteractiveDeflaterOutputStream(base.WriteStream, new Deflater(Deflater.BEST_COMPRESSION, true), 512);

				WriteTextBlock(ResponseCodes.READY);				
			}
		}

		public override bool Connected
		{
			get
			{
				return this.connected ?? base.Connected;
			}
		}
		private bool? connected;

		public override void Delete(string uri, NodeType nodeType, bool recursive)
		{
			using (this.AcquireCommandContext())
			{
				try
				{
					SendCommand(DefaultTryCount, "delete -t={0} {1} \"{2}\"", TextNetworkProtocol.GetNodeTypeName(nodeType), recursive ? "-r" : "", uri).ProcessError();
				}
				catch (Exception)
				{
					this.connected = false;

					throw;
				}
			}
		}

		public override void Create(string uri, NodeType nodeType, bool createParent)
		{
			using (this.AcquireCommandContext())
			{
				try
				{
					SendCommand(DefaultTryCount, "create -t={0} {1} \"{2}\"", TextNetworkProtocol.GetNodeTypeName(nodeType), createParent && nodeType.IsLikeDirectory ? "-p" : "", uri).ProcessError();
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}
		}

		public override void Move(string srcUri, string desUri, NodeType nodeType, bool overwrite)
		{
			using (this.AcquireCommandContext())
			{
				try
				{
					SendCommand(DefaultTryCount, "move -t={0} -o={1} \"{2}\" \"{3}\"", TextNetworkProtocol.GetNodeTypeName(nodeType), overwrite, srcUri, desUri).ProcessError();
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}
		}

		public override void Copy(string srcUri, string desUri, NodeType nodeType, bool overwrite)
		{
			using (this.AcquireCommandContext())
			{
				try
				{
					SendCommand(DefaultTryCount, "copy -t={0} -o={1} \"{2}\" \"{3}\"", TextNetworkProtocol.GetNodeTypeName(nodeType), overwrite, srcUri, desUri).ProcessError();
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}
		}

		public override IEnumerable<Pair<string, NodeType>> List(string uri, string regex)
		{
			Predicate<string> acceptName = null;

			using (this.AcquireCommandContext(false))
			{
				try
				{
					try
					{
						if (!String.IsNullOrEmpty(regex))
						{
							SendCommand(DefaultTryCount, @"list -regex=""{0}"" ""{1}""", TextConversion.ToEscapedHexString(regex), uri).ProcessError();
						}
						else
						{
							SendCommand(DefaultTryCount, @"list ""{0}""", uri).ProcessError();

							acceptName = PredicateUtils.NewRegex(regex);
						}				
					}
					catch
					{
						ReadReady();

						throw;
					}
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}

				for (; ; )
				{
					string line;
					NodeType currentNodeType;
					Pair<string, string> currentFile;

					try
					{
						line = TextConversion.FromEscapedHexString(ReadNextLine());
					}
					catch (TextNetworkProtocolException)
					{
						this.connected = false;

						throw;
					}
					catch (IOException)
					{
						this.connected = false;

						throw;
					}

					if (line.EqualsIgnoreCase(ResponseCodes.READY))
					{
						break;
					}

					currentFile = line.SplitAroundFirstCharFromLeft(':');

				    currentFile.Right = TextConversion.FromEscapedHexString(currentFile.Right);

					currentNodeType = TextNetworkProtocol.GetNodeType(currentFile.Left);

					if (currentNodeType == null || currentFile.Right.Length == 0)
					{
						continue;
					}

					if (acceptName != null)
					{
						if (acceptName(currentFile.Right))
						{
							yield return new Pair<string, NodeType>(currentFile.Right, currentNodeType);
						}
					}
					else
					{
						yield return new Pair<string, NodeType>(currentFile.Right, currentNodeType);
					}
				}
			}
		}

		#region ListAttributes

		public override IEnumerable<NetworkFileSystemEntry> ListAttributes(string uri, string regex)
		{
			Predicate<string> acceptName = null;

			using (this.AcquireCommandContext(false))
			{
				try
				{
					try
					{
						if (!String.IsNullOrEmpty(regex))
						{							
							try
							{
								SendCommand(DefaultTryCount, @"list -a -regex=""{0}"" ""{1}""", TextConversion.ToEscapedHexString(regex), uri).ProcessError();
							}
							catch (TextNetworkProtocolErrorResponseException)
							{
								ReadReady();

								SendCommand(DefaultTryCount, @"list -a ""{1}""", regex, uri).ProcessError();

								acceptName = PredicateUtils.NewRegex(regex);
							}
						}
						else
						{
							SendCommand(DefaultTryCount, @"list -a ""{0}""", uri).ProcessError();
						}						
					}
					catch
					{
						ReadReady();

						throw;
					}
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}

				var enumerator = TextNetworkProtocol.ReadEntries(this.ReadNextLine).GetEnumerator();

				try
				{
					for (; ; )
					{

						try
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
						}
						catch (TextNetworkProtocolException)
						{
							this.connected = false;

							throw;
						}
						catch (IOException)
						{
							this.connected = false;

							throw;
						}

						if (enumerator.Current.Right != null)
						{
							CommandResponse response;

							response = ParseResponse(enumerator.Current.Right);

							response.ProcessError();
						}

						if (acceptName != null)
						{
							if (acceptName(enumerator.Current.Left.Name))
							{
								yield return enumerator.Current.Left;
							}
						}
						else
						{
							yield return enumerator.Current.Left;
						}
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}

		public override void CreateHardLink(string srcUri, string desUri, bool overwrite)
		{
			using (this.AcquireCommandContext())
			{
				try
				{
					this.SendCommand(DefaultTryCount, "createhardlink -o={0} \"{1}\" \"{2}\"", overwrite.ToString(), srcUri, desUri).ProcessError();
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}			
		}

		#endregion

		public override Stream OpenRandomAccessStream(string uri, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			long length;
			CommandResponse response;

			using (this.AcquireCommandContext())
			{
				try
				{
					response = SendCommand
					(
						DefaultTryCount,
						"randomaccess -mode=\"{0}\" -share=\"{1}\" -access=\"{2}\" \"{3}\"",
						fileMode,
						fileShare,
						fileAccess,
						uri
					);

					response.ProcessError();
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}

			var canseek = Convert.ToBoolean(response.ResponseTuples["canseek"]);

			if (canseek)
			{
				length = Convert.ToInt64(response.ResponseTuples["length"]);
			}
			else
			{
				length = -1;
			}

			return new TextRandomAccessNetworkFileSystemStream(this, fileAccess, fileShare, canseek, length);
		}

		#region BufferedFileSystemNetworkStream
		private class BufferedFileSystemNetworkStream
			: StreamWrapper, IStreamWithEvents
		{
			public TextRandomAccessNetworkFileSystemStream NetworkFileSystemStream
			{
				get
				{
					return m_NetworkFileSystemStream;
				}
			}
			private TextRandomAccessNetworkFileSystemStream m_NetworkFileSystemStream;

			public BufferedFileSystemNetworkStream(TextRandomAccessNetworkFileSystemStream stream, int bufferSize)
				: base(new BufferedStream(stream, bufferSize))
			{
				m_NetworkFileSystemStream = stream;

				m_NetworkFileSystemStream.AfterClose += delegate(object sender, EventArgs eventArgs)
				{
					OnAfterClose(eventArgs);
				};

				m_NetworkFileSystemStream.BeforeClose += delegate(object sender, EventArgs eventArgs)
				{
					OnBeforeClose(eventArgs);
				};
			}

			#region IStreamWithEvents Members

			public virtual event EventHandler AfterClose;

			public virtual void OnAfterClose(EventArgs eventArgs)
			{
				if (AfterClose != null)
				{
					AfterClose(this, eventArgs);
				}
			}


			public virtual event EventHandler BeforeClose;

			public virtual void OnBeforeClose(EventArgs eventArgs)
			{
				if (BeforeClose != null)
				{
					BeforeClose(this, eventArgs);
				}
			}

			#endregion
		}
		#endregion

		public override HashValue ComputeHash(string uri, NodeType nodeType, string algorithm, bool recursive, long offset, long length, IEnumerable<string> fileAttributes, IEnumerable<string> dirAttributes)
		{
			StringBuilder dirAttributesString = null;
			StringBuilder fileAttributesString = null;
			
			using (this.AcquireCommandContext())
			{
				try
				{
					if (dirAttributes != null)
					{
						dirAttributesString = new StringBuilder();

						foreach (string s in dirAttributes)
						{
							dirAttributesString.Append(s);
							dirAttributesString.Append(',');
						}

						if (dirAttributesString.Length > 0)
						{
							dirAttributesString.Length--;
						}
						else
						{
							dirAttributesString = null;
						}
					}

					if (fileAttributes != null)
					{
						fileAttributesString = new StringBuilder();

						foreach (string s in fileAttributes)
						{
							fileAttributesString.Append(s);
							fileAttributesString.Append(',');
						}

						if (fileAttributesString.Length > 0)
						{
							fileAttributesString.Length--;
						}
						else
						{
							fileAttributesString = null;
						}
					}

					StringBuilder commandText = new StringBuilder(128);

					commandText.Append("computehash -hex");
					commandText.Append(" -t=\"").Append(TextNetworkProtocol.GetNodeTypeName(nodeType)).Append('\"');

					if (offset != 0)
					{
						commandText.Append(" -o=\"").Append(offset).Append('\"');
					}

					if (recursive)
					{
						commandText.Append(" -r");
					}

					if (length != -1)
					{
						commandText.Append(" -l=\"").Append(length).Append('\"');
					}

					if (algorithm != "md5")
					{
						commandText.Append(" -a=\"").Append(algorithm).Append('\"');
					}

					if (fileAttributesString != null)
					{
						commandText.Append(" -fileattribs=\"").Append(fileAttributesString).Append('\"');
					}

					if (dirAttributesString != null)
					{
						commandText.Append(" -dirattribs=\"").Append(dirAttributesString).Append('\"');
					}

					commandText.Append(" \"").Append(uri).Append('\"');

					var response = SendCommand(DefaultTryCount, commandText.ToString()).ProcessError();

					return new HashValue
					(
						TextConversion.FromHexString(response.ResponseTuples["hash"]),
						algorithm,
						0,
						length
					);
				}
				catch (TextNetworkProtocolException)
				{
					this.connected = false;

					throw;
				}
				catch (IOException)
				{
					this.connected = false;

					throw;
				}
			}
		}

		public override HashValue ComputeHash(Stream stream, string algorithm, long offset, long length)
		{
			var networkStream = (TextRandomAccessNetworkFileSystemStream)stream;

			return networkStream.ComputeHash(algorithm, offset, length);
		}

		public override IEnumerable<Pair<string,object>>  GetAttributes(string uri, NodeType nodeType)
		{
			using (this.AcquireCommandContext(false))
			{
				var lastReadLine = new ValueBox<string>(this.lastLineRead);
				
				try
				{
					this.SendCommand(DefaultTryCount, @"getattributes -t={0} ""{1}""", TextNetworkProtocol.GetNodeTypeName(nodeType), uri).ProcessError();
				}
				catch
				{
					ReadReady();

					throw;
				}

				foreach (var attribute in TextNetworkProtocol.ReadAttributes(this.ReadNextLine, lastReadLine))
				{
					yield return attribute;
				}
			}
		}

		public override void SetAttributes(string uri, NodeType nodeType, IEnumerable<Pair<string, object>> attributes)
		{
			using (this.AcquireCommandContext())
			{
				var attributesList = attributes.Where(x => !x.Left.EqualsIgnoreCase("length") && !x.Left.EqualsIgnoreCase("exists")).ToList();

				SendCommandWithoutResponse(@"setattributes -t={0} ""{1}""", TextNetworkProtocol.GetNodeTypeName(nodeType), uri);

				ReadResponse().ProcessError();

				try
				{
					foreach (var attribute in attributesList)
					{
						WriteTextBlock(@"{0}=""{1}:{2}""", attribute.Name, ProtocolTypes.GetTypeName(attribute.Value.GetType()), ProtocolTypes.ToEscapedString(attribute.Value));
					}
				}
				finally
				{
					WriteTextBlock(ResponseCodes.READY);
				}

				ReadResponse().ProcessError();
			}
		}

		public override TimeSpan Ping()
		{
			using (this.AcquireCommandContext())
			{
				var start = DateTime.Now;

				this.SendCommand(DefaultTryCount, "noop").ProcessError();

				return DateTime.Now - start;
			}
		}
	}
}
