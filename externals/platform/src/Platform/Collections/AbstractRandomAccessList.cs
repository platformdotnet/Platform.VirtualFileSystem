using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for all <see cref="IList{T}"/> objects that implement
	/// lists that are efficient when accessed randomly.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the list</typeparam>
	public abstract class AbstractRandomAccessList<T>
		: AbstractList<T>
	{
		public override bool RemoveLast(T item)
		{
			if (this.Count == 0)
			{
				return false;
			}

			for (var i = this.Count - 1; i >= 0; i--)
			{
				if (this[i].Equals(item))
				{
					RemoveAt(i);

					return true;
				}
			}

			return false;
		}

		public override bool RemoveFirst(T item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].Equals(item))
				{
					RemoveAt(i);

					return true;
				}
			}

			return false;
		}

		public override int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			int x, y, z, r;

			if (index < 0 || length < 0)
			{
				throw new ArgumentException();
			}

			if (index + length > this.Count)
			{
				throw new ArgumentException();
			}

			x = index;
			y = (index + length) - 1;

			while (x <= y)
			{
				z = x + ((y - x) >> 1);

				r = comparison(this[z], item);

				if (r == 0)
				{
					return z;
				}

				if (r < 0)
				{
					x = z + 1;

					continue;
				}

				y = z - 1;
			}

			return ~x;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			for (var i = 0; i < this.Count; i++)
			{
				yield return this[i];
			}
		}

		public override void CopyTo(T[] array, int arrayIndex, int collectionIndex, int count)
		{
			for (var i = collectionIndex; i < count; i++)
			{
				array[arrayIndex++] = this[i];
			}
		}

		public override IEnumerator<T> GetReverseEnumerator()
		{
			for (var i = this.Count - 1; i >= 0; i--)
			{
				yield return this[i];
			}
		}
	}
}
