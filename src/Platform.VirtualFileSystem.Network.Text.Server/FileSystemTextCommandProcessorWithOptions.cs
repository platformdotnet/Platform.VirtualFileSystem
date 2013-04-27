using System;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public abstract class FileSystemTextCommandProcessorWithOptions
		: FileSystemTextCommandProcessor
	{
		private readonly OptionsSerializer optionsSerializer;

		protected FileSystemTextCommandProcessorWithOptions(Connection connection)
			: base(connection)
		{
			var type = Type.GetType(this.GetType().FullName + "+CommandOptions");

			if (type == null)
			{
				throw new InvalidOperationException(String.Format("Type {0} does not define inner type CommandOptions", GetType().Name));
			}

			this.optionsSerializer = new OptionsSerializer(this, type);
		}

		protected FileSystemTextCommandProcessorWithOptions(Connection connection, Type optionsType)
			: base(connection)
		{
			this.optionsSerializer = new OptionsSerializer(this, optionsType);
		}

		protected virtual T LoadOptions<T>(TextCommand command)
		{
			return (T)LoadOptions(command);
		}

		protected virtual object LoadOptions(TextCommand command)
		{
			object retval;
			bool errorOccured;

			retval = this.optionsSerializer.Deserialize(command.Parameters, out errorOccured);

			if (errorOccured)
			{
				throw new ProcessNextCommandException();
			}

			return retval;
		}

		protected virtual object LoadOptions(Type optionsType, TextCommand command)
		{
			bool errorOccured;
			var serializer = new OptionsSerializer(this, optionsType);
			var retval = serializer.Deserialize(command.Parameters, out errorOccured);

			if (errorOccured)
			{
				throw new ProcessNextCommandException();
			}

			return retval;
		}
	}
}
