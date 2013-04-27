using System;

namespace Platform.VirtualFileSystem
{
	public interface INodeResolutionFilter
	{
		INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache);
	}
}
