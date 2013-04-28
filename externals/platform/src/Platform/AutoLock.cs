using System;
using System.Threading;

namespace Platform
{
	/// <summary>
	/// An <see cref="IAutoLock"/> implementation that uses a monitor for the lock.
	/// </summary>
	public class AutoLock
		: IAutoLock
	{
		/// <summary>
		/// The monitor this <see cref="AutoLock"/> is based upon.
		/// </summary>
		private readonly object lockObject;

		/// <summary>
		/// Creates a new <see cref="AutoLock"/> using the new <see cref="AutoLock"/>
		/// as the monitor for locking.
		/// </summary>
		public AutoLock()			
		{
			lockObject = this;
		}

		/// <summary>
		/// Creates a new <see cref="AutoLock"/> using the supplied object
		/// as the monitor for locking.
		/// </summary>
		/// <param name="lockObject">The object for locking</param>
		public AutoLock(object lockObject)
		{
			this.lockObject = lockObject;
		}

		/// <summary>
		/// Locks the current object's monitor.
		/// </summary>
		/// <returns>The current object</returns>
		public virtual AutoLock Lock()
		{
			return (AutoLock)((IAutoLock)this).Lock();
		}

		/// <summary>
		/// Unlocks the current object's monitor.
		/// </summary>
		/// <returns>The current object</returns>
		public virtual AutoLock Unlock()
		{
			return (AutoLock)((IAutoLock)this).Unlock();
		}

		/// <summary>
		/// Locks the current object's monitor.
		/// </summary>
		/// <returns>The current object</returns>
		IAutoLock IAutoLock.Lock()
		{
			Monitor.Enter(lockObject);

			return this;
		}

		/// <summary>
		/// Unlocks the current object's monitor.
		/// </summary>
		/// <returns>The current object</returns>
		IAutoLock IAutoLock.Unlock()
		{
			Monitor.Exit(lockObject);

			return this;
		}

		/// <summary>
		/// Unlocks the current object's monitor.
		/// </summary>
		void IDisposable.Dispose()
		{
			Unlock();
		}
	}
}
