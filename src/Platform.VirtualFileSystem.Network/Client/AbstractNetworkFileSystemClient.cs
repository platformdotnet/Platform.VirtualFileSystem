using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Platform.VirtualFileSystem.Network.Client
{
	public abstract class AbstractNetworkFileSystemClient
		: INetworkFileSystemClient
	{
		protected TcpClient TcpClient { get; private set; }
		protected virtual Stream ReadStream
		{
			get  { return this.readStream; }
			set { this.readStream = value; }
		}

		protected virtual Stream WriteStream
		{
			get { return this.writeStream; }
			set { this.writeStream = value; }
		}

		private readonly int port;
		private readonly string hostName;
		private Stream readStream;
		private Stream writeStream;

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		protected AbstractNetworkFileSystemClient(string hostname, int port)
		{
			if (hostname == null)
			{
				throw new ArgumentNullException("hostname");
			}

			this.port = port;
			this.hostName = hostname;

			this.TcpClient = new TcpClient();
		}

		protected AbstractNetworkFileSystemClient(IPEndPoint serverEndPoint)
		{
			this.ServerEndPoint = serverEndPoint;

			this.TcpClient = new TcpClient();
		}

		public abstract void Create(string uri, NodeType nodeType, bool createParent);

		public virtual bool Connected
		{
			get
			{
				return this.TcpClient.Connected;
			}
		}

		public virtual IPEndPoint ServerEndPoint { get; private set; }

		public virtual void Reconnect()
		{
			this.ReadStream.Close();
			this.WriteStream.Close();

			this.TcpClient = new TcpClient();

			Connect();
		}

		public virtual void Connect()
		{
			if (this.Connected)
			{
				throw new InvalidOperationException();
			}

			if (this.ServerEndPoint != null)
			{
				this.TcpClient.Connect(this.ServerEndPoint);
			}
			else
			{
				this.TcpClient.Connect(this.hostName, this.port);
			}
									
			this.readStream = this.TcpClient.GetStream();
			this.writeStream = this.TcpClient.GetStream();
		}

		public virtual void Disconnect()
		{
			if (this.ReadStream != null)
			{
				this.ReadStream.Close();
			}

			if (this.WriteStream != null)
			{
				this.WriteStream.Close();
			}

			this.TcpClient.Client.Disconnect(false);

			this.TcpClient.Close();
		}

		public abstract void Delete(string uri, NodeType nodeType, bool recursive);

		public abstract void Move(string srcUri, string desUri, NodeType nodeType, bool overwrite);

		public abstract void Copy(string srcUri, string desUri, NodeType nodeType, bool overwrite);

		public abstract IEnumerable<Pair<string, NodeType>> List(string uri, string regex);

		public abstract IEnumerable<NetworkFileSystemEntry> ListAttributes(string uri, string regex);

		public virtual void Dispose()
		{
			try
			{
				this.Disconnect();
			}
			catch
			{
			}
		}

		public abstract Stream OpenRandomAccessStream(string uri, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

		public abstract void CreateHardLink(string srcUri, string desUri, bool overwrite);

		public abstract TimeSpan Ping();

		public abstract HashValue ComputeHash(string uri, NodeType nodeType, string algorithm, bool recursive, long offset, long length, IEnumerable<string> fileAttributes, IEnumerable<string> dirAttributes);

		public abstract HashValue ComputeHash(Stream stream, string algorithm, long offset, long length);

		public abstract IEnumerable<Pair<string, object>> GetAttributes(string uri, NodeType nodeType);

		public abstract void SetAttributes(string uri, NodeType nodeType, IEnumerable<Pair<string, object>> attributes);
	}
}
