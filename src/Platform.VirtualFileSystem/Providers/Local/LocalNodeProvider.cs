using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers.Local
{
	public class LocalNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		public class Configuration
			: NodeProviderConfiguration
		{
		}

		public override string[] SupportedUriSchemas
		{
			get
			{
				return new[] { "file" };;
			}
		}

		private readonly Configuration configuration;
		private readonly Dictionary<FileSystemOptions, FileSystemOptions> modifiedFileSystemOptions = new Dictionary<FileSystemOptions, FileSystemOptions>();

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
			FileSystemOptions modifiedOptions;

			if (!modifiedFileSystemOptions.TryGetValue(options, out modifiedOptions))
			{
				modifiedOptions = options.CreateWithAdditionalConfig(this.configuration);

				modifiedFileSystemOptions[options] = modifiedOptions;
			}

			cache = true;

			return new LocalFileSystem(rootAddress, modifiedOptions);
		}
	}
}
