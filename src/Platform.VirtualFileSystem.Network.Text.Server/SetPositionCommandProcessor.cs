using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("SETPOSITION", RandomAccessRunLevel.NAME)]
	public class SetPositionCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public long Position = 0;
		}

		public SetPositionCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			if (options.Position < 0)
			{
				Connection.WriteError(ErrorCodes.INVALID_PARAM, "POSITION");

				return;
			}
            
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;

			stream.Position = options.Position;

			Connection.WriteOk("position", stream.Position.ToString());
			Connection.Flush();
		}
	}
}
