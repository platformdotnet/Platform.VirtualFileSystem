using System;
using System.Net.Sockets;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	/// <summary>
	/// Summary description for CommandConnection.
	/// </summary>
	public class CommandConnection
		: Connection
	{
		/// <summary>
		///  CommandProcessorProvider
		/// </summary>
		public virtual CommandProcessorProvider CommandProcessorProvider
		{
			get;
			private set;
		}

		/// <summary>
		///  CommandBuilder
		/// </summary>
		public virtual CommandBuilder CommandBuilder
		{
			get;
			private set;
		}

		public CommandConnection(NetworkServer networkServer, Socket socket)
			: base(networkServer, socket)
		{
			CommandBuilder = ((CommandNetworkServer)networkServer).CommandBuilderFactory.NewCommandBuilder(this);
			CommandProcessorProvider = ((CommandNetworkServer)networkServer).CommandProcessorProviderFactory.NewCommandProcessorProvider(this);
		}

		public override void Run()
		{
			try
			{
				for (;;)
				{
					try
					{
						if (this.RunLevel.Equals(DisconnectedRunLevel.Default))
						{
							ActionUtils.IgnoreExceptions(Close);
							
							return;
						}

						BuildAndProcessSingleCommand();
					}
					catch (Exception e)
					{
						UnhandledExceptionFromSingleCommand(e);
					}
				}
			}
			finally
			{
				ActionUtils.IgnoreExceptions(Close);
			}
		}

		protected virtual void UnhandledExceptionFromSingleCommand(Exception e)
		{
			throw e;
		}
		
		protected virtual void BuildAndProcessSingleCommand()
		{
			var command = CommandBuilder.BuildNextCommand();
			var commandProcessor = CommandProcessorProvider.GetCommandProcessor(command);

			commandProcessor.Process(command);
		}
	}
}
