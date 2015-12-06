using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("LISTCOMMANDS", AnyRunLevel.RunLevelName)]
	public class ListCommandsCommandProcessor
		: FileSystemTextCommandProcessor
	{
		public ListCommandsCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{			
			Connection.WriteOk();

			foreach (var s in ((TextCommandProcessorProvider)Connection.CommandProcessorProvider).GetCommandNames().Sorted())
			{
				Connection.WriteTextBlock(s);
			}
		}
	}
}
