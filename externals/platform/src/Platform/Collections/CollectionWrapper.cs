using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	public class CollectionWrapper<T>
		: ILCollection<T>
	{
		public virtual IAutoLock GetAutoLock()
		{
			return this.Wrappee.GetAutoLock();
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return this.Wrappee.AquireAutoLock();
		}

		public virtual ILCollection<T> Wrappee
		{
			get
			{
				return wrappee;
			}
			set
			{
				wrappee = value;

				if (wrappee == null)
				{
					return;
				}

				this.Wrappee.AfterItemAdded += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (AfterItemAdded != null)
					{
						AfterItemAdded(this, eventArgs);
					}
				};

				this.Wrappee.BeforeItemAdded += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (BeforeItemAdded != null)
					{
						BeforeItemAdded(this, eventArgs);
					}
				};

				this.Wrappee.AfterItemRemoved += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (AfterItemRemoved != null)
					{
						AfterItemRemoved(this, eventArgs);
					}
				};

				this.Wrappee.BeforeItemRemoved += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (BeforeItemRemoved != null)
					{
						BeforeItemRemoved(this, eventArgs);
					}
				};

				this.Wrappee.AfterCleared += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (AfterCleared != null)
					{
						AfterCleared(this, eventArgs);
					}
				};

				this.Wrappee.BeforeCleared += delegate(object sender, CollectionEventArgs<T> eventArgs)
				{
					if (BeforeCleared != null)
					{
						BeforeCleared(this, eventArgs);
					}
				};
			}
		}
		private ILCollection<T> wrappee;

		public CollectionWrapper(ILCollection<T> wrappee)
		{
			this.Wrappee = wrappee;
		}

		#region ILCollection<T> Members

		public virtual event EventHandler<CollectionEventArgs<T>> AfterItemAdded;	

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeItemAdded;

		public virtual event EventHandler<CollectionEventArgs<T>> AfterItemRemoved;

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeItemRemoved;

		public virtual event EventHandler<CollectionEventArgs<T>> AfterCleared;

		public virtual event EventHandler<CollectionEventArgs<T>> BeforeCleared;

		public virtual object SyncLock
		{
			get
			{
				return this.Wrappee.SyncLock;
			}
		}

		public virtual bool IsEmpty
		{
			get
			{
				return this.Wrappee.IsEmpty;
			}
		}

		public virtual object Clone()
		{
			throw new NotSupportedException();
		}

		public virtual void AddAll(T[] items, int offset, int count)
		{
			this.Wrappee.AddAll(items, offset, count);
		}

		public virtual void AddAll(IEnumerable<T> items)
		{
			this.Wrappee.AddAll(items);
		}

		public virtual int RemoveAll(IEnumerable<T> items)
		{
			return this.Wrappee.RemoveAll(items);
		}

		public virtual bool ContainsAll(IEnumerable<T> items)
		{
			return this.Wrappee.ContainsAll(items);
		}

		public virtual bool ContainsAny(IEnumerable<T> items)
		{
			return this.Wrappee.ContainsAny(items);
		}

		public virtual void ForEach(Action<T> action)
		{
			this.Wrappee.ForEach(action);
		}

		public virtual void ForEach(Action<T> action, Predicate<T> accept)
		{
			this.Wrappee.ForEach(action, accept);
		}

		public virtual void Print(Predicate<T> accept)
		{
			this.Wrappee.Print(accept);
		}

		public virtual void PrintAll()
		{
			this.Wrappee.PrintAll();
		}

		public virtual T[] ToArray()
		{
			return this.Wrappee.ToArray();
		}

		public virtual void CopyTo(T[] array, int arrayIndex, int count)
		{
			this.Wrappee.CopyTo(array, arrayIndex, count);
		}

		public virtual IEnumerable<U> ConvertAll<U>(Converter<T, U> converter)
		{
			return this.Wrappee.ConvertAll<U>(converter);
		}

		#endregion

		#region ICollection<T> Members

		public virtual void Add(T item)
		{
			this.Wrappee.Add(item);
		}

		public virtual void Clear()
		{
			this.Wrappee.Clear();
		}

		public virtual bool Contains(T item)
		{
			return this.Wrappee.Contains(item);
		}

		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			this.Wrappee.CopyTo(array, arrayIndex);
		}

		public virtual int Count
		{
			get
			{
				return this.Wrappee.Count;
			}
		}

		public virtual bool IsReadOnly
		{
			get
			{
				return this.Wrappee.IsReadOnly;
			}
		}

		public virtual bool Remove(T item)
		{
			return this.Wrappee.Remove(item);
		}

		#endregion

		#region IEnumerable<T> Members

		public virtual IEnumerator<T> GetEnumerator()
		{
			return this.Wrappee.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.Wrappee.GetEnumerator();
		}

		#endregion

		public virtual ILCollection<T> ToReadOnly()
		{
			return this.Wrappee.ToReadOnly();
		}

		#region ILCollection<T> Members


		public virtual IEnumerable<T> FindAll(Predicate<T> accept)
		{
			return this.Wrappee.FindAll(accept);
		}

		#endregion
	}
}
