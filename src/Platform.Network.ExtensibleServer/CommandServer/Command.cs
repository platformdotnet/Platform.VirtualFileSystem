using System;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public abstract class Command
		: INamed
	{
		public virtual string Name
		{
			get;
			set;
		}

		protected Command(string name)
		{
			Name = name;
		}
		
		public override string ToString()
		{
			return Convert.ToString(Name);
		}
	}
}
