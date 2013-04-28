using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An interface for objects that can be locked using the <c>using</c> statement
	/// and contain an object that can be used with the lock statement.
	/// </summary>
	public interface ISyncLocked
	{
		/// <summary>
		/// Gets the object used to synchronize the current object.
		/// </summary>
		object SyncLock
		{
			get;
		}

		/// <summary>
		/// Returns the <see cref="IAutoLock"/> but does not acquire it,
		/// </summary>
		/// <returns>The <see cref="IAutoLock"/> for the current object</returns>
		IAutoLock GetAutoLock();

		/// <summary>
		/// Acquires the <see cref="IAutoLock"/> and returns it.
		/// </summary>
		/// <returns>The <see cref="IAutoLock"/> for the current object</returns>
		IAutoLock AquireAutoLock();
	}
}
