using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public class CommandNotSupportedException
		: NotSupportedException
	{
		public virtual Command Command
		{
			get;
			set;
		}

		public CommandNotSupportedException()
			: this(null)
		{
			
		}

		public CommandNotSupportedException(Command command)
			: base(command.ToString())
		{
			Command = command;
		}
	}
}
