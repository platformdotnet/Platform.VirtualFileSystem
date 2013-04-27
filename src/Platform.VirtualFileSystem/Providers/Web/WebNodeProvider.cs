using System;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebNodeProvider
		: AbstractMultiFileSystemNodeProvider
	{
		private static readonly string[] supportedUriSchemas = new string[]
		{
			Uri.UriSchemeHttp,
			Uri.UriSchemeHttps,
			Uri.UriSchemeFtp
		};

		public override string[] SupportedUriSchemas
		{
			get	
			{
				return supportedUriSchemas;
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

		protected override IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options)
		{
			return new WebFileSystem(rootAddress, options);
		}		
	}
}
