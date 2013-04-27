namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class TextCommandProcessorProvider
		: Platform.Network.ExtensibleServer.CommandServer.TextCommandProcessorProvider
	{
		public TextCommandProcessorProvider(Platform.Network.ExtensibleServer.Connection connection)
			: base(connection, typeof(TextCommandProcessorProvider).Assembly)
		{
		}
	}
}
