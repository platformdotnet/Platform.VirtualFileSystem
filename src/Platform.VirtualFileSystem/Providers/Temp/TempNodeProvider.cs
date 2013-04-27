using System.IO;

namespace Platform.VirtualFileSystem.Providers.Temp
{
	public class TempNodeProvider
		: View.ViewNodeProvider
	{
		public new class Configuration
			: View.ViewNodeProvider.Configuration
		{
			public Configuration()
			{
				this.Create = true;
				this.Scheme = "temp";
			}

			public Configuration(string scheme, string uri, bool create)
				: base(scheme, uri, create)
			{
			}
		}

		public TempNodeProvider(IFileSystemManager manager)
			: base(manager, "temp", manager.ResolveDirectory(Path.GetTempPath()).Create(true))
		{
		}

		public TempNodeProvider(IFileSystemManager manager, string scheme, IDirectory root)
			: base(manager, scheme, root)
		{
		}

		public TempNodeProvider(IFileSystemManager manager, string scheme, IDirectory root, FileSystemOptions options)
			: base(manager, scheme, root, options)
		{
		}

		public TempNodeProvider(IFileSystemManager manager, Configuration options)
			: base(manager, options)
		{
		}
	}
}
