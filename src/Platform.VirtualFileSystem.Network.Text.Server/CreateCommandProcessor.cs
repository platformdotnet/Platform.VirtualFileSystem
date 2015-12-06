using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("CREATE", "NORMAL")]
	public class CreateCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri;

			[CommandOption("t", Required = true)]
			public string NodeType = "f";

			[CommandOption("p", Required = false)]
			public bool CreateParent = false;
		}

		public CreateCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = this.LoadOptions<CommandOptions>((TextCommand)command);

			if (StringUtils.IsNullOrEmpty(options.Uri))
			{
				Connection.WriteError(ErrorCodes.INVALID_VALUE, "uri");
			}

			var node = this.Connection.FileSystemManager.Resolve(options.Uri, NodeType.FromName(options.NodeType));

			node.Create(options.CreateParent);

			Connection.WriteOk();
			Connection.Flush();
		}
	}
}
