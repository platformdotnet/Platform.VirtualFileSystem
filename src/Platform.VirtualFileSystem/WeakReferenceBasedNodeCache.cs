using System;
using System.Collections;
using System.Collections.Generic;
using Platform.Collections;

namespace Platform.VirtualFileSystem
{
	public abstract class WeakReferenceBasedNodeCache<K>
		: INodeCache
	{
		public const int DefaultCapacity = Int16.MaxValue;

		private readonly ILDictionary<K, INode> cache;

		protected ILDictionary<K, INode> Cache
		{
			get
			{
				return this.cache;
			}
		}

		protected WeakReferenceBasedNodeCache()
			: this(DefaultCapacity)
		{
		}

		protected WeakReferenceBasedNodeCache(int capacity)
		{
			this.cache = new TimedReferenceDictionary<K, INode>(TimeSpan.FromMinutes(30), capacity, typeof(Dictionary<,>), Int16.MaxValue);
		}

		public virtual IEnumerator GetEnumerator()
		{
			return this.cache.Values.GetEnumerator();
		}

		protected abstract K GetKey(INodeAddress address, NodeType nodeType);

		public virtual void Add(INodeAddress address, INode node)
		{
			lock (this.SyncLock)
			{
				this.cache[this.GetKey(address, node.NodeType)] = node;
			}
		}

		public virtual INode Get(INodeAddress address, NodeType nodeType)
		{
			lock (this.SyncLock)
			{
				if (nodeType == NodeType.None)
				{
					throw new ArgumentException("Can't be None", "nodeType");
				}
				else
				{
					INode node;

					if (nodeType == NodeType.Any)
					{
						if (this.cache.TryGetValue(this.GetKey(address, NodeType.File), out node))
						{
							return node;
						}

						if (this.cache.TryGetValue(this.GetKey(address, NodeType.Directory), out node))
						{
							return node;
						}

						foreach (var node2 in this.cache.Values)
						{
							if (this.GetKey(address, node2.NodeType).Equals(this.GetKey(node2.Address, node2.NodeType)))
							{
								return node;
							}
						}

						return null;
					}
					else
					{
						if (!this.cache.TryGetValue(this.GetKey(address, nodeType), out node))
						{
							return null;
						}

						return node;
					}
				}
			}
		}

		public virtual void Purge()
		{
			lock (this.SyncLock)
			{
				this.cache.Clear();
			}
		}

		public virtual void Purge(INodeAddress address, NodeType nodeType)
		{
			lock (this.SyncLock)
			{
				this.cache.Remove(this.GetKey(address, nodeType));
			}
		}

		public virtual void PurgeWithDescendents(INodeAddress address, NodeType nodeType)
		{
			this.Purge();
		}

		public virtual object SyncLock
		{
			get
			{
				return this.cache.SyncLock;
			}
		}

		public virtual IAutoLock GetAutoLock()
		{
			return new AutoLock(this.cache.SyncLock);
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return this.GetAutoLock().Lock();
		}
	}
}