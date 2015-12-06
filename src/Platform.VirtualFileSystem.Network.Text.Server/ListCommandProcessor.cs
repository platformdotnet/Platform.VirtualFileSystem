using System;
using System.Collections.Generic;
using Platform.Text;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("LIST", NormalRunLevel.RunLevelName)]
	public class ListCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
			[CommandOption(0, Required = true)]
			public string Uri = "";

			[CommandOption("a", Required = false)]
			public bool IncludeAttributes = false;

			[CommandOption("regex", Required = false)]
			public string Regex = null;
		}

		public ListCommandProcessor(Connection connection)
			: base(connection, typeof(CommandOptions))
		{
		}
            
        [ThreadStatic]
        private static InvocationQueue t_InvocationQueue;

		public override void Process(Command command)
		{
            var count = 0;
			var options = this.LoadOptions<CommandOptions>((TextCommand)command);
			var dir = this.Connection.FileSystemManager.ResolveDirectory(options.Uri);
						
			dir.Refresh();

			if (options.Regex != null)
			{
				options.Regex = TextConversion.FromEscapedHexString(options.Regex);
			}

			if (!dir.Exists)
			{
				throw new DirectoryNodeNotFoundException(dir.Address);
			}

			Connection.WriteOk();

			if (options.IncludeAttributes)
			{
				Exception exception = null;
                InvocationQueue queue;

                if (t_InvocationQueue == null)
                {
                    t_InvocationQueue = new InvocationQueue();
                }
                                
                queue = t_InvocationQueue;

				queue.TaskAsynchronisity = TaskAsynchronisity.AsyncWithSystemPoolThread;

				queue.Start();

				try
				{
					IEnumerable<INode> children = null;

					if (String.IsNullOrEmpty(options.Regex))
					{
						children = dir.GetChildren();
					}
					else
					{
						children = dir.GetChildren(RegexBasedNodePredicateHelper.New(options.Regex));
					}

					foreach (var node in children)
					{
						var enclosedNode = node;

						if (queue.TaskState == TaskState.Stopped)
						{
							break;
						}

						queue.Enqueue
						(
							delegate
							{
								try
								{
									if (enclosedNode.NodeType == NodeType.Directory)
									{
										Connection.WriteTextPartialBlock("d:");
									}
									else
									{
										Connection.WriteTextPartialBlock("f:");
									}

									Connection.WriteTextBlock(TextConversion.ToEscapedHexString(enclosedNode.Name, TextConversion.IsStandardUrlEscapedChar));

									AttributesCommands.PrintAttributes(this.Connection, enclosedNode);

									count++;

									if (count % 15 == 0)
									{
										Connection.Flush();
									}
								}
								catch (Exception e)
								{
									queue.Stop();

									exception = e;
								}
							}
						);
					}

					if (queue.TaskState == TaskState.Stopped)
					{
						throw exception;
					}
				}
				finally
				{
					queue.Enqueue(queue.Stop);
										
					queue.WaitForAnyTaskState(value => value != TaskState.Running);

					queue.Reset();

					Connection.Flush();
				}
			}
			else
			{
				IEnumerable<string> children;

				if (string.IsNullOrEmpty(options.Regex))
				{
					children = dir.GetChildNames(NodeType.Directory);
				}
				else
				{
					children = dir.GetChildNames(NodeType.Directory, PredicateUtils.NewRegex(options.Regex));
				}

				foreach (var name in children)
				{
					Connection.WriteTextPartialBlock("d:");
                    Connection.WriteTextBlock(TextConversion.ToEscapedHexString(name, TextConversion.IsStandardUrlEscapedChar));
                    
                    count++;

                    if (count % 15 == 0)
                    {
                        Connection.Flush();
                    }
				}

				count = 0;

				if (string.IsNullOrEmpty(options.Regex))
				{
					children = dir.GetChildNames(NodeType.File);
				}
				else
				{
					children = dir.GetChildNames(NodeType.File, PredicateUtils.NewRegex(options.Regex));
				}

				foreach (var name in children)
				{
                    Connection.WriteTextPartialBlock("f:");
                    Connection.WriteTextBlock(TextConversion.ToEscapedHexString(name));
                    
					count++;

                    if (count % 15 == 0)
                    {
                        Connection.Flush();
                    }
				}
			}

			Connection.Flush();
		}
	}
}