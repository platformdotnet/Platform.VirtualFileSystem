using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("SETLENGTH", RandomAccessRunLevel.NAME)]
	public class SetLengthCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public long Length;
		}

		public SetLengthCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;

			stream.SetLength(options.Length);

			Connection.WriteOk("position", stream.Position.ToString(), "length", stream.Length.ToString());
			Connection.Flush();
		}
	}
}
