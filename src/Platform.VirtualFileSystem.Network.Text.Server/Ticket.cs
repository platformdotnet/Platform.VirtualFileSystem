using System;
using System.Net;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public abstract class Ticket
	{
		public virtual EndPoint OwnerEndPoint { get; set; }

		public virtual string TicketId { get; set; }

		public static string NewTicketId()
		{
			return Guid.NewGuid().ToString("N");
		}

		protected Ticket(FileSystemCommandConnection connection)
			: this(NewTicketId(), connection)
		{
			
		}

		protected Ticket(string ticketId, FileSystemCommandConnection connection)
		{
			this.TicketId = ticketId;
			this.OwnerEndPoint = connection.Socket.RemoteEndPoint;
		}

		public abstract void Claim(FileSystemCommandConnection connection);
	}
}
