using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		private readonly string[] c_SupportedSchemas = new string[] { "netvfs" };

		public override string[] SupportedUriSchemas
		{
			get
			{
				return c_SupportedSchemas;
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

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options)
		{
			return new NetworkFileSystem(rootAddress, options);
		}
	}
}
