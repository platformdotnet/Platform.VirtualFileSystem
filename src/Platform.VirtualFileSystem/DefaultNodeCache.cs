using Platform;
using Platform.Utilities;
using Platform.Collections;

namespace Platform.VirtualFileSystem
{
	public class DefaultNodeCache
		: WeakReferenceBasedNodeCache<DefaultNodeCache.DefaultNodeCacheKey>
	{
		public struct DefaultNodeCacheKey
		{			
			private readonly NodeType nodeType;
			private readonly INodeAddress address;

			public NodeType NodeType
			{
				get
				{
					return nodeType;
				}
			}

			public INodeAddress NodeAddress
			{
				get
				{
					return address;
				}
			}

			public DefaultNodeCacheKey(INodeAddress address, NodeType nodeType)
			{
				this.address = address;
				this.nodeType = nodeType;
			}

			public override string ToString()
			{
				return address.ToString();
			}

			public override int GetHashCode()
			{
				return address.PathAndQuery.GetHashCode() ^ nodeType.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var cacheKey = obj as DefaultNodeCacheKey?;

				if (cacheKey == null)
				{
					return false;
				}

				if (cacheKey.Value.address.PathAndQuery != this.address.PathAndQuery)
				{
					return false;
				}

				if (!cacheKey.Value.nodeType.Equals(this.nodeType))
				{
					return false;
				}

				return true;
			}
		}

		protected override DefaultNodeCacheKey GetKey(INodeAddress address, NodeType nodeType)
		{
			return new DefaultNodeCacheKey(address, nodeType);
		}

		public override void PurgeWithDescendents(INodeAddress address, NodeType nodeType)
		{
			lock (this.SyncLock)
			{
				ILList<DefaultNodeCacheKey> removeList = new ArrayList<DefaultNodeCacheKey>();

				foreach (var key in this.Cache.Keys)
				{
					if (key.NodeAddress.IsDescendentOf(address, AddressScope.DescendentOrSelf))
					{
						if (key.NodeType.Is(nodeType))
						{
							removeList.Add(key);
						}
					}
				}

				foreach (DefaultNodeCacheKey key in removeList)
				{
					this.Purge(key.NodeAddress, key.NodeType);
				}
			}
		}
	}
}
