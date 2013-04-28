using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Contains information about events.
	/// </summary>
	/// <typeparam name="T">The type in the list</typeparam>
	public class ListEventArgs<T>
		: CollectionEventArgs<T>
	{
		/// <summary>
		/// The related index.
		/// </summary>
		public virtual int Index
		{
			get;
			private set;
		}

		/// <summary>
		/// Creates a new <see cref="ListEventArgs{T}"/>.
		/// </summary>
		/// <param name="item">The item related to the event</param>
		/// <param name="index">The index related to the event</param>
		public ListEventArgs(T item, int index)
			: base(item)
		{
			this.Index = index;
		}
	}
}
