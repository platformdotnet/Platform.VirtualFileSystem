using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("GETPOSITION", RandomAccessRunLevel.NAME)]
	public class GetPositionCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
		}

		public GetPositionCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var stream = ((RandomAccessRunLevel)Connection.RunLevel).Stream;

			Connection.WriteOk("position", stream.Position.ToString());

			Connection.Flush();
		}
	}
}
