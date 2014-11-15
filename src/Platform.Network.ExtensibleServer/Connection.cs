using System;
using System.IO;
using System.Net.Sockets;

namespace Platform.Network.ExtensibleServer
{
	public class ConnectionEventArgs
		: EventArgs
	{
		public RunLevel RunLevel
		{
			get;
			set;
		}

		public ConnectionEventArgs(RunLevel runLevel)
		{
			this.RunLevel = runLevel;
		}
	}

	public delegate void ConnectionEventHandler(object sender, ConnectionEventArgs eventArgs);

	/// <summary>
	/// Summary description for Connection.
	/// </summary>
	public class Connection
		: AbstractTask
	{
		public event EventHandler Closed;
		public event ConnectionEventHandler RunLevelChanged;
		
		protected virtual void OnClosed(EventArgs eventArgs)
		{
			if (Closed != null)
			{
				Closed(this, eventArgs);
			}
		}

		protected virtual void OnRunLevelChanged(ConnectionEventArgs eventArgs)
		{
			if (RunLevelChanged != null)
			{
				RunLevelChanged(this, eventArgs);
			}
		}

		public virtual Socket Socket
		{
			get;
			private set;
		}

		public virtual Stream WriteStream
		{
			get { return writeStream; }
			set { writeStream = value; }
		}
		private Stream writeStream;

		public virtual Stream ReadStream
		{
			get { return readStream; }
			set { readStream = value; }
		}
		private Stream readStream;

		public virtual RunLevel RunLevel
		{
			get
			{
				return runLevel;
			}
			set
			{
				if (runLevel != null)
				{
					IDisposable disposable;

					if ((disposable = runLevel as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}

				runLevel = value;
			}
		}
		private RunLevel runLevel;

		public virtual NetworkServer NetworkServer
		{
			get;
			set;
		}

		public Connection(NetworkServer networkServer, Socket socket)
		{
			this.Socket = socket;
			this.NetworkServer = networkServer;
			readStream = new NetworkStream(socket, FileAccess.Read);
			writeStream = new NetworkStream(socket, FileAccess.Write);
		}

		public override void DoRun()
		{			
		}

		public virtual bool IsConnected
		{
			get
			{
				return Socket.Connected;
			}
		}

		private bool closeFired = false;

		public virtual void Close()
		{
			ActionUtils.IgnoreExceptions(Flush);
						
			ActionUtils.IgnoreExceptions(() => WriteStream.Close());

			ActionUtils.IgnoreExceptions(() => Socket.Close());

			if (!closeFired)
			{
				closeFired = true;

				OnClosed(EventArgs.Empty);				
			}
		}

		public virtual void Flush()
		{
			WriteStream.Flush();
		}
	}
}
