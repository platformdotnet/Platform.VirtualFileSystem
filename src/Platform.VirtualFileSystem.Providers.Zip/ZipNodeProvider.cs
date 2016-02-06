using System.Collections.Specialized;
using System.Linq;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ZipNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		public override string[] SupportedUriSchemas => new[] { "zip" };

		public ZipNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		protected override INodeAddress ParseUri(string uri)
		{
			return LayeredNodeAddress.Parse(uri);
		}

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache)
		{
			var backingFile = this.Manager.ResolveFile(((LayeredNodeAddress)rootAddress).InnerUri);

			cache = options.ReadOnly;

			return new ZipFileSystem(rootAddress, backingFile, this.AmmendOptionsFromAddress(backingFile.Address, options));
		}
	}
}
