using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Used to convert any <see cref="IEnumerator{T}"/> into an <see cref="ICollection{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the collection</typeparam>
	public class EnumerableCollectionAdapter<T>
		: ICollection<T>
	{
		private readonly Func<int> GetCount;
		private readonly IEnumerable<T> Enumerable;

		/// <summary>
		/// Constructs a new <see cref="EnumerableCollectionAdapter{T}"/>.
		/// </summary>
		/// <param name="enumerable">The enumerable that this collection will use</param>
		/// <param name="count">The number of elements in the enumerable</param>
		public EnumerableCollectionAdapter(IEnumerable<T> enumerable, int count)
			: this(enumerable, () => count)
		{			
		}

		/// <summary>
		/// Constructs a new <see cref="EnumerableCollectionAdapter{T}"/>.
		/// </summary>
		/// <param name="enumerable">The enumerable that this collection will use</param>
		/// <param name="getCount">A function that returns the cou nt of the enumerable</param>
		public EnumerableCollectionAdapter(IEnumerable<T> enumerable, Func<int> getCount)
		{
			this.GetCount = getCount;
			this.Enumerable = enumerable;
		}

		#region ICollection<T> Members

		public virtual void Add(T item)
		{
			throw new NotSupportedException();
		}

		public virtual void Clear()
		{
			throw new NotSupportedException();
		}

		public virtual bool Contains(T item)
		{
			return this.Enumerable.Contains(item);
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			foreach (T value in this)
			{
				array[arrayIndex++] = value;
			}
		}

		public virtual int Count
		{
			get
			{
				return this.GetCount();
			}
		}

		public virtual bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public virtual bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region IEnumerable<T> Members

		public virtual IEnumerator<T> GetEnumerator()
		{
			return Enumerable.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
}
}
