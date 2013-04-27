using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("DELETE", "NORMAL")]
	public class DeleteCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Uri;

			[CommandLineOption("t", Required = true)]
			public string NodeType = "f";

			[CommandLineOption("r", Required = false)]
			public bool Recursive = false;
		}

		public DeleteCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var nodeType = NodeType.FromName(options.NodeType);
			var node = this.Connection.FileSystemManager.Resolve(options.Uri, nodeType);

			if (node.NodeType.Equals(NodeType.Directory))
			{
				((IDirectory)node).Delete(options.Recursive);
			}
			else
			{
				node.Delete();
			}

			Connection.WriteOk();
		}
	}
}
