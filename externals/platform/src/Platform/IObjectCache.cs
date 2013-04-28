using System;
using System.Collections.Generic;
using System.Text;
using Platform.Collections;

namespace Platform
{
	/// <summary>
	/// An interface for classes that can cache objects.
	/// </summary>
	/// <typeparam name="K">The type used to key objects</typeparam>
	/// <typeparam name="V">The type of objects stored in the cache</typeparam>
	public interface IObjectCache<K, V>
	{
		/// <summary>
		/// Gets the maximum capacity of the cache (may be 0 if unknown).
		/// </summary>
		int MaximumCapacity
		{
			get;
		}

		/// <summary>
		/// Gets the amount of objects in the cache.
		/// </summary>
		int Count
		{
			get;
		}

		/// <summary>
		/// Adds an object to the cache.
		/// </summary>
		/// <param name="key">The key for the object</param>
		/// <param name="value">The object</param>
		void Push(K key, V value);

		/// <summary>
		/// Removes an object from the cache.
		/// </summary>
		/// <param name="key">The key for the object</param>
		/// <returns>True if the object was found and removed</returns>
		bool Evict(K key);

		/// <summary>
		/// Flushes the cache.
		/// </summary>
		void Flush();
	}
}
