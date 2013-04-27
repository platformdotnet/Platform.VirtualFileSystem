using System;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("EXIT", RandomAccessRunLevel.NAME)]
	public class ExitCommandProcessor
		: FileSystemTextCommandProcessor
	{
		public ExitCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			Connection.WriteOk();
			Connection.Flush();
			Connection.RunLevel = NormalRunLevel.Default;
		}
	}
}
