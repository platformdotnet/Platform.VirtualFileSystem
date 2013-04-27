using System;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("QUIT", AnyRunLevel.RunLevelName)]
	public class QuitCommandProcessor
		: FileSystemTextCommandProcessor
	{
		public QuitCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			Connection.WriteOk();
			Connection.RunLevel = DisconnectedRunLevel.Default;
		}
	}
}
