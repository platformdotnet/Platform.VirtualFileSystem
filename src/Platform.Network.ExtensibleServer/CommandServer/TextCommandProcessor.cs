using System.Collections;

namespace Platform.Network.ExtensibleServer.CommandServer
{
	/// <summary>
	/// Summary description for SimpleCommandProcessor.
	/// </summary>
	public abstract class TextCommandProcessor
		: CommandProcessor
	{
		private IList supportedNames;
		private TextCommandSpecificationAttribute[] attributes;

		protected TextCommandProcessor(Connection connection)
			: base(connection)
		{
			IList list;
			
			attributes = (TextCommandSpecificationAttribute[])GetType().GetCustomAttributes(typeof(TextCommandSpecificationAttribute), true);

			list = new ArrayList(attributes.Length);

			supportedNames = ArrayList.ReadOnly(list);

			foreach (TextCommandSpecificationAttribute attribute in attributes)
			{
				list.Add(attribute.CommandName);
			}
		}
        
		public virtual IList GetNames()
		{
			return supportedNames;
		}

		public virtual bool SupportsRunLevel(RunLevel runLevel)
		{
			foreach (var attribute in attributes)
			{
				foreach (string s in attribute.RunLevels)
				{
					if (AnyRunLevel.Default.Equals(s) || runLevel.Equals(s))
					{
						return true;
					}
				}
			}

			return false;
		}

		public override bool SupportsCommand(Command command)
		{
			if (!(command is TextCommand))
			{
				return false;
			}

			foreach (var attribute in attributes)
			{
				if (attribute.CommandName.EqualsIgnoreCase((command).Name))
				{
					foreach (string s in attribute.RunLevels)
					{
						if (AnyRunLevel.Default.Equals(s) || Connection.RunLevel.Equals(s))
						{
							return true;
						}
					}
				}
			}

			return false;
		}
	}
}
