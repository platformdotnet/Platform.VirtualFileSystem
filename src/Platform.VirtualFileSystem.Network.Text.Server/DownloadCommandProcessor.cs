using System;
using System.Collections;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("DOWNLOAD", "NORMAL")]
	public class DownloadCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Uri;

			[CommandLineOption(Required = false)]
			public int Offset = 0;

			[CommandLineOption(Required = false)]
			public int Length = -1;

			[CommandLineOption(Required = false)]
			[CommandLineOptionChoices("read", "write", "readwrite", "none")]
			public string Share = "readwrite";
		}

		protected OptionsSerializer optionsSerializer;

		public DownloadCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var file = this.Connection.FileSystemManager.ResolveFile(options.Uri);
			var ticket = new DownloadTicket(this.Connection, file, (FileShare)Enum.Parse(typeof(FileShare), options.Share, true),  options.Offset, options.Length);

			Connection.NetworkServer.AddTicket(ticket);
			
			Connection.WriteTextBlock("OK TICKET={0}", ticket.TicketId);
			Connection.Flush();
		}
	}
}
