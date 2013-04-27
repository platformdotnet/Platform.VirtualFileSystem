using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	public class NodeAttributesWrapper
		: MarshalByRefObject, INodeAttributes
	{
		private readonly INodeAttributes wrappee;

		public NodeAttributesWrapper(INodeAttributes innerMutableNodeAttributes)
		{
			this.wrappee = innerMutableNodeAttributes;
		}

		protected virtual INodeAttributes Wrappee
		{
			get { return this.wrappee; }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ((System.Collections.IEnumerable) this.Wrappee).GetEnumerator();
		}

		public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return this.Wrappee.GetEnumerator();
		}

		public virtual DateTime? CreationTime
		{
			get { return this.Wrappee.CreationTime; }
			set { this.Wrappee.CreationTime = value; }
		}

		public virtual DateTime? LastAccessTime
		{
			get { return this.Wrappee.LastAccessTime; }
			set { this.Wrappee.LastAccessTime = value; }
		}

		public virtual DateTime? LastWriteTime
		{
			get { return this.Wrappee.LastWriteTime; }
			set { this.Wrappee.LastWriteTime = value; }
		}

		public virtual object this[string name]
		{
			get { return this.Wrappee[name]; }
			set { this.Wrappee[name] = value; }
		}

		public virtual bool Exists
		{
			get { return this.Wrappee.Exists; }
		}

		public virtual IEnumerable<string> Names
		{
			get { return this.Wrappee.Names; }
		}

		public virtual IEnumerable<object> Values
		{
			get { return this.Wrappee.Values; }
		}

		public virtual bool Supports(string name)
		{
			return this.Wrappee.Supports(name);
		}

		void IRefreshable.Refresh()
		{
			this.Wrappee.Refresh();
		}

		public virtual INodeAttributes Refresh()
		{
			this.Wrappee.Refresh();

			return this;
		}

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode();
		}

		public override string ToString()
		{
			return this.Wrappee.ToString();
		}

		public virtual object SyncLock
		{
			get { return this.Wrappee.SyncLock; }
		}

		public virtual IAutoLock GetAutoLock()
		{
			return this.Wrappee.GetAutoLock();
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return this.Wrappee.AquireAutoLock();
		}

		public virtual INodeAttributesUpdateContext AquireUpdateContext()
		{
			return this.Wrappee.AquireUpdateContext();
		}

		public bool? ReadOnly
		{
			get { return this.Wrappee.ReadOnly; }
			set { this.Wrappee.ReadOnly = value; }
		}

		public virtual bool? IsHidden
		{
			get { return this.Wrappee.IsHidden; }
			set { this.Wrappee.IsHidden = value; }
		}

		object ISyncLocked.SyncLock
		{
			get { return this.SyncLock; }
		}

		IAutoLock ISyncLocked.GetAutoLock()
		{
			return this.GetAutoLock();
		}

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
