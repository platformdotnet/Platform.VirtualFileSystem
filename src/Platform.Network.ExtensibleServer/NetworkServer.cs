using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Platform.Network.ExtensibleServer
{
	public abstract class NetworkServer
		: AbstractTask
	{
		private TcpListener tcpListener;
		private readonly IPEndPoint localEndPoint;

		public int ConnectionCount
		{
			get
			{
				return connectionCount;
			}
			set
			{
				connectionCount = value;
			}
		}
		private int connectionCount = 0;

		public virtual RunLevel RunLevel
		{
			get;
			set;
		}

		public virtual ConnectionFactory ConnectionFactory
		{
			get;
			private set;
		}

		protected NetworkServer(ConnectionFactory connectionFactory, int port)
			: this(connectionFactory, IPAddress.Any,  port)
		{
		}

		protected NetworkServer(ConnectionFactory connectionFactory, string address, int port)
			: this(connectionFactory, IPAddress.Parse(address), port)
		{
		}

		protected NetworkServer(ConnectionFactory connectionFactory, IPAddress address, int port)
			: this(connectionFactory, new IPEndPoint(address, port))
		{
		}

		protected NetworkServer(ConnectionFactory connectionFactory, IPEndPoint localEndPoint)
		{
			RunLevel = NullRunLevel.Default;
			this.localEndPoint = localEndPoint;	
			ConnectionFactory = connectionFactory;
		}

		public override void RequestTaskState(TaskState taskState)
		{
			if (taskState == TaskState.Stopped || taskState == TaskState.Finished || taskState == TaskState.Paused)
			{
				while (true)
				{
					try
					{
						tcpListener.Stop();
						tcpListener.Server.Close();
					}
					catch
					{
					}

					if (base.RequestTaskState(taskState, TimeSpan.FromSeconds(0.5)))
					{
						return;
					}

					if (taskState == TaskState.Stopped || taskState == TaskState.Finished)
					{
						return;
					}
				}
			}

			base.RequestTaskState(taskState);
		}

		public override bool RequestTaskState(TaskState taskState, TimeSpan timeout)
		{
			if (taskState == TaskState.Stopped || taskState == TaskState.Finished || taskState == TaskState.Paused)
			{
				try
				{
					tcpListener.Stop();
				}
				catch
				{
				}
			}

			return base.RequestTaskState(taskState, timeout);
		}

		public override void DoRun()
		{
			tcpListener = new TcpListener(localEndPoint);

			tcpListener.Start();

			while (true)
			{
				try
				{
					Socket socket;

					try
					{
						ProcessTaskStateRequest();

						socket = tcpListener.AcceptSocket();
					}
					catch (SocketException)
					{
						if (this.RequestedTaskState == TaskState.Stopped || this.RequestedTaskState == TaskState.Finished)
						{
							break;
						}

						try
						{
							tcpListener.Start();
						}
						catch
						{

						}

						ProcessTaskStateRequest();

						continue;
					}

					var endPoint = socket.RemoteEndPoint;

					this.ProcessTaskStateRequest();

					var connection = ConnectionFactory.NewConnection(this, socket);

					Interlocked.Increment(ref connectionCount);

					connection.Closed += ((sender, args) => Interlocked.Decrement(ref connectionCount));

					connection.RequestTaskState(TaskState.Running, TimeSpan.Zero);
				}
				catch (StopRequestedException)
				{
					throw;
				}
				catch
				{	
				}
			}
		}
	}
}
