using System;
using System.Threading;

namespace Platform.References
{
	/// <summary>
	/// A concrete implementation of <see cref="IReferenceQueue{T}"/>.  This class
	/// provides queue like interface for accessing references as they are
	/// enqueued.  The <see cref="Dequeue()"/> methods all block until a <see cref="Reference{T}"/>
	/// is next available on the queue.
	/// </summary>
	public class ReferenceQueue<T>
		: Platform.Collections.AbstractQueue<Reference<T>>, IReferenceQueue<T>
		where T : class
	{
		private Reference<T> firstReference;
		private Reference<T> lastReference;

		public override void Clear()
		{
			throw new NotSupportedException();
		}

		public override System.Collections.Generic.IEnumerator<Reference<T>> GetEnumerator()
		{
			throw new NotSupportedException();
		}

		public override int Count
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override bool TryPeek(out Reference<T> value)
		{
			lock (this.SyncLock)
			{
				if (firstReference == null)
				{
					value = null;

					return false;
				}

				value = firstReference;

				return true;
			}
		}

		public override bool TryDequeue(out Reference<T> value)
		{
			lock (this.SyncLock)
			{
				if (firstReference == null || lastReference == null)
                {
					value = null;

                	return false;
                }

				value = firstReference;

				if (firstReference.nextOnQueue == firstReference)
				{
					firstReference = lastReference = null;
				}
				else
				{
					firstReference = firstReference.nextOnQueue;
				}

				return true;
			}
		}

		public override Reference<T> Dequeue()
		{
			return Dequeue(Timeout.Infinite);
		}

		public virtual Reference<T> Dequeue(int timeout)
		{
			return Dequeue(TimeSpan.FromMilliseconds(timeout));
		}

		public virtual Reference<T> Dequeue(TimeSpan timeout)
		{
			Reference<T> retval;

			if (TryDequeue(timeout, out retval))
			{
				return retval;
			}

			return null;
		}

		public virtual bool TryDequeue(TimeSpan timeout, out Reference<T> value)
		{
			lock (this.SyncLock)
			{
				for (;;)
				{
					if (!TryDequeue(out value))
					{
						if (!Monitor.Wait(this.SyncLock, timeout))
						{
							return false;
						}

						continue;
					}

					return true;
				}
			}
		}

		public override void Enqueue(Reference<T> reference)
		{
			lock (this.SyncLock)
			{
				if (firstReference == null)
				{
					firstReference = lastReference = reference;
					reference.nextOnQueue = reference;
				}
				else
				{
					lastReference.nextOnQueue = reference;
					reference.nextOnQueue = reference;
					lastReference = reference;
				}

				Monitor.PulseAll(this.SyncLock);
			}
		}
	}
}
