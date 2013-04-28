using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	public class CollectionEventArgs<T>
		: EventArgs
	{
		public virtual T Item
		{
			get;
			private set;
		}

		public virtual bool Cancel
		{
			get;
			set;
		}

		public CollectionEventArgs()
		{
		}

		public CollectionEventArgs(T item)
		{
			this.Item = item;
		}
	}
}
