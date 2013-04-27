using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("R", RandomAccessRunLevel.NAME)]
	[TextCommandSpecification("READ", RandomAccessRunLevel.NAME)]
	public class ReadCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public int Length;

			[CommandLineOption("w", Required = false)]
			public bool WaitForReady = false;
		}

		[ThreadStatic]
		private static byte[] inputBuffer;

		public ReadCommandProcessor(Connection connection)
			: base(connection)
		{			
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			if (options.Length < 0)
			{
				Connection.WriteError(ErrorCodes.INVALID_VALUE, "LENGTH");

				return;
			}

			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;

			if (inputBuffer == null)
			{
				inputBuffer = new byte[Math.Max(1024 * 1024, options.Length)];
			}
			else if (inputBuffer.Length < options.Length
				&& inputBuffer.Length < 1024 * 1024)
			{
				inputBuffer = new byte[Math.Max(1024 * 1024, options.Length)];
			}

			var x = stream.Read(inputBuffer, 0, Math.Min(options.Length, inputBuffer.Length));

			Connection.WriteOk("length", x.ToString());
									
			if (options.WaitForReady)
			{
				Connection.Flush();
				Connection.ReadReady();
			}

			Connection.WriteStream.Write(inputBuffer, 0, x);
			Connection.WriteStream.Flush();
						
			Connection.Flush();			
		}
	}
}
