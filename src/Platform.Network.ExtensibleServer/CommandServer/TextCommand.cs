namespace Platform.Network.ExtensibleServer.CommandServer
{
	public class TextCommand
		: Command
	{
		public override string Name
		{
			get;
			set;
		}

		public virtual string Parameters
		{
			get;
			set;
		}

		public TextCommand(string name, string parameters)
			: base(name)
		{
			this.Name = name;
			this.Parameters = parameters;
		}
	}
}
