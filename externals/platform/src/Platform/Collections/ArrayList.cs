using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A list implementation that uses an array as storage.
	/// </summary>
	/// <typeparam name="T">The type of object stored in the list</typeparam>
	public class ArrayList<T>
		: AbstractRandomAccessList<T>, ILList<T>
	{
		private int count;
		private T[] array;
		private const int MinimumCapacity = 0x10;

		public ArrayList()
			: this(MinimumCapacity)
		{
		}

		public ArrayList(int capacity)
		{
			if (capacity < MinimumCapacity)
			{
				capacity = MinimumCapacity;
			}

			count = 0;
			array = new T[capacity];
		}

		public ArrayList(IEnumerable<T> items)			
		{
			count = 0;
			array = new T[MinimumCapacity];

			foreach (T item in items)
			{
				Add(item);
			}
		}

		public override T this[int index]
		{
			get
			{
				if (index >= this.Count)
				{
					throw new IndexOutOfRangeException();
				}

				return array[index];
			}
			set
			{
				if (this.InvokeBeforeItemChangedRequired)
				{
					OnAfterItemChanged(new ListEventArgs<T>(array[index], index));
				}

				if (index >= this.Count)
				{
					throw new IndexOutOfRangeException();
				}

				array[index] = value;
				version++;

				if (this.InvokeAfterItemChangedRequired)
				{
					OnAfterItemChanged(new ListEventArgs<T>(array[index], index));
				}
			}
		}
		
		public override void Add(T item)
		{
			ListEventArgs<T> eventArgs = null;

			if (this.InvokeBeforeItemAddedRequired || this.InvokeAfterItemAddedRequired)
			{
				eventArgs = new ListEventArgs<T>(item, count);
			}

			OnBeforeItemAdded(eventArgs);

			if (count >= array.Length)
			{
				Array.Resize<T>(ref array, array.Length << 1);
			}

			array[count] = item;

			version++;
			count++;

			OnAfterItemAdded(eventArgs);
		}

		private int version = 0;

		public override IEnumerator<T> GetEnumerator()
		{
			int version = this.version;

			for (int i = 0; i < count; i++)
			{
				if (version != this.version)
				{
					throw new InvalidOperationException();
				}

				yield return array[i];
			}
		}

		public override IEnumerable<T> GetReverseEnumerable()
		{
			var currentVersion = this.version;

			for (var i = count; i >= 0; i--)
			{
                if (currentVersion != this.version)
				{
					throw new InvalidOperationException();
				}

				yield return array[i];
			}
		}

		public override void Clear()
		{
		    var eventArgs = new CollectionEventArgs<T>();

			OnBeforeCleared(eventArgs);

			if (eventArgs.Cancel)
			{
				return;
			}

			if (this.InvokeAfterItemRemovedRequired)
			{
				base.Clear();
			}
			else
			{
				Array.Clear(array, 0, count);
				count = 0;
			}

			OnAfterCleared(eventArgs);
		}

		public override int Count
		{
			get
			{
				return count;
			}
		}

		public override void RemoveAt(int index)
		{
			if (index < 0)
			{
				throw new ArgumentException("index < 0", "index");
			}

			if (index > this.Count)
			{
				throw new ArgumentException("index > count", "index");
			}

			ListEventArgs<T> eventArgs;

			eventArgs = new ListEventArgs<T>(this[index], index);

			OnBeforeItemRemoved(eventArgs);
						
			if (index == this.Count - 1)
			{
				array[count - 1] = default(T);
			}
			else
			{
				Array.Copy(array, index + 1, array, index, array.Length - index - 1);

				array[count - 1] = default(T);
			}

			version++;

			count--;

			OnAfterItemRemoved(eventArgs);
		}

		#region IList<T> Members

		public override void Insert(int index, T item)
		{
			if (index < 0)
			{
				throw new ArgumentException("index < 0", "index");
			}

			if (index > this.Count)
			{
				throw new ArgumentException("index > count", "index");
			}

			if (count >= array.Length)
			{
				Array.Resize<T>(ref array, array.Length << 1);
			}

			ListEventArgs<T> eventArgs = null;

			if (this.InvokeBeforeItemAddedRequired || this.InvokeAfterItemAddedRequired)
			{
				eventArgs = new ListEventArgs<T>(item, index);
			}

			if (this.InvokeBeforeItemAddedRequired || this.InvokeAfterItemAddedRequired)
			{
				OnBeforeItemAdded(eventArgs);
			}

			Array.Copy(array, index, array, index + 1, array.Length - index - 1);

			array[index] = item;

			count++;

			if (this.InvokeBeforeItemAddedRequired || this.InvokeAfterItemAddedRequired)
			{
				OnAfterItemAdded(eventArgs);
			}
		}

		public override int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			if (index < 0 || length < 0)
			{
				throw new ArgumentException();
			}

			if (index + length > count)
			{
				throw new ArgumentException();
			}

			return Array.BinarySearch<T>(array, index, length, item, new ComparisonComparer<T>(comparison));
		}

		public override void Sort(int index, int length, Comparison<T> comparison)
		{
			if (index < 0 || length < 0)
			{
				throw new ArgumentException();
			}

			if (index + length > count)
			{
				throw new ArgumentException();
			}

			Array.Sort(array, index, length, new ComparisonComparer<T>(comparison));
		}

		#endregion
	}
}
