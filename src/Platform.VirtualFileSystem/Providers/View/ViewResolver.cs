using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Providers.View
{
	internal class ViewResolver
		: AbstractResolver
	{
		private readonly INodeAddress parentAddress;
		private readonly ViewFileSystem viewFileSystem;

		public ViewResolver(ViewFileSystem viewFileSystem, INodeAddress parentAddress)
		{
			this.parentAddress = parentAddress;
			this.viewFileSystem = viewFileSystem;
		}

		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			return this.viewFileSystem.Resolve(this.parentAddress.ResolveAddress(name).AbsolutePath, nodeType, scope);
		}
	}
}
