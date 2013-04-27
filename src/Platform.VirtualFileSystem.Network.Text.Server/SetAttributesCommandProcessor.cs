using System;
using System.IO;
using System.Text;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	/// <summary>
	/// This is a test.
	/// </summary>
	[TextCommandSpecification("SETATTRIBUTES", "NORMAL")]
	public class SetAttributesCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Uri;

			[CommandLineOption("t", Required = true)]
			public string NodeType = "Any";
		}

		public SetAttributesCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		private static bool NodeTypeSupported(NodeType nodeType)
		{
			return nodeType == NodeType.File || nodeType == NodeType.Directory;
		}

		public override void Process(Command command)
		{
			Exception e = null;

			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var nodeType = NodeType.FromName(options.NodeType);
			var node = this.Connection.FileSystemManager.Resolve(options.Uri, nodeType);

			node.Refresh();

			if (!node.Exists)
			{
				throw new NodeNotFoundException(node.Address);
			}

			if (!NodeTypeSupported(node.NodeType))
			{
				Connection.WriteError(ErrorCodes.INVALID_PARAM);

				return;
			}

			Connection.WriteOk();
			Connection.Flush();
												
			using (node.Attributes.AquireUpdateContext())
			{
				var lastLineRead = new ValueBox<string>();

				foreach (var attribute in TextNetworkProtocol.ReadAttributes(Connection.ReadTextBlock, lastLineRead))
				{
					var attributeName = attribute.Name;
					var attributeValue = attribute.Value;

					if (!(attributeName.EqualsIgnoreCase("exists") || attributeName.EqualsIgnoreCase("length")))
					{
						e = ActionUtils.IgnoreExceptions(delegate
						{
							node.Attributes[attributeName] = attributeValue;
						});
					}
				}
			}

			if (e != null)
			{
				throw e;
			}

			this.Connection.WriteOk();
		}
	}
}
