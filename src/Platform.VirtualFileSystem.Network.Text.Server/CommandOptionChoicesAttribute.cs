using System;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CommandOptionChoicesAttribute
		: Attribute
	{
		public string[] Choices { get; set; }

		public CommandOptionChoicesAttribute(params string[] choices)
		{
			this.Choices = choices;
		}
	}
}