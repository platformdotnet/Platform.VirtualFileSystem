using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for all <see cref="IList{T}"/> objects that implement
	/// lists that are most efficient when accessed sequentially.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the list</typeparam>
	public abstract class AbstractSequentialAccessList<T>
		: AbstractList<T>
	{
		public override void CopyTo(T[] array, int arrayIndex, int collectionIndex, int count)
		{
			IEnumerator<T> enumerator;

			using (enumerator = GetEnumerator())
			{
				while (collectionIndex > 0)
				{
					if (!enumerator.MoveNext())
					{
						return;
					}
				}

				while (enumerator.MoveNext())
				{
					array[arrayIndex++] = enumerator.Current;
				}
			}
		}

		public override int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			int x, y, result;

			x = 0;
			y = (index + length) - 1; 

			foreach (T value in this)
			{
				if (x == y)
				{
					return x;
				}

				if (x < index)
				{
					x++;

					continue;
				}

				result = comparison(value, item);

				if (result == 0)
				{
					return x;
				}
				else if (x < 0)
				{
					x++;

					continue;
				}
			}

			return ~x;
		}
	}
}
