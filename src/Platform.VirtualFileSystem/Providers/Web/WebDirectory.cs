using System;
using System.Collections.Generic;
using System.IO;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebDirectory
		: AbstractDirectory
	{
		public override string Name
		{
			get
			{
				return this.Address.ShortName;
			}
		}

		private class WebDirectoryAttributes
			: DefaultNodeAttributes
		{
			public override bool Exists
			{
				get
				{
					return ((WebDirectory)this.Node).exists;
				}
			}

			public WebDirectoryAttributes(INode node)
				: base(node)
			{
			}
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new WebDirectoryAttributes(this);
		}

		public override bool SupportsActivityEvents
		{
			get
			{
				return false;
			}
		}

		private bool exists = true;
		private DateTime? creationDate;

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode mode, FileShare sharing)
		{
			long length;

			return WebFileSystem.DoGetInputStream(this, contentName, out encoding, mode, sharing, out creationDate, out exists, out length);
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			return WebFileSystem.DoGetOutputStream(this, contentName, encoding, mode, sharing);
		}

		public override INode Delete()
		{
			throw new NotSupportedException("WebDirectory.Delete()");
		}

		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			throw new NotSupportedException("WebDirectory.GetChildren(NodeType, IObjectFilter)");
		}	

		protected override IFileSystem DoCreateView(string scheme, FileSystemOptions options)
		{
			var address = ((AbstractNodeAddressWithRootPart)this.Address).CreateAsRoot(scheme);

			return new WebFileSystem(address, options);
		}

		public WebDirectory(WebFileSystem fileSystem, INodeAddress address)
			: base(fileSystem, address)
		{			
		}
	}
}