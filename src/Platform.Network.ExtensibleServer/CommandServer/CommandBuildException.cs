using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	/// <summary>
	/// Summary description for CommandBuildException.
	/// </summary>
	public class CommandBuildException
		: Exception
	{
		public CommandBuildException()
		{
			
		}

		public CommandBuildException(string message)
			: base(message)
		{
		}
	}
}
