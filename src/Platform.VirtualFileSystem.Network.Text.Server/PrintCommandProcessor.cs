using System;
using System.IO;
using System.Text;
using Platform.Text;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("PRINT", "NORMAL")]
	public class PrintCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri;

			[CommandOption("outenc", Required = false)]
			[CommandOptionChoices("none", "url")]
			public string Encoding = "none";

			[CommandOption("txtenc", Required = false)]
			[CommandOptionChoices("utf-8", "utf-16", "ascii")]
			public string TextEncoding = "utf-8";
		}

		protected OptionsSerializer optionsSerializer;

		public PrintCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		[ThreadStatic]
		private static char[] intputBuffer;

		public override void Process(Command command)
		{
			TextReader reader;

			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			var file = this.Connection.FileSystemManager.ResolveFile(options.Uri);

			if (!file.Exists)
			{
				throw new FileNodeNotFoundException(file.Address);
			}

			if (intputBuffer == null)
			{
				intputBuffer = new char[256];
			}

			var buffer = intputBuffer;

			var encoding = Encoding.GetEncoding(options.TextEncoding);

			Connection.WriteOk();

			var noencoding = options.Encoding.Equals("none", StringComparison.CurrentCultureIgnoreCase);

			Console.WriteLine("PRINT 1");

			Console.WriteLine("file: " + file);
			Console.WriteLine("content: " + file.GetContent().GetType());
			Console.WriteLine("reader: " + file.GetContent().GetReader(FileShare.ReadWrite));

			using (reader = file.GetContent().GetReader(FileShare.ReadWrite))
			{
				Console.WriteLine("PRINT 2");

				while (true)
				{
					var x = reader.ReadBlock(buffer, 0, buffer.Length);

					Console.WriteLine("PRINT 3 " + x);

					if (x == 0)
					{
						break;
					}

					if (noencoding)
					{
						Connection.WriteTextPartialBlock(buffer, 0, x);
					}
					else
					{
						TextConversion.WriteEscapedHexString
						(
							new CharArrayReader(buffer, 0, x),
							Connection.Writer,
							c => c == '<' || c == '>' || c == '\r' || c == '\n'
							);
					}

					Connection.Flush();
				}
			}

			Console.WriteLine("PRINT END");

			Connection.WriteTextBlock("");
		}		
	}
}
