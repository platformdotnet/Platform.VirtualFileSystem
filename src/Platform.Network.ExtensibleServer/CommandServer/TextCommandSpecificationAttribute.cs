using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TextCommandSpecificationAttribute
		: Attribute
	{
		public virtual string CommandName
		{
			get;
			set;
		}

		public virtual string[] RunLevels
		{
			get;
			set;
		}

		public TextCommandSpecificationAttribute(string commandName, params string[] runLevels)
		{
			this.CommandName = commandName;
			this.RunLevels = runLevels;
		}
	}
}
