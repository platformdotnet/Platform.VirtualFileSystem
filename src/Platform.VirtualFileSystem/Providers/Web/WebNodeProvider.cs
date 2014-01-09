using System;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		public override string[] SupportedUriSchemas
		{
			get
			{
				return new []
				{
					Uri.UriSchemeHttp,
					Uri.UriSchemeHttps,
					Uri.UriSchemeFtp
				};
			}
		}

		public WebNodeProvider(IFileSystemManager manager)
			: base(manager)
		{		
		}

		protected override INodeAddress ParseUri(string uri)
		{
			return StandardNodeAddress.Parse(uri);
		}

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache)
		{
			cache = true;

			return new WebFileSystem(rootAddress, options);
		}		
	}
}
