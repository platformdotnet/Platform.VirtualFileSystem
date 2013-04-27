using System;
using System.Collections;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server.Text
{
	[TextCommandSpecification("CLAIM", "HANDSHAKE")]
	public class ClaimCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption("ticket", Required = true)]
			public string Ticket = "";
		}

		public ClaimCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			Ticket ticket;
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
	
			if (!Connection.NetworkServer.TryGetTicket(options.Ticket, out ticket))
			{
				this.Connection.WriteError(ErrorCodes.INVALID_VALUE, "TICKET");

				return;
			}

			Connection.NetworkServer.RemoveTicket(ticket.TicketId);

			ticket.Claim(Connection);
		}
	}
}
