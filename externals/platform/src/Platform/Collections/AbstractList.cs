using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for all <see cref="ILList{T}"/> implementers.
	/// </summary>
	/// <typeparam name="T">The type of gthe objects stored</typeparam>
	public abstract class AbstractList<T>
		: AbstractCollection<T>, ILList<T>
	{
		#region ILIndexable<T> Members

		public virtual event EventHandler<ListEventArgs<T>> AfterItemChanged;
		public virtual event EventHandler<ListEventArgs<T>> BeforeItemChanged;

		protected virtual bool InvokeAfterItemChangedRequired
		{
			get
			{
				return AfterItemChanged != null;
			}
		}

		protected virtual void OnAfterItemChanged(ListEventArgs<T> eventArgs)
		{
			if (AfterItemChanged != null)
			{
				AfterItemChanged(this, eventArgs);
			}
		}

		protected virtual bool InvokeBeforeItemChangedRequired
		{
			get
			{
				return BeforeItemChanged != null;
			}
		}

		protected virtual void OnBeforeItemChanged(ListEventArgs<T> eventArgs)
		{
			if (BeforeItemChanged != null)
			{
				BeforeItemChanged(this, eventArgs);
			}
		}

		public abstract T this[int index]
		{
			get;
			set;
		}

		public virtual int IndexOf(T value)
		{
			int retval = 0;

			foreach (T item in this)
			{
				if (item.Equals(value))
				{
					return retval;
				}

				retval++;
			}

			return -1;
		}

		public virtual T First
		{
			get
			{
				return this[0];
			}
		}

		public virtual T Last
		{
			get
			{
				return this[this.Count - 1];
			}
		}

		public override bool Remove(T item)
		{
			return RemoveFirst(item);
		}

		public virtual T RemoveLast()
		{
			T retval = this[this.Count - 1];

			RemoveAt(this.Count - 1);

			return retval;
		}

		public virtual T RemoveFirst()
		{
			T retval = this[0];

			RemoveAt(0);

			return retval;
		}

		public abstract bool RemoveLast(T item);
		public abstract bool RemoveFirst(T item);		

		public abstract void RemoveAt(int index);

		public virtual void RemoveRange(int startIndex, int count)
		{
			if (startIndex < 0 || startIndex > this.Count - 1)
			{
				throw new ArgumentOutOfRangeException("endIndex");
			}

			if (startIndex + count > this.Count - 1)
			{
				throw new ArgumentOutOfRangeException("count");
			}

			for (int i = 0; i < count; i++)
			{
				RemoveAt(startIndex);
			}
		}

		public virtual IEnumerable<Pair<int, T>> GetIndexValuePairs()
		{
			int index = 0;

			foreach (T item in this)
			{
				yield return new Pair<int, T>(index, item);
			}
		}

		#endregion

		public override void Clear()
		{
			while (this.Count > 0)
			{
				this.RemoveAt(this.Count - 1);
			}
		}

        public virtual void CopyTo(T[] array, int arrayIndex, int collectionIndex, int count)
        {
            for (int i = 0; i < count; i++)
            {
                array[i + arrayIndex] = this[i + collectionIndex];
            }
        }

        #region IReverseEnumerable<T> Members

        public abstract IEnumerator<T> GetReverseEnumerator();

		public abstract IEnumerable<T> GetReverseEnumerable();

		#endregion

		#region IList<T> Members

		public abstract void Insert(int index, T item);

		#endregion

        public override IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
            {
                yield return this[i];
            }
        }

		public virtual void Sort(int index, int length, Comparison<T> comparison)
		{
			throw new NotSupportedException();
		}

		public virtual T FindFirst(Predicate<T> accept)
		{
			T retval;

			if (TryFindFirst(accept, out retval))
			{
				return retval;
			}

			throw new ItemNotFoundException();
		}

		public virtual T FindLast(Predicate<T> accept)
		{
			T retval;

			if (TryFindLast(accept, out retval))
			{
				return retval;
			}
			
			throw new ItemNotFoundException();
		}

		public virtual bool TryFindFirst(Predicate<T> accept, out T retval)
		{
			foreach (T value in this)
			{
				if (accept(value))
				{
					retval = value;

					return true;
				}
			}

			retval = default(T);

			return false;
		}

		public virtual bool TryFindLast(Predicate<T> accept, out T retval)
		{
			foreach (T value in this.GetReverseEnumerable())
			{
				if (accept(value))
				{
					retval = value;

					return true;
				}
			}

			retval = default(T);

			return false;
		}

		public abstract int SortedSearch(T item, int index, int length, Comparison<T> comparison);

		/// <summary>
		/// Creates wrapper for this list that is readonly.
		/// </summary>
		public override ILCollection<T> ToReadOnly()
		{
			return new ReadOnlyList<T>(this);
		}

		/// <summary>
		/// Creates wrapper for this list that is readonly.
		/// </summary>
		ILList<T> ILList<T>.ToReadOnly()
		{
			return new ReadOnlyList<T>(this);
		}
    }
}
