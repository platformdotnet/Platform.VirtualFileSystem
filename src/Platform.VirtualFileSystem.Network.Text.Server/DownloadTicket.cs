using System.Globalization;
using System.IO;
using Platform.IO;
using Platform.Network.ExtensibleServer;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class DownloadTicket
		: Ticket
	{
		private readonly IFile file;
		private readonly int offset;
		private readonly int length;
		private readonly FileShare fileShare;

		public DownloadTicket(FileSystemCommandConnection connection, IFile file, FileShare fileShare, int offset, int length)
			: base(connection)
		{
			this.file = file;
			this.offset = offset;
			this.length = length;
			this.fileShare = fileShare;
		}

		public override void Claim(FileSystemCommandConnection connection)
		{
			if (!connection.Socket.RemoteEndPoint.Equals(this.OwnerEndPoint))
			{
				connection.WriteError(ErrorCodes.INVALID_PARAM, "ticket");

				connection.RunLevel = DisconnectedRunLevel.Default;

				return;
			}

			var src = this.file.GetContent().GetInputStream(this.fileShare);

			if (this.offset > 0 || this.length != -1)
			{
				src = new PartialStream(src, this.offset, this.length);
			}
			
			connection.WriteOk("length", src.Length.ToString(CultureInfo.InvariantCulture));
			connection.Flush();
			
			var des = connection.WriteStream;

			var pump = new StreamCopier(src, des);
        
			pump.Run();

			connection.RunLevel = DisconnectedRunLevel.Default;
		}
	}
}
