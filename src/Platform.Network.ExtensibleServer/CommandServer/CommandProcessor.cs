namespace Platform.Network.ExtensibleServer.CommandServer
{
	public abstract class CommandProcessor
	{
		public virtual Connection Connection
		{
			get;
			set;
		}

		protected CommandProcessor(Connection connection)
		{
			this.Connection = connection;
		}

		public abstract void Process(Command command);
		public abstract bool SupportsCommand(Command command);
	}
}
