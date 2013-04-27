using System;

namespace Platform.VirtualFileSystem.Providers.Shadow
{
	public class ShadowNodeProvider
		: AbstractNodeProvider
	{
		private static readonly string[] supportedUriSchemas = new string[] { "shadow" };

		public override string[] SupportedUriSchemas
		{
			get
			{
				return supportedUriSchemas;
			}
		}

		private IFileSystem TempFileSystem
		{
			get
			{
				if (this.tempFileSystem == null)
				{
					lock (this)
					{
						if (this.tempFileSystem == null)
						{
							IFileSystem tempFileSystem;

							tempFileSystem = Manager.ResolveDirectory("temp:///").FileSystem;

							System.Threading.Thread.MemoryBarrier();

							this.tempFileSystem = tempFileSystem;
						}
					}
				}

				return this.tempFileSystem;
			}
		}
		private IFileSystem tempFileSystem;

		public override bool SupportsUri(string uri)
		{
			return uri.StartsWith("shadow", StringComparison.CurrentCultureIgnoreCase);
		}

		public ShadowNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		public override INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options)
		{
			var address = LayeredNodeAddress.Parse(uri);

			if (address.Port >= 0 || address.UserName != "" || address.Password != "" || address.ServerName != "")
			{
				throw new MalformedUriException(uri, "Network & Authentication information not permitted");
			}

			if (nodeType.Is(typeof(IFile)))
			{
				var uri2 = address.InnerUri;

				if (StringUriUtils.ContainsQuery(uri2))
				{
					uri2 += "&shadow=true";
				}
				else
				{
					uri2 += "?shadow=true";
				}

				return resolver.Resolve(uri2, nodeType);
			}

			return resolver.Resolve(address.InnerUri);
		}
	}
}
