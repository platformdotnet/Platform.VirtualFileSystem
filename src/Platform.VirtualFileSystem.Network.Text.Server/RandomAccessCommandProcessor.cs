using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.Collections;
using Platform.Utilities;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;


namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("RANDOMACCESS", "NORMAL")]
	public class RandomAccessCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri;

			[CommandOption(Required = false)]
			[CommandOptionChoices("read", "write", "readwrite", "none")]
			public string Share = "readwrite";

			[CommandOption(Required = false)]
			[CommandOptionChoices("read", "write", "readwrite", "none")]
			public string Access = "readwrite";

			[CommandOption(Required = false)]
			[CommandOptionChoices("append", "create", "createnew", "open", "openorcreate", "truncate")]
			public string Mode = "openorcreate";

			[CommandOption("w", Required = false)]
			public bool WaitForReady = false;
		}

		public RandomAccessCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);

			var file = this.Connection.FileSystemManager.ResolveFile(options.Uri);

			var content = file.GetContent();

			Stream stream = null;

			for (int i = 0; i < 16; i++)
			{
				try
				{
					stream =
						content.OpenStream((FileMode) Enum.Parse(typeof(FileMode), options.Mode, true),
						                   (FileAccess) Enum.Parse(typeof(FileAccess), options.Access, true),
						                   (FileShare) Enum.Parse(typeof(FileShare), options.Share, true));

					break;
				}
				catch (Exception)
				{
					if (i == 9)
					{
						throw;
					}

					System.Threading.Thread.Sleep(150);
				}
			}

			EventHandler closeStream = (sender, eventArgs) => ActionUtils.IgnoreExceptions(delegate
			{
				if (stream != null)
				{
					stream.Close();
				}
			});

			try
			{
				IList<object> list = new List<object>();

				Connection.RunLevel = new RandomAccessRunLevel(file, stream);

				this.Connection.Closed += closeStream;

				list.Add("canread");
				list.Add(stream.CanRead);
				list.Add("canwrite");
				list.Add(stream.CanWrite);
				list.Add("canseek");
				list.Add(stream.CanSeek);
				list.Add("sharing");
				list.Add(options.Share);
				
				if (stream.CanSeek)
				{
					list.Add("length");
					list.Add(stream.Length);
				}

				Connection.WriteOk
				(
					list.ToArray()		
				);
								
				if (options.WaitForReady)
				{
					Connection.Flush();
					Connection.ReadReady();
				}
			}
			catch
			{
				throw;
			}
		}
	}
}
