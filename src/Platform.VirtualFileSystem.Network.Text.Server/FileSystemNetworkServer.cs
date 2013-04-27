using System;
using System.Collections;
using System.Net;
using Platform;
using Platform.Utilities;
using Platform.Collections;
using Platform.VirtualFileSystem.Network.Server;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class FileSystemNetworkServer
		: CommandNetworkServer, INetworkFileSystemServer
	{
		public virtual ILDictionary<string, Ticket> Tickets { get; private set; }

		public virtual void AddTicket(Ticket ticket)
		{
			lock (this.Tickets.SyncLock)
			{
				this.Tickets[ticket.TicketId] = ticket;
			}
		}

		public virtual void RemoveTicket(string ticketId)
		{
			lock (this.Tickets.SyncLock)
			{
				this.Tickets.Remove(ticketId);
			}
		}

		public virtual bool TryGetTicket(string ticketId, out Ticket ticket)
		{
			lock (this.Tickets.SyncLock)
			{
				if (this.Tickets.TryGetValue(ticketId, out ticket))
				{
					return true;
				}
			}

			ticket = null;

			return false;
		}

		private void Initialize()
		{
			this.Tickets = new TimedReferenceDictionary<string, Ticket>
				(
					TimeSpan.FromMinutes(15),
					typeof(LinearHashDictionary<,>)
				);
		}

		public FileSystemNetworkServer(IFileSystemManager fileSystemManager, int port)
			: this(new ConnectionFactory(typeof(FileSystemCommandConnection), fileSystemManager),
					new CommandBuilderFactory(typeof(TextCommandBuilder)),
					new CommandProcessorProviderFactory(typeof(TextCommandProcessorProvider), typeof(FileSystemNetworkServer).Assembly),
					port)
		{
		}

		public FileSystemNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, int port)
			: base(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, port)
		{
			Initialize();
		}

		public FileSystemNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, string address, int port)
			: base(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, address, port)
		{
			Initialize();
		}

		public FileSystemNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, IPAddress address, int port)
			: base(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, address, port)
		{
			Initialize();
		}

		public FileSystemNetworkServer(ConnectionFactory connectionFactory, CommandBuilderFactory commandBuilderFactory, CommandProcessorProviderFactory commandProcessorProviderFactory, IPEndPoint localEndPoint)
			: base(connectionFactory, commandBuilderFactory, commandProcessorProviderFactory, localEndPoint)
		{
			Initialize();
		}
	}
}
