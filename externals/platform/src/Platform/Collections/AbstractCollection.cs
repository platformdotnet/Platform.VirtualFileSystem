using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for objects that implement <see cref="ILCollection{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of objects that the collection can store.</typeparam>
	public abstract class AbstractCollection<T>
		: ILCollection<T>
	{
		#region ICollection<T> Members

		public abstract void Add(T item);

		public abstract void Clear();

		public virtual object Clone()
		{
			throw new NotSupportedException();
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}

		public virtual IAutoLock GetAutoLock()
		{
			return new AutoLock(this);
		}

		public virtual bool IsEmpty
		{
			get
			{
				return this.Count == 0;
			}
		}

		public virtual bool Contains(T item)
		{
			foreach (T value in this)
			{
				if (value.Equals(item))
				{
					return true;
				}
			}

			return false;
		}

		public virtual void AddAll(T[] items, int offset, int count)
		{
			this.AddAll(EnumerableUtils.Range<T>(items, offset, Count));
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			int i = 0;

			foreach (T item in this)
			{
				array[arrayIndex + i++] = item;
			}
		}

		public abstract int Count
		{
			get;
		}

		public virtual bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public abstract bool Remove(T item);

		#endregion

		#region IEnumerable<T> Members

		public abstract IEnumerator<T> GetEnumerator();

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		#region ILCollection<T> Members

		public virtual event EventHandler<CollectionEventArgs<T>> AfterItemAdded;

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeItemAdded;

		public virtual event EventHandler<CollectionEventArgs<T>> AfterItemRemoved;

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeItemRemoved;

		public virtual event EventHandler<CollectionEventArgs<T>> AfterCleared;

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeCleared;

		protected virtual bool InvokeAfterItemAddedRequired
		{
			get
			{
				return AfterItemAdded != null;
			}
		}

		protected virtual void OnAfterItemAdded(CollectionEventArgs<T> eventArgs)
		{
			if (AfterItemAdded != null)
			{
				AfterItemAdded(this, eventArgs);
			}
		}

		protected virtual bool InvokeBeforeItemAddedRequired
		{
			get
			{
				return BeforeItemAdded != null;
			}
		}

		protected virtual void OnBeforeItemAdded(CollectionEventArgs<T> eventArgs)
		{
			if (BeforeItemAdded != null)
			{
				BeforeItemAdded(this, eventArgs);
			}
		}

		protected virtual bool InvokeAfterItemRemovedRequired
		{
			get
			{
				return AfterItemRemoved != null;
			}
		}

		protected virtual void OnAfterItemRemoved(CollectionEventArgs<T> eventArgs)
		{
			if (AfterItemRemoved != null)
			{
				AfterItemRemoved(this, eventArgs);
			}
		}

		protected virtual bool InvokeBeforeItemRemovedRequired
		{
			get
			{
				return BeforeItemRemoved != null;
			}
		}

		protected virtual void OnBeforeItemRemoved(CollectionEventArgs<T> eventArgs)
		{
			if (BeforeItemRemoved != null)
			{
				BeforeItemRemoved(this, eventArgs);
			}
		}

		protected virtual void OnAfterCleared(CollectionEventArgs<T> eventArgs)
		{
			if (AfterCleared != null)
			{
				AfterCleared(this, eventArgs);
			}
		}

		protected virtual void OnBeforeCleared(CollectionEventArgs<T> eventArgs)
		{
			if (BeforeCleared != null)
			{
				BeforeCleared(this, eventArgs);
			}
		}

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		public virtual void AddAll(IEnumerable<T> values)
		{
			foreach (T item in values)
			{
				Add(item);
			}
		}

		public virtual int RemoveAll(IEnumerable<T> values)
		{
			int count = 0;

			foreach (T item in values)
			{
				if (Remove(item))
				{
					count++;
				}
			}

			return count;
		}

		public bool ContainsAll(IEnumerable<T> values)
		{
			foreach (T item in values)
			{
				if (!this.Contains(item))
				{
					return false;
				}
			}

			return true;
		}

		public bool ContainsAny(IEnumerable<T> values)
		{
			foreach (T item in values)
			{
				if (this.Contains(item))
				{
					return true;
				}
			}

			return false;
		}

		public virtual void ForEach(Action<T> action)
		{
			foreach (T item in this)
			{
				action(item);
			}
		}

		public virtual void ForEach(Action<T> action, Predicate<T> accept)
		{
			foreach (T item in this)
			{
				if (accept(item))
				{
					action(item);
				}
			}
		}

		public virtual void Print(Predicate<T> accept)
		{
			foreach (T item in this)
			{
				if (accept(item))
				{
					Console.WriteLine(item);
				}
			}
		}

		public virtual void PrintAll()
		{
			foreach (T item in this)
			{
				Console.WriteLine(item);
			}
		}

		public virtual T[] ToArray()
		{
		    var retval = new T[this.Count];

			CopyTo(retval, 0);

			return retval;
		}

		public virtual void CopyTo(T[] array, int arrayIndex, int count)
		{
			var i = 0;

			foreach (var item in this)
			{
				if (i == count)
				{
					break;
				}

				array[i++] = item;
			}
		}

		public virtual IEnumerable<U> ConvertAll<U>(Converter<T, U> converter)
		{
			foreach (T item in this)
			{
				yield return converter(item);
			}
		}

		public virtual ILCollection<T> ToReadOnly()
		{
			throw new NotSupportedException();
		}

		public virtual IEnumerable<T> FindAll(Predicate<T> accept)
		{
			foreach (var value in this)
			{
				if (accept(value))
				{
					yield return value;
				}
			}
		}

		#endregion
	}
}
