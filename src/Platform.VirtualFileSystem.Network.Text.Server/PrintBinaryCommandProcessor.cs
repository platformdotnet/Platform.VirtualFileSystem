using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("PRINTBINARY", "NORMAL")]
	public class PrintBinaryCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri;

			[CommandOption("outenc", Required = false)]
			[CommandOptionChoices("base64")]
			public string OutEncoding = "base64";
		}

		protected OptionsSerializer optionsSerializer;

		public PrintBinaryCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		[ThreadStatic]
		private static byte[] inputBuffer;

		[ThreadStatic]
		private static char[] outputBuffer;

		public override void Process(Command command)
		{
			Stream input;
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var file = this.Connection.FileSystemManager.ResolveFile(options.Uri);

			if (!file.Exists)
			{
				throw new FileNodeNotFoundException(file.Address);
			}

			using (input = file.GetContent().GetInputStream(FileShare.ReadWrite))
			{
				if (inputBuffer == null)
				{
					inputBuffer = new byte[512];
				}

				byte[] buffer = inputBuffer;

				if (outputBuffer == null)
				{
					int length;

					length = (int)(buffer.Length * (4d / 3d));
					length += length % 4;

					outputBuffer = new char[length];
				}

				var outbuffer = outputBuffer;

				var leftovers = 0;

				try
				{
					for (; ; )
					{
						var x = input.Read(buffer, leftovers, buffer.Length - leftovers);

						if (x == 0)
						{
							if (leftovers == 0)
							{
								break;
							}

							x = leftovers;
							leftovers = 0;
						}
						else if (x < 3)
						{
							leftovers = x;

							continue;
						}

						if (x >= 3)
						{
							leftovers = x % 3;

							x -= leftovers;
						}

						var y = Convert.ToBase64CharArray(buffer, 0, x, outbuffer, 0, Base64FormattingOptions.None);

						Connection.WriteTextPartialBlock(outbuffer, 0, y);

						if (leftovers != 0)
						{
							Array.Copy(buffer, x, buffer, 0, leftovers);
						}
					}
				}
				catch (Exception)
				{
					ActionUtils.IgnoreExceptions(() => this.Connection.WriteTextBlock(""));

					throw;
				}
			}

			Connection.WriteTextBlock("");
		}
	}
}
