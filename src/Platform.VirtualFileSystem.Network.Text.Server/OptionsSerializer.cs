using System;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class OptionsSerializer
		: CommandOptionsSerializer
	{
		private readonly CommandProcessor commandProcessor;

		public OptionsSerializer(CommandProcessor commandProcessor, Type type)
			: base(type)
		{
			this.commandProcessor = commandProcessor;
		}

		public virtual object Deserialize(string commandLine, out bool errorOccured)
		{
			var localErrorOccured = false;

			var retval = this.Parse(commandLine, (sender, eventArgs) =>
			{
				if (localErrorOccured)
				{
					return;
				}

				var connection = (FileSystemCommandConnection)this.commandProcessor.Connection;

				switch (eventArgs.Error)
				{
					case CommandLineError.InvalidValue:
						connection.WriteError(ErrorCodes.INVALID_PARAM, eventArgs.OptionName);
						break;
					case CommandLineError.MissingOption:
						connection.WriteError(ErrorCodes.MISSING_PARAMS, eventArgs.OptionName);
						break;
					case CommandLineError.TooManyOptions:
						connection.WriteError(ErrorCodes.TOO_MANY_PARAMS, eventArgs.OptionName);
						break;
					case CommandLineError.UnknownOption:
						connection.WriteError(ErrorCodes.INVALID_PARAM, eventArgs.OptionName);
						break;
					default:
						connection.WriteError(ErrorCodes.UNEXPECTED, eventArgs.Error.ToString());
						break;
				}

				localErrorOccured = true;
			});

			errorOccured = localErrorOccured;

			return retval;
		}
	}
}
