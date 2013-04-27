using System;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractNodeService
		: AbstractService, INodeService
	{
		protected AbstractNodeService(INode operatingNode)
		{
			this.OperatingNode = operatingNode;
		}

		public virtual INode OperatingNode { get; private set; }
	}
}
