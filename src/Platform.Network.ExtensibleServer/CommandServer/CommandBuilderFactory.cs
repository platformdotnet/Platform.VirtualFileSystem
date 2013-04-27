using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	/// <summary>
	/// Summary description for CommandBuilderFactory.
	/// </summary>
	public class CommandBuilderFactory
	{
		private object[] extraArgs;
		private Type commandBuilderType;

		protected CommandBuilderFactory()
		{
		}

		public CommandBuilderFactory(Type commandBuilderType, params object[] extraArgs)
		{
			if (!typeof(CommandBuilder).IsAssignableFrom(commandBuilderType))
			{
				throw new ArgumentException("Must extend CommandBuilder", "connectionType");
			}

			this.extraArgs = extraArgs;
			this.commandBuilderType = commandBuilderType;
		}

		public virtual CommandBuilder NewCommandBuilder(Connection connection)
		{
			object[] args;

			args = extraArgs.Prepend(connection);
			
			try
			{
				return (CommandBuilder)Activator.CreateInstance(commandBuilderType, args);
			}
			catch (MissingMethodException)
			{
				
			}

			return (CommandBuilder)Activator.CreateInstance(commandBuilderType, new object[0]);
		}
	}
}
