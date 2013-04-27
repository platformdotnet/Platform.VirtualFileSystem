using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("CREATEHARDLINK", "NORMAL")]
	public class CreateHardLinkCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Path = "";

			[CommandLineOption(1, Required = true)]
			public string Target = "";

			[CommandLineOption("o", Required = false)]
			public bool Overwrite;
		}

		public CreateHardLinkCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = this.LoadOptions<CommandOptions>((TextCommand)command);

			if (StringUtils.IsNullOrEmpty(options.Path))
			{
				Connection.WriteError(ErrorCodes.INVALID_VALUE, "sourceuri");
			}

			if (StringUtils.IsNullOrEmpty(options.Target))
			{
				Connection.WriteError(ErrorCodes.INVALID_VALUE, "targeturi");
			}

			var src = this.Connection.FileSystemManager.ResolveFile(options.Path);
			var target = this.Connection.FileSystemManager.ResolveFile(options.Target);

			src.CreateAsHardLink(target, options.Overwrite);

			Connection.WriteOk();
			Connection.Flush();
		}
	}
}
