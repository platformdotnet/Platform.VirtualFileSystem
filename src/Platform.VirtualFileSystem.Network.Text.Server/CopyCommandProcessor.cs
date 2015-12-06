using System;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("COPY", "NORMAL")]
	public class CopyCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Src = "";

			[CommandOption(1, Required = true)]
			public string Des = "";

			[CommandOption("o", Required = false)]
			public bool Overwrite;

			[CommandOption("bsz", Required = false)]
			public int BufferSize = 1024 * 64;

			[CommandOption("t", Required = true)]
			public string NodeType = "f";

			[CommandOption("m", Required = false)]
			public bool Monitor = false;
		}
				
		public CopyCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var options = (CommandOptions)this.LoadOptions((TextCommand)command);
			var nodeType = NodeType.FromName(options.NodeType);
			var src = this.Connection.FileSystemManager.Resolve(options.Src, nodeType);
			var des = this.Connection.FileSystemManager.Resolve(options.Des, nodeType);

			if (options.Monitor)
			{
				var x = 0;
				var service = (INodeCopyingService)src.GetService(new NodeCopyingServiceType(des, options.Overwrite, options.BufferSize));

				service.Progress.ValueChanged += delegate
				{
					Connection.WriteTextBlock("{0} {1}", service.Progress.CurrentValue, service.Progress.MaximumValue);

					if (x % 8 == 0)
					{
						Connection.Flush();
					}

					x++;
				};

				int okwritten = 0;

				Connection.WriteOk();

				Action routine = delegate
				{
					service.Run();
					this.Connection.Flush();

					//
					// Only write OK if the main thread hasn't printed OK/CANCEL
					// in response to an a CANCEL request.
					//

					if (System.Threading.Interlocked.CompareExchange(ref okwritten, 1, 0) == 0)
					{
						this.Connection.WriteOk();
						this.Connection.Flush();
					}
				};

				var result = routine.BeginInvoke(null, null);

				//
				// Read the special CANCEL and READY commands.
				//

				var cancelled = false;

				for (;;)
				{
					var line = this.Connection.ReadTextBlock();

					if (line == null)
					{
						this.Connection.RunLevel = DisconnectedRunLevel.Default;

						return;
					}

					if (line.StartsWith(ResponseCodes.READY, StringComparison.CurrentCultureIgnoreCase))
					{
						//
						// READY tells us to process new commands.
						//

						break;
					}
					else if (line.StartsWith(ResponseCodes.CANCEL, StringComparison.CurrentCultureIgnoreCase))
					{
						//
						// CANCEL tells us to cancel the operation.
						//

						// Cancel the operation

						if (cancelled)
						{
							continue;
						}

						service.Stop();

						// Write OK/CANCELED if the operation thread hasn't already

						if (System.Threading.Interlocked.CompareExchange(ref okwritten, 1, 0) == 0)
						{
							if (service.TaskState == Platform.TaskState.Finished)
							{
								Connection.WriteOk();
							}
							else
							{
								Connection.WriteError(ErrorCodes.CANCELLED);
							}

							Connection.Flush();
						}

						// Wait for the operation thread to finish

						routine.EndInvoke(result);

						cancelled = true;
					}
					else
					{
						this.Connection.RunLevel = DisconnectedRunLevel.Default;

						return;
					}
				}

				Connection.Flush();
			}
			else
			{
				// Perform the operation

				Connection.WriteOk();
				
				src.CopyTo(des, options.Overwrite);
			}			
		}
	}
}
