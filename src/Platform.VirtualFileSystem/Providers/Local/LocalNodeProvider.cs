namespace Platform.VirtualFileSystem.Providers.Local
{
	public class LocalNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		private static readonly string[] supportedUriSchemas = new string[] { "file" };

		public class Configuration
			: NodeProviderConfiguration
		{
		}

		public override string[] SupportedUriSchemas
		{
			get
			{
				return supportedUriSchemas;
			}
		}

		private readonly Configuration configuration;

		public LocalNodeProvider(IFileSystemManager manager)
			: this(manager, new Configuration())
		{
		}

		public LocalNodeProvider(IFileSystemManager manager, Configuration config)
			: base(manager)
		{
			this.configuration = config;
		}

		public override bool SupportsUri(string uri)
		{
			if (LocalNodeAddress.CanParse(uri))
			{
				return true;
			}

			return base.SupportsUri(uri);
		}

		protected override INodeAddress ParseUri(string uri)
		{
			return LocalNodeAddress.Parse(uri);
		}

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache)
		{
			options.AddNodeProviderConfiguration(this.configuration);

			cache = true;

			return new LocalFileSystem(rootAddress, options);
		}
	}
}
