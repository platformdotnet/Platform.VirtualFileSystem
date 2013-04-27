using System.Net;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public class CommandNetworkServer
		: NetworkServer
	{
		public virtual CommandBuilderFactory CommandBuilderFactory
		{
			get;
			set;
		}

		public virtual CommandProcessorProviderFactory CommandProcessorProviderFactory
		{
			get;
			set;
		}

		public CommandNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, int port)
			: this(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, IPAddress.Any,  port)
		{
		}

		public CommandNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, string address, int port)
			: this(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, IPAddress.Parse(address), port)
		{
		}

		public CommandNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, IPAddress address, int port)
			: this(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, new IPEndPoint(address, port))
		{
		}

		public CommandNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, IPEndPoint localEndPoint)
			: base(connectionFactory, localEndPoint)
		{
			this.CommandBuilderFactory = commandBuilderFactory;
			this.CommandProcessorProviderFactory = commandProcessorProviderFactory;
		}
	}
}
