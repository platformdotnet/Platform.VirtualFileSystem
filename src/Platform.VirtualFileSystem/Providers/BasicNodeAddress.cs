using System;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public class BasicNodeAddress
		: AbstractNodeAddress
	{
		private readonly string rootUri;

		public BasicNodeAddress(string rootUri, string path)
			: this(StringUriUtils.GetScheme(rootUri), rootUri, path, false)
		{
		}

		public BasicNodeAddress(string scheme, string rootUri, string path)
			: this(scheme, rootUri, path, false)
		{
		}

		public BasicNodeAddress(string scheme, string rootUri, string path, bool normalized)
			: this(scheme, rootUri, path, normalized, "")
		{
		}

		public BasicNodeAddress(string scheme, string rootUri, string path, bool normalized, string query)
			: base(scheme, rootUri, query)
		{
			this.rootUri = rootUri;
		}

		public BasicNodeAddress(INodeAddress rootUri, string path)
			: this(rootUri.Scheme, rootUri.Uri, path, false)
		{
		}

		public BasicNodeAddress(System.Uri rootUri, string path)
			: this(rootUri.Scheme, rootUri.AbsoluteUri, path, false)
		{
		}

		protected override INodeAddress CreateAddress(string path, string query)
		{
			return new BasicNodeAddress(this.Scheme, this.RootUri, path, true, query);
		}

		protected override string GetRootUri()
		{
			return this.rootUri;
		}
	}
}
