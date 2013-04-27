using System;
using System.Collections;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("SEEK", RandomAccessRunLevel.NAME)]
	public class SeekCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption("o", Required = false)]
			[CommandLineOptionChoices("begin", "current", "end")]
			public string SeekOrigin = "begin";

			[CommandLineOption(0, Required = true)]
			public long Position;
		}

		public SeekCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;
			var origin = (SeekOrigin)Enum.Parse(typeof(SeekOrigin), options.SeekOrigin, true);
						
			stream.Seek(options.Position, origin);

			Connection.WriteOk("position", stream.Position.ToString(), "length", stream.Length.ToString());
			
			Connection.Flush();
		}
	}
}
