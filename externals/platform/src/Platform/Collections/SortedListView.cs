using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Provides a sorted view of a list (automatically adjusting when the backing
	/// list changes).
	/// </summary>
	/// <typeparam name="T">The type stored in the list</typeparam>
	public class SortedListView<T>
		: AbstractRandomAccessList<T>
	{		
		private int version = 0;
		private readonly Comparison<T> comparison;
		private int[] sortedToRealIndexMapping;
		private int[] realToSortedIndexMapping;

		protected virtual ILList<T> Wrappee
		{
			get
			{
				return wrappee;
			}
		}
		private readonly ILList<T> wrappee;

		public SortedListView(ILList<T> wrappee)
			: this(wrappee, Comparer<T>.Default.Compare)
		{
		}

		public SortedListView(ILList<T> wrappee, Comparison<T> comparison)
		{
			this.wrappee = wrappee;

			this.comparison = comparison;

			sortedToRealIndexMapping = new int[Math.Max(16, wrappee.Count)];
			realToSortedIndexMapping = new int[Math.Max(16, wrappee.Count)];

			wrappee.AfterItemAdded += WrappeeItemAdded;
			wrappee.AfterItemChanged += WrappeeItemChanged;
			wrappee.AfterItemRemoved += WrappeeItemRemoved;
			wrappee.BeforeCleared += WrappeeBeforeCleared;
			wrappee.AfterCleared += WrappeeAfterCleared;

			int i = 0;

			foreach (T item in wrappee)
			{
				Add(item, i, ++i);
			}
		}

		public override T this[int index]
		{
			get
			{
				return this.Wrappee[sortedToRealIndexMapping[index]];
			}
			set
			{
				index = sortedToRealIndexMapping[index];

				this.Wrappee[index] = value;
			}
		}

		public override void RemoveAt(int index)
		{
			index = sortedToRealIndexMapping[index];

			this.Wrappee.RemoveAt(index);
		}

		public override void Insert(int index, T item)
		{
			if (index == this.Count)
			{
				Add(item);
			}

			index = sortedToRealIndexMapping[index];

			this.Wrappee.Insert(index, item);
		}

		public override void Add(T item)
		{
			this.Wrappee.Add(item);
		}

		public override int Count
		{
			get
			{
				return this.Wrappee.Count;
			}
		}

		public override void Clear()
		{
			this.Wrappee.Clear();
		}

		public override int IndexOf(T item)
		{
			int index;

			index = this.Wrappee.IndexOf(item);

			return realToSortedIndexMapping[index];
		}

		public override bool Contains(T item)
		{
			return BinarySearch(0, this.Count, item) >= 0;
		}

		public override IEnumerator<T> GetEnumerator()
		{
			int localVersion = this.version;

			for (int i = 0; i < this.Count; i++)
			{
				if (this.version != localVersion)
				{
					throw new InvalidOperationException();
				}

				yield return this.Wrappee[sortedToRealIndexMapping[i]];
			}
		}

		public override IEnumerable<T> GetReverseEnumerable()
		{
			int localVersion = this.version;

			for (int i = this.Count - 1; i >= 0; i--)
			{
				if (this.version != localVersion)
				{
					throw new InvalidOperationException();
				}

				yield return this.Wrappee[sortedToRealIndexMapping[i]];
			}
		}

		public override IEnumerator<T> GetReverseEnumerator()
		{
			return this.GetReverseEnumerable().GetEnumerator();
		}

		private void WrappeeBeforeCleared(object sender, CollectionEventArgs<T> e)
		{
			OnBeforeCleared(e);
		}

		private void WrappeeAfterCleared(object sender, CollectionEventArgs<T> e)
		{
			OnAfterCleared(e);
		}

		private bool suspendFire = false;

		private void WrappeeItemChanged(object sender, CollectionEventArgs<T> e)
		{
			int index;
			suspendFire = true;

			try
			{
				WrappeeItemRemoved(sender, e);
				
				index = Add(e.Item, ((ListEventArgs<T>)e).Index, this.Wrappee.Count);

				OnAfterItemChanged(new ListEventArgs<T>(e.Item, index));
			}
			finally
			{
				suspendFire = false;
			}	
		}

		private void WrappeeItemRemoved(object sender, CollectionEventArgs<T> e)
		{
			int index;
			int count = this.Wrappee.Count;
			ListEventArgs<T> listEventArgs = (ListEventArgs<T>)e;

			index = realToSortedIndexMapping[listEventArgs.Index];

			Array.Copy(sortedToRealIndexMapping, index + 1, sortedToRealIndexMapping, index, count - index);

			Array.Copy(realToSortedIndexMapping, listEventArgs.Index + 1, realToSortedIndexMapping, listEventArgs.Index, count - listEventArgs.Index);

			if (!suspendFire)
			{
				OnAfterItemRemoved(new ListEventArgs<T>(e.Item, index));
			}
		}

		private void WrappeeItemAdded(object sender, CollectionEventArgs<T> e)
		{
			int index;

			index = Add(e.Item, ((ListEventArgs<T>)e).Index, this.Wrappee.Count);

			if (!suspendFire)
			{
				OnAfterItemAdded(new ListEventArgs<T>(e.Item, index));
			}
		}

		private int Add(T item, int listindex, int count)
		{
			int index;
									
			index = BinarySearch(0, count - 1, item);
						
			if (index < 0)
			{
				index = -index;
			}
			else
			{
				index++;
			}

			if (sortedToRealIndexMapping.Length < count)
			{
				Array.Resize<int>(ref sortedToRealIndexMapping, sortedToRealIndexMapping.Length * 2);
				Array.Resize<int>(ref realToSortedIndexMapping, realToSortedIndexMapping.Length * 2);
			}

			if (count - index > 0)
			{
				Array.Copy(sortedToRealIndexMapping, index - 1, sortedToRealIndexMapping, index, count - index);

				for (int i = index - 1; i < count; i++)
				{
					realToSortedIndexMapping[i]++;
				}
			}

			realToSortedIndexMapping[listindex] = index - 1;
			sortedToRealIndexMapping[index - 1] = listindex;

			return index - 1;
		}

		public override int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			throw new NotSupportedException();
		}

		private int BinarySearch(int index, int length, T value)
		{
			int x, y, z, r;

			x = index;
			y = (index + length) - 1;

			while (x <= y)
			{
				z = x + ((y - x) >> 1);

				r = comparison(this.Wrappee[sortedToRealIndexMapping[z]], value);

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
	}
}
