using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public abstract class FileSystemTextCommandProcessor
		: TextCommandProcessor
	{
		public new FileSystemCommandConnection Connection
		{
			get
			{
				return (FileSystemCommandConnection)base.Connection;
			}
		}

		protected FileSystemTextCommandProcessor(Connection connection)
			: base(connection)
		{
		}
	}
}
