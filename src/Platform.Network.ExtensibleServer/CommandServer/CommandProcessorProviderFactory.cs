using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public class CommandProcessorProviderFactory
	{
		private readonly object[] extraArgs;
		private readonly Type commandProcessorProviderType;

		protected CommandProcessorProviderFactory()
		{
		}

		public CommandProcessorProviderFactory(Type commandProcessorProviderType, params object[] extraArgs)
		{
			if (!typeof(CommandProcessorProvider).IsAssignableFrom(commandProcessorProviderType))
			{
				throw new ArgumentException("Must extend CommandProcessorProvider", "connectionType");
			}

			this.extraArgs = extraArgs;
			this.commandProcessorProviderType = commandProcessorProviderType;
		}

		public virtual CommandProcessorProvider NewCommandProcessorProvider(Connection connection)
		{
			var args = extraArgs.Prepend(connection);

			try
			{
				return (CommandProcessorProvider)Activator.CreateInstance(commandProcessorProviderType, args);
			}
			catch (MissingMethodException)
			{
			}

			return (CommandProcessorProvider)Activator.CreateInstance(commandProcessorProviderType, new object[0]);
		}
	}
}
