using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A wrapper for a list.  Delegates all calls to a backing (wrappee) list.
	/// Use this as the base for classes that decorates list objects.
	/// </summary>
	/// <typeparam name="T">The type stored in the list</typeparam>
	public class ListWrapper<T>
		: CollectionWrapper<T>, ILList<T>
	{
		public new ILList<T> Wrappee
		{
			get
			{
				return (ILList<T>)base.Wrappee;
			}
			set
			{
				base.Wrappee = value;
			}
		}

		public ListWrapper(ILList<T> wrappee)
			: base(wrappee)
		{
		}

		public virtual int SortedSearch(T item, int index, int length, Comparison<T> comparison)
		{
			return this.Wrappee.SortedSearch(item, index, length, comparison);
		}

		public virtual bool RemoveLast(T item)
		{
			return this.Wrappee.RemoveLast(item);
		}

		public virtual bool RemoveFirst(T item)
		{
			return this.Wrappee.RemoveFirst(item);
		}

		#region IList<T> Members

		public virtual int IndexOf(T item)
		{
			return this.Wrappee.IndexOf(item);
		}

		public virtual void Insert(int index, T item)
		{
			this.Wrappee.Insert(index, item);
		}

		public virtual void RemoveAt(int index)
		{
			this.Wrappee.RemoveAt(index);
		}

		public virtual T this[int index]
		{
			get
			{
				return this.Wrappee[index];
			}
			set
			{
				this.Wrappee[index] = value;
			}
		}

		#endregion

		#region ILList<T> Members

		public virtual void Sort(int index, int length, Comparison<T> comparison)
		{
			this.Wrappee.Sort(index, length, comparison);
		}

		public virtual event EventHandler<ListEventArgs<T>> AfterItemChanged
		{
			add
			{
				if (e_AfterItemChanged == null)
				{
					this.Wrappee.AfterItemChanged += OnAfterItemChanged;
				}
			}
			remove
			{
				e_AfterItemChanged = (EventHandler<ListEventArgs<T>>)Delegate.Remove(e_AfterItemChanged, value);

				if (e_AfterItemChanged == null)
				{
					this.Wrappee.AfterItemChanged -= OnAfterItemChanged;
				}
			}
		}
		protected virtual void OnAfterItemChanged(object sender, ListEventArgs<T> eventArgs)
		{
			if (e_AfterItemChanged != null)
			{
				e_AfterItemChanged(this, eventArgs);
			}
		}
		private EventHandler<ListEventArgs<T>> e_AfterItemChanged;

		public virtual event EventHandler<ListEventArgs<T>> BeforeItemChanged
		{
			add
			{
				if (e_BeforeItemChanged == null)
				{
					this.Wrappee.BeforeItemChanged += OnBeforeItemChanged;
				}
			}
			remove
			{
				e_BeforeItemChanged = (EventHandler<ListEventArgs<T>>)Delegate.Remove(e_BeforeItemChanged, value);

				if (e_BeforeItemChanged == null)
				{
					this.Wrappee.BeforeItemChanged -= OnBeforeItemChanged;
				}
			}
		}
		protected virtual void OnBeforeItemChanged(object sender, ListEventArgs<T> eventArgs)
		{
			if (e_BeforeItemChanged != null)
			{
				e_BeforeItemChanged(this, eventArgs);
			}
		}
		private EventHandler<ListEventArgs<T>> e_BeforeItemChanged;

		public virtual T First
		{
			get
			{
				return this.Wrappee.First;
			}
		}

		public virtual T Last
		{
			get
			{
				return this.Wrappee.Last;
			}
		}

		public virtual T RemoveLast()
		{
			return this.Wrappee.RemoveLast();
		}

		public virtual T RemoveFirst()
		{
			return this.Wrappee.RemoveFirst();
		}

		public virtual void RemoveRange(int startIndex, int endIndex)
		{
			this.Wrappee.RemoveRange(startIndex, endIndex);
		}

		public virtual IEnumerable<Pair<int, T>> GetIndexValuePairs()
		{
			return this.Wrappee.GetIndexValuePairs();
		}

		public virtual void CopyTo(T[] array, int arrayIndex, int collectionIndex, int count)
		{
			this.Wrappee.CopyTo(array, arrayIndex, collectionIndex, count);
		}

		ILList<T> ILList<T>.ToReadOnly()
		{
			return (ILList<T>)this.ToReadOnly();
		}

		#endregion
		
		public virtual IEnumerator<T> GetReverseEnumerator()
		{
			return this.Wrappee.GetReverseEnumerator();
		}

		public virtual T FindFirst(Predicate<T> accept)
		{
			return this.Wrappee.FindFirst(accept);
		}

		public virtual T FindLast(Predicate<T> accept)
		{
			return this.Wrappee.FindLast(accept);
		}

		#region ILList<T> Members


		public virtual bool TryFindFirst(Predicate<T> accept, out T retval)
		{
			return this.Wrappee.TryFindFirst(accept, out retval);
		}

		public virtual bool TryFindLast(Predicate<T> accept, out T retval)
		{
			return this.Wrappee.TryFindLast(accept, out retval);
		}

		#endregion
	}
}
