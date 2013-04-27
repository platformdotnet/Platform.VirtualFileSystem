using System;
using System.Threading;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("LOGIN", "HANDSHAKE")]
	public class LoginCommandProcessor
		: FileSystemTextCommandProcessor
	{
		private static int loginCount = 0;

		public LoginCommandProcessor(Connection connection)
			: base(connection)
		{
			
		}

		public override void Process(Command command)
		{
			if (Interlocked.Increment(ref loginCount) % 8 == 0)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			Connection.WriteOk("message", "LOGIN SUCCESSFUL");
			
			Connection.RunLevel = NormalRunLevel.Default;
		}
	}
}
