using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Adapts any <see cref="ILQueue{T}"/> implementation into a queue 
	/// which which supports blocked dequeue operations.
	/// </summary>
	/// <remarks>
	/// Blocking queues block on all dequeue operations until
	/// an item is available or any specified timeout expires.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public class BlockingQueueAdapter<T>
		: QueueWrapper<T>, ILBlockingQueue<T>
	{
		private readonly object monitor = new object();

		public BlockingQueueAdapter(ILQueue<T> wrappee)
			: base(wrappee)
		{
			this.AfterItemAdded += delegate
			{
				lock (monitor)
				{
					Monitor.PulseAll(monitor);
				}
			};		
		}

		public override void Enqueue(T item)
		{
			lock (this.SyncLock)
			{
				base.Enqueue(item);	
			}

			lock (monitor)
			{
				Monitor.Pulse(monitor);
			}
		}

		public override void Enqueue(T[] items, int offset, int count)
		{
			lock (this.SyncLock)
			{
				base.Enqueue(items, offset, count);	
			}

			lock (monitor)
			{				
				Monitor.Pulse(monitor);
			}
		}

		public override T Dequeue()
		{
			return Dequeue(Timeout.Infinite);
		}

		public virtual T Dequeue(int timeout)
		{
			return Dequeue(TimeSpan.FromMilliseconds(timeout));
		}

		public virtual T Dequeue(TimeSpan timeout)
		{
			T value;

			if (TryDequeue(timeout, out value))
			{
				return value;
			}
			else
			{
				throw new TimeoutException();
			}
		}

		public override bool TryDequeue(out T value)
		{
			return TryDequeue(TimeSpan.FromMilliseconds(Timeout.Infinite), out value);
		}

		public virtual bool TryDequeue(TimeSpan timeout, out T value)
		{
			lock (monitor)
			{
				for (;;)
				{
					if (this.Count == 0)
					{
						if (!Monitor.Wait(monitor, timeout))
						{
							value = default(T);

							return false;
						}
					}
					else
					{
						break;	
					}
				}
			}

			lock (this.SyncLock)
			{
				value = base.Dequeue();
			}

			return true;
		}		
	}
}
