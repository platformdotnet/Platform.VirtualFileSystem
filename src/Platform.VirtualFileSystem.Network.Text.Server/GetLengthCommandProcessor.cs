using System;
using System.Globalization;
using System.IO;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	[TextCommandSpecification("GETLENGTH", RandomAccessRunLevel.NAME)]
	public class GetLengthCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		protected class CommandOptions
		{
		}

		public GetLengthCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		public override void Process(Command command)
		{
			var stream = ((RandomAccessRunLevel)this.Connection.RunLevel).Stream;

			Connection.WriteOk("length", stream.Length.ToString(CultureInfo.InvariantCulture));
			Connection.Flush();
		}
	}
}
