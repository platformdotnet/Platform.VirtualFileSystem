namespace Platform.Network.ExtensibleServer
{
	public abstract class RunLevel
	{
		public virtual string Name
		{
			get;
			set;
		}

		protected RunLevel()
		{
			Name = GetType().Name.TrimRight("RunLevel");
		}

		protected RunLevel(string name)
		{
			Name = name;
		}

		public override string ToString()
		{
			return Name;
		}

		public virtual bool Equals(string s)
		{
			return s.EqualsIgnoreCase(this.Name);
		}

		public override bool Equals(object o)
		{
            if ((o as RunLevel) == null)
            {
            	return false;
            }

			return ((RunLevel)o).Name.EqualsIgnoreCase(this.Name);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

	}
}
