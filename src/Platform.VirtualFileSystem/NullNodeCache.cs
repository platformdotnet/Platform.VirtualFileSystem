using System.Collections;

namespace Platform.VirtualFileSystem
{
	public class NullNodeCache
		: INodeCache
	{
		public virtual IEnumerator GetEnumerator()
		{
			return new object[0].GetEnumerator();
		}

		public void Add(INodeAddress address, INode node)
		{
		}

		public INode Get(INodeAddress address, Platform.VirtualFileSystem.NodeType nodeType)
		{
			return null;
		}

		public void Purge()
		{
		}

		void INodeCache.Purge(INodeAddress address, NodeType nodeType)
		{
		}

		public void PurgeWithDescendents(INodeAddress address, NodeType nodeType)
		{
		}

		public object SyncLock
		{
			get { return this; }
		}

		public IAutoLock GetAutoLock()
		{
			return new AutoLock(this);
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}
	}
}

