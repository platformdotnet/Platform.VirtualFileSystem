namespace Platform.Network.ExtensibleServer.CommandServer
{
	public abstract class CommandProcessorProvider
	{
		public virtual Connection Connection
		{
			get;
			set;
		}

		protected CommandProcessorProvider(Connection connection)
		{
			this.Connection = connection;
		}

		public abstract CommandProcessor GetCommandProcessor(Command command);
	}
}
