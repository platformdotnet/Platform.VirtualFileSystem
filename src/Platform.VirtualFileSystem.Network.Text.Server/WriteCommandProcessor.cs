using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("W", RandomAccessRunLevel.NAME)]
	[TextCommandSpecification("WRITE", RandomAccessRunLevel.NAME)]
	public class WriteCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public int Length;
		}

		[ThreadStatic]
		private static byte[] inputBuffer;

		public WriteCommandProcessor(Connection connection)
			: base(connection)
		{			
		}

		public override void Process(Command command)
		{
			Exception exception = null;

			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			if (options.Length < 0)
			{
				Connection.WriteError(ErrorCodes.INVALID_PARAM, "LENGTH");

				return;
			}

			if (inputBuffer == null)
			{
				inputBuffer = new byte[8192];
			}

			Connection.WriteOk();
			Connection.Flush();
			
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;
			var toread = options.Length;

			using (Connection.AquireBinaryReadContext())
			{
				while (true)
				{
					if (toread == 0)
					{
						break;
					}

					var x = Connection.ReadStream.Read(inputBuffer, 0, Math.Min(inputBuffer.Length, toread));

					if (x == 0)
					{
						throw new DisconnectedException();
					}

					try
					{
						stream.Write(inputBuffer, 0, x);
					}
					catch (IOException e)
					{
						exception = e;
					}

					toread -= x;
				}
			}

			stream.Flush();

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
