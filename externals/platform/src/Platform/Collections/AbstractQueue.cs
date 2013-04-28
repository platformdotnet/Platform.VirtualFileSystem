using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for all <see cref="ILQueue{T}"/> implementers.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the queue</typeparam>
	public abstract class AbstractQueue<T>
		: AbstractCollection<T>, ILQueue<T>
	{
		#region ILQueue<T> Members

		public abstract void Enqueue(T item);

		public virtual void Enqueue(T[] items, int offset, int count)
		{
			for (int i = offset; i < offset + count; i++)
			{
				Enqueue(items[i]);
			}
		}

		public virtual T Dequeue()
		{
			T value;

			if (TryDequeue(out value))
			{
				return value;
			}

			throw new InvalidOperationException();
		}

		public virtual T Peek()
		{
			T value;

			if (TryPeek(out value))
			{
				return value;
			}

			throw new InvalidOperationException();
		}

		public virtual int TryDequeue(T[] items, int offset, int count)
		{
			var retval = 0;

			for (var i = offset; i < count + offset; i++)
			{
				T value;

				if (TryDequeue(out value))
				{
					items[i] = value;

					retval++;
				}
				else
				{
					break;
				}
			}

			return retval;
		}

		public abstract bool TryDequeue(out T value);

		public abstract bool TryPeek(out T value);

		#endregion

		public override void Add(T item)
		{
			Enqueue(item);
		}

		public override bool Remove(T item)
		{
			throw new NotSupportedException();
		}		
	}
}
