using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("NOOP", AnyRunLevel.RunLevelName)]
	public class NOOPCommandProcessor
		: FileSystemTextCommandProcessor
	{
		public NOOPCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			Connection.WriteOk();
		}
	}
}
