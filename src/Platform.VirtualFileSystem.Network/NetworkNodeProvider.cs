using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		public override string[] SupportedUriSchemas
		{
			get
			{
				return new [] { "netvfs" };;
			}
		}

		public NetworkNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		protected override INodeAddress ParseUri(string uri)
		{
			return NetworkNodeAddress.Parse(uri);
		}

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache)
		{
			cache = true;

			return new NetworkFileSystem(rootAddress, options);
		}
	}
}
