using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Defines an interface for queues that support blocking.
	/// </summary>
	/// <remarks>
	/// Blocking queues block on all dequeue operations until
	/// an item is available or any specified timeout expires.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	interface ILBlockingQueue<T>
		: ILQueue<T>
	{
		/// <summary>
		/// Gets the next item in the queue.
		/// </summary>
		/// <param name="timeout">The amount of time to wait for the item to dequeue</param>
		/// <exception cref="TimeoutException">If the dequeue operation times out</exception>
		/// <returns>The next item in the queue</returns>
		T Dequeue(int timeout);

		/// <summary>
		/// Gets the next item in the queue.
		/// </summary>
		/// <param name="timeout">The amount of time to wait for the item to dequeue</param>
		/// <exception cref="TimeoutException">If the dequeue operation times out</exception>
		/// <returns>The next item in the queue</returns>
		T Dequeue(TimeSpan timeout);

		/// <summary>
		/// Tries to get the next item in the queue.
		/// </summary>
		/// <param name="timeout">The amount of time to wait for the item to dequeue</param>
		/// <param name="value">A variable in which to store the next item (if available before the timeout period)</param>
		/// <returns>True if an item was returned before the timeout period otherwise false</returns>
		bool TryDequeue(TimeSpan timeout, out T value);
	}
}
