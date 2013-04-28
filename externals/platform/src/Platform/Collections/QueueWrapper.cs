using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	public class QueueWrapper<T>
		: CollectionWrapper<T>, ILQueue<T>
	{
		public new ILQueue<T> Wrappee
		{
			get
			{
				return (ILQueue<T>)base.Wrappee;
			}
			set
			{
				base.Wrappee = value;
			}
		}

		public QueueWrapper(ILQueue<T> wrappee)
			: base(wrappee)
		{
		}

		#region ILQueue<T> Members

		public virtual void Enqueue(T item)
		{
			this.Wrappee.Enqueue(item);
		}

		public virtual void Enqueue(T[] items, int offset, int count)
		{
			this.Wrappee.Enqueue(items, offset, count);
		}

		public virtual T Dequeue()
		{
			return this.Wrappee.Dequeue();
		}

		public virtual bool TryDequeue(out T item)
		{
			return this.Wrappee.TryDequeue(out item);
		}

		public virtual int TryDequeue(T[] items, int offset, int count)
		{
			return this.Wrappee.TryDequeue(items, offset, count);
		}

		public T Peek()
		{
			return this.Wrappee.Peek();
		}

		public bool TryPeek(out T value)
		{
			return this.Wrappee.TryPeek(out value);
		}

		#endregion
	}
}
