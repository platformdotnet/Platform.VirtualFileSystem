using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Utilities
{
	/// <summary>
	/// Provides a pool of reusable (usually resource of memory intensive) objects
	/// that can be reused/recycled.  The ObjectPool is made up of one or more sets
	/// identified by key.  Each set contains its own unique pool of reusable objects.
	/// Reusable objects are thus identified by a key and their type.  Usually only
	/// one set (the default set) is required, thus objects can be retrieved by only
	/// their type.
	/// </summary>
	/// <typeparam name="V">The base type for objects stored in the pool</typeparam>
	public class RecylingObjectPool<V>
		: IEnumerable<V>
	{
		/// <summary>
		/// Used for indexing keys that are null in the pool dictionary.
		/// </summary>
        private class NullKey
		{
			public static readonly Type Default = typeof(NullKey);
		}

		/// <summary>
		/// A dictionary that holds all the sets int he pool.
		/// </summary>
		private readonly IDictionary<object, IDictionary<Type, Queue<V>>> pool;

		/// <summary>
		/// The object used to synchronize this object pool.
		/// </summary>
		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// The <see cref="ObjectActivator"/> used by the pool to create new
		/// objects if an object is not available and no alternative <see cref="ObjectActivator"/>
		/// is provided by in the <c>GetObject</c> call.
		/// </summary>
		public virtual ObjectActivator<V> ObjectActivator
		{
			get;
			private set;
		}

		public RecylingObjectPool()
			: this(DefaultObjectActivator)
		{
		}

		public RecylingObjectPool(ObjectActivator<V> objectActivator)
		{
			this.ObjectActivator = objectActivator;

			pool = new Dictionary<object, IDictionary<Type, Queue<V>>>();
		}

		public virtual IEnumerator<V> GetEnumerator()
		{
			foreach (IDictionary<Type, Queue<V>> dictionary in pool.Values)
			{
				foreach (Queue<V> queue in dictionary.Values)
				{
					foreach (V obj in queue)
					{
						yield return obj;
					}
				}
			}
		}

		/// <summary>
		/// Clears the pool of objects.
		/// </summary>
		public virtual void Clear()
		{
			pool.Clear();
		}

		/// <summary>
		/// Gets an enumerator for all the objects in the pool.
		/// </summary>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>
		/// An <see cref="ObjectActivator"/> that returns null or the default value for the type
		/// </summary>
		/// <param name="type"></param>
		/// <returns>A newly created object</returns>
		public static V NullObjectActivator(Type type)
		{
			if (!typeof(V).IsAssignableFrom(type))
			{
				throw new ArgumentException(String.Format("Type {0} does not inherit from {1}", type.Name, typeof(V).Name));
			}

			if (typeof(V).IsValueType)
			{
				return typeof(V) == type ? default(V) : (V)Activator.CreateInstance(type);
			}

			return default(V);
		}

		/// <summary>
		/// A <see cref="ObjectActivator"/> that creates an object of type <see cref="type"/>
		/// by using its default constructor.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <returns>A newly created object</returns>
		public static V DefaultObjectActivator(Type type)
		{
			if (!typeof(V).IsAssignableFrom(type))
			{
				throw new ArgumentException(String.Format("Type {0} does not inherit from {1}", type.Name, typeof(V).Name));
			}

			return (V)Activator.CreateInstance(type);
		}

		/// <summary>
		/// Puts an object back into the pool to be available for reuse.
		/// </summary>
		/// <param name="key">
		/// The key for the set in the pool to place the object into
		/// (can be null or default(V) for the default pool)/
		/// </param>
		/// <param name="value">The object to place back into the pool for reuse</param>
		public virtual void Recycle(object key, V value)
		{
			Queue<V> queue;
			IDictionary<Type, Queue<V>> dictionary;

			if (key == null)
			{
				key = NullKey.Default;
			}

			if (!pool.TryGetValue(key, out dictionary))
			{
				dictionary = new Dictionary<Type, Queue<V>>();

				pool[key] = dictionary;
			}

			if (!dictionary.TryGetValue(value.GetType(), out queue))
			{
				queue = new Queue<V>();

				dictionary[value.GetType()] = queue;
			}

			queue.Enqueue(value);
		}

		/// <summary>
		/// Puts an object back into the pool to be available for reuse.  The object
		/// will be placed back into the default set (null key).
		/// </summary>
		/// <param name="value">The object to place back into the pool for reuse</param>
		public virtual void Recycle(V value)
		{
			Recycle(null, value);
		}

		/// <summary>
		/// Gets an object from the pool.  Creates a new object if none is available
		/// in the pool.
		/// </summary>
		/// <typeparam name="T">The type of object to get or create</typeparam>
		/// <param name="key">The key for the set in the pool to use (null to use the default set)</param>
		/// <param name="objectActivator">
		/// An <see cref="ObjectActivator"/> that will be used to create the object if none already exist in the pool
		/// </param>
		/// <returns>An object (either from the pool or newly created)</returns>
		public virtual T GetObject<T>(object key, ObjectActivator<V> objectActivator)
			where T : V
		{
			return (T)GetObject(key, typeof(T), objectActivator);
		}

		/// <summary>
		/// Gets an object from the pool.  Creates a new object if none is available
		/// in the pool.
		/// </summary>
		/// <param name="key">The key for the set in the pool to use (null to use the default set)</param>
		/// <param name="type">The type of object to create</param>
		/// <param name="objectActivator">
		/// An <see cref="ObjectActivator"/> that will be used to create the object if none already exist in the pool
		/// </param>
		/// <returns>An object (either from the pool or newly created)</returns>
		public virtual object GetObject(object key, Type type, ObjectActivator<V> objectActivator)
		{
			Queue<V> queue;
			IDictionary<Type, Queue<V>> dictionary;

			if (key == null)
			{
				key = NullKey.Default;
			}

			if (!pool.TryGetValue(key, out dictionary))
			{
				return objectActivator(type);
			}

			if (!(dictionary.TryGetValue(type, out queue) && queue.Count > 0))
			{
				return objectActivator(type);
			}

			return queue.Dequeue();
		}

		/// <summary>
		/// Gets an object from the pool's default set.  Creates a new object if none is available
		/// in the pool.
		/// </summary>
		/// <typeparam name="T">The type of object to get or create</typeparam>
		/// <param name="objectActivator">
		/// An <see cref="ObjectActivator"/> that will be used to create the object if none already exist in the pool
		/// </param>
		/// <returns>An object (either from the pool or newly created)</returns>
		public virtual T GetObject<T>(ObjectActivator<V> objectActivator)
		{
			return (T)GetObject(typeof(T), objectActivator);
		}

		/// <summary>
		/// Gets an object from the pool's default set.
		/// Creates a new object if none is available in the pool.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <param name="objectActivator">
		/// An <see cref="ObjectActivator"/> that will be used to create the object if none already exist in the pool
		/// </param>
		/// <returns>An object (either from the pool or newly created)</returns>
		public virtual object GetObject(Type type, ObjectActivator<V> objectActivator)
		{
			return GetObject(null, type, objectActivator);
		}

		/// <summary>
		/// Gets an object from the pool's default set.  Uses the pool's default
		/// <see cref="ObjectActivator"/> to create a new object if none is available in the pool.
		/// </summary>
		/// <typeparam name="T">The type of object to create</typeparam>
		/// <returns>An object(either from the pool or newly created)</returns>
		public virtual T GetObject<T>()
		{
			return (T)GetObject(typeof(T));
		}

		/// <summary>
		/// Gets an object from the pool's default set.  Uses the pool's default
		/// <see cref="ObjectActivator"/> to create a new object if none is available in the pool.
		/// </summary>
		/// <param name="type">The type of object to create</param>
		/// <returns>An object(either from the pool or newly created)</returns>
		public virtual object GetObject(Type type)
		{
			return GetObject(type, this.ObjectActivator);
		}
	}
}
