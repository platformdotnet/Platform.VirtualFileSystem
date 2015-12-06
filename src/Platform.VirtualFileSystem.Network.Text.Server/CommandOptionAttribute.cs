using System;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class CommandOptionAttribute
		: Attribute
	{
		public Type Type { get; set; }
		public string Name { get; set; }
		public bool Required { get; set; }
		public int UnnamedOptionIndex { get; set; }

		public CommandOptionAttribute()
			: this((Type)null)
		{
		}

		public CommandOptionAttribute(Type type)
			: this("", type)
		{

		}

		public CommandOptionAttribute(int unnamedOptionIndex)
			: this(unnamedOptionIndex, null)
		{

		}

		public CommandOptionAttribute(string name)
			: this(name, null)
		{

		}

		public CommandOptionAttribute(int unnamedOptionIndex, Type type)
			: this(type)
		{
			this.UnnamedOptionIndex = unnamedOptionIndex;
		}

		public CommandOptionAttribute(string name, Type type)
		{
			this.Name = name;
			this.Type = type;
			this.Required = true;
			this.UnnamedOptionIndex = -1;
		}
	}
}

