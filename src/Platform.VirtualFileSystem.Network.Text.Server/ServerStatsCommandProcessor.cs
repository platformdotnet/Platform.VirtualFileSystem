using System;
using System.IO;
using System.Collections.Generic;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Text;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("SERVERSTATS", NormalRunLevel.RunLevelName)]
	public class ServerStatsCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(Required = false)]
			public bool ConnectionCount = false;			
		}

		public ServerStatsCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var list = new List<string>();
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			if (options.ConnectionCount)
			{
				list.Add("connectioncount");
				list.Add(Connection.NetworkServer.ConnectionCount.ToString());
			}
			
			Connection.WriteOk(list.ToArray());

			Connection.Flush();
		}
	}
}
