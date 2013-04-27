using System;
using System.Collections.Generic;

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
					return true;
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