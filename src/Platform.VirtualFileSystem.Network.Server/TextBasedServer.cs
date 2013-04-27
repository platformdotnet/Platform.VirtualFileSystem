using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.VirtualFileSystem.Network.Text.Protocol;
using Platform.VirtualFileSystem.Network.Text.Server;

namespace Platform.VirtualFileSystem.Network.Server
{
	public class TextBasedServer
	{
		public static INetworkFileSystemServer CreateServer()
		{
			return CreateServer(TextNetworkProtocol.DefaultPort);
		}

		public static INetworkFileSystemServer CreateServer(int port)
		{
			var commandProviderType = ConfigurationSection.Instance.CommandProcessorProvider;

			if (commandProviderType == null)
			{
				commandProviderType = typeof(Text.Server.TextCommandProcessorProvider);
			}

			var server = new FileSystemNetworkServer
			(
				new ConnectionFactory(typeof(FileSystemCommandConnection), FileSystemManager.Default),
				new CommandBuilderFactory(typeof(TextCommandBuilder)),
				new CommandProcessorProviderFactory(commandProviderType),
				TextNetworkProtocol.DefaultPort
			);

			return server;
		}
	}
}
