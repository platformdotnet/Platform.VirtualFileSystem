using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("GETATTRIBUTES", "NORMAL")]
	public class GetAttributesCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri;

			[CommandOption("t", Required = true)]
			public string NodeType = "Any";
		}

		public GetAttributesCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		private static bool NodeTypeSupported(NodeType nodeType)
		{
			return nodeType == NodeType.File || nodeType == NodeType.Directory;
		}

		private static void PrintNodeType(FileSystemCommandConnection connection, NodeType nodeType)
		{
			if (nodeType == NodeType.File)
			{
				connection.WriteTextPartialBlock("f");
			}
			else if (nodeType == NodeType.Directory)
			{
				connection.WriteTextPartialBlock("d");
			}
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var nodeType = NodeType.FromName(options.NodeType);
			var node = this.Connection.FileSystemManager.Resolve(options.Uri, nodeType);

			node.Refresh();

			this.Connection.WriteOk();

			foreach (Pair<string, object> attribute in node.Attributes)
			{
				if (attribute.Value != null)
				{
					Connection.WriteTextBlock(@"{0}=""{1}:{2}""", attribute.Name, ProtocolTypes.GetTypeName(attribute.Value.GetType()), ProtocolTypes.ToEscapedString(attribute.Value));
				}
			}
		}
	}
}
