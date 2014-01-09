using System;

namespace Platform.VirtualFileSystem
{
	public class PathKeyedNodeCache
		: WeakReferenceBasedNodeCache<Pair<string, NodeType>>
	{
		public PathKeyedNodeCache()
			: base()
		{
		}

		public PathKeyedNodeCache(int maxsize)
			: base(maxsize)
		{
		}

		protected override Pair<string, NodeType> GetKey(INodeAddress address, NodeType nodeType)
		{
			return new Pair<string, NodeType>(address.PathAndQuery, nodeType);
		}
	}
}