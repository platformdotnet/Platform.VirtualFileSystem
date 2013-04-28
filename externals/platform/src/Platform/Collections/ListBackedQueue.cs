using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A queue that uses a list for storage.
	/// </summary>
	/// <typeparam name="T">Type of the objects stored in the list</typeparam>
	public class ListBackedQueue<T>
		: AbstractQueue<T>
	{		
		protected ILList<T> list;
		protected bool listIsRandomAccess;

		public ListBackedQueue(ILList<T> list)
		{
			this.list = list;
		}

		public override void Enqueue(T item)
		{
			list.Add(item);
		}

		public override bool TryDequeue(out T value)
		{
			if (list.Count == 0)
			{
				value = default(T);

				return false;
			}

			value = list.RemoveFirst();

			return true;
		}

		public override bool TryPeek(out T value)
		{
			if (list.Count == 0)
			{
				value = default(T);

				return false;
			}

			value = list.First;

			return true;
		}

		public override void Add(T item)
		{
			Enqueue(item);
		}

		public override void Clear()
		{
			list.Clear();
		}

		public override int Count
		{
			get
			{
				return list.Count;
			}
		}

		public override bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		public override IEnumerator<T> GetEnumerator()
		{
			return list.GetEnumerator();
		}
	}
}
