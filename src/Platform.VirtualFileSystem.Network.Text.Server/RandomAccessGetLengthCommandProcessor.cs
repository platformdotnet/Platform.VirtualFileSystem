using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("GETLENGTH", "RANDOMACCESS")]
	public class RandomAccessGetLengthCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
		}

		public RandomAccessGetLengthCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;

			Connection.WriteOk("length", stream.Length.ToString());
			Connection.Flush();
		}
	}
}
