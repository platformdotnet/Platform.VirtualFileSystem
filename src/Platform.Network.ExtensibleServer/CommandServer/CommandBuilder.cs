namespace Platform.Network.ExtensibleServer.CommandServer
{
	public abstract class CommandBuilder
	{
		public virtual Connection Connection
		{
			get;
			set;
		}

		protected CommandBuilder(Connection connection)
		{
			this.Connection = connection;
		}

		public abstract Command BuildNextCommand();
	}
}
