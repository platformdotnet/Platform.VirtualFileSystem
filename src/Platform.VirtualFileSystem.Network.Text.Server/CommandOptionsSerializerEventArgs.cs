using System;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public sealed class CommandOptionsSerializerEventArgs
		: EventArgs
	{
		public string OptionName { get; set; }
		public string OptionValue { get; set; }
		public CommandLineError Error { get; set; }

		public CommandOptionsSerializerEventArgs(CommandLineError error, string optionName)
			: this(error, optionName, "")
		{
		}

		public CommandOptionsSerializerEventArgs(CommandLineError error, string optionName, string optionvalue)
		{
			this.Error = error;
			this.OptionName = optionName;
			this.OptionValue = optionvalue;
		}
	}
}