using System;

namespace Platform.Utilities
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CommandLineOptionChoicesAttribute
		: Attribute
	{
		/// <summary>
		///  Choices
		/// </summary>
		public virtual string[] Choices
		{
			get
			{
				return choices;
			}
			set
			{
				choices = value;
			}
		}
		/// <summary>
		/// <see cref="Choices"/>
		/// </summary>
		private string[] choices;

		public CommandLineOptionChoicesAttribute(params string[] choices)
		{
			this.choices = choices;	
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CommandLineOptionAttribute
		: Attribute
	{
		/// <summary>
		///  Required
		/// </summary>
		public virtual bool Required
		{
			get;
			set;
		}

		/// <summary>
		///  Name
		/// </summary>
		public virtual string Name
		{
			get;
			set;
		}

		/// <summary>
		///  Type
		/// </summary>
		public virtual Type Type
		{
			get;
			set;
		}

		/// <summary>
		///  UnnamedOptionIndex
		/// </summary>
		public virtual int UnnamedOptionIndex
		{
			get;
			set;
		}

		public CommandLineOptionAttribute()
			: this((Type)null)
		{
			
		}

		public CommandLineOptionAttribute(Type type)
			: this("", type)
		{
			
		}

		public CommandLineOptionAttribute(int unnamedOptionIndex)
			: this(unnamedOptionIndex, null)
		{
			
		}

		public CommandLineOptionAttribute(string name)
			: this(name, null)
		{
			
		}

		public CommandLineOptionAttribute(int unnamedOptionIndex, Type type)
			: this(type)
		{
			UnnamedOptionIndex = unnamedOptionIndex;
		}

		public CommandLineOptionAttribute(string name, Type type)
		{
			this.Name = name;
			this.Type = type;
			this.Required = true;
			this.UnnamedOptionIndex = -1;
		}
	}
}
