using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("MOVE", "NORMAL")]
	public class MoveCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Src = "";

			[CommandOption(1, Required = true)]
			public string Des = "";

			[CommandOption("t", Required = true)]
			public string NodeType = null;
						
			[CommandOption("o", Required = false)]
			public bool Overwrite;
		}

		public MoveCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var nodeType = NodeType.FromName(options.NodeType);
			var src = this.Connection.FileSystemManager.Resolve(options.Src, nodeType);
			var des = this.Connection.FileSystemManager.Resolve(options.Des, nodeType);

			src.MoveTo(des, options.Overwrite);

			Connection.WriteOk();
			Connection.Flush();
		}
	}
}
