using System;
using System.Collections.Generic;
using System.Text;
using Platform;
using Platform.References;

namespace Platform.Collections
{	
	/// <summary>
	/// Base class for dictionaries that store items using <see cref="Reference{T}"/> objects.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	/// <typeparam name="R">The <see cref="Reference{T}"/> type to use</typeparam>
	public abstract class ReferenceDictionary<K, V, R>
		: AbstractDictionary<K, V>, IObjectCache<K, V>
		where V : class
		where R : Reference<V>, IKeyed<K>
	{
		protected IDictionary<K, R> dictionary;
		protected ReferenceQueue<V> referenceQueue;

		public event EventHandler<ReferenceDictionaryEventArgs<K, V>> ReferenceCleaned;

		protected virtual void OnReferenceCleaned(ReferenceDictionaryEventArgs<K, V> eventArgs)
		{
			if (ReferenceCleaned != null)
			{
				ReferenceCleaned(this, eventArgs);
			}
		}

		public override object SyncLock
		{
			get
			{
				if (dictionary is ISyncLocked)
				{
					return ((ISyncLocked)dictionary).SyncLock;
				}
				else
				{
					return dictionary;
				}
			}
		}

		public override bool Contains(KeyValuePair<K, V> item)
		{
			V value;

			if (!this.TryGetValue(item.Key, out value))
			{
				return false;
			}
			
			if (value == null && item.Value == null)
			{
				return true;
			}

			if (value == null)
			{
				return false;
			}

			if (!value.Equals(item.Value))
			{
				return false;
			}

			return true;
		}

		public override bool ContainsKey(K key)
		{
			V value;

			if (!this.TryGetValue(key, out value))
			{
				return false;
			}

			return true;
		}

		protected ReferenceDictionary(Type openGenericDictionaryType, params object[] constructorArgs)
		{
			Type type;

			type = openGenericDictionaryType.MakeGenericType(typeof(K), typeof(R));
			
			referenceQueue = new ReferenceQueue<V>();

			dictionary = (IDictionary<K, R>)Activator.CreateInstance(type, constructorArgs);
		}

		protected abstract R CreateReference(K key, V value);

		private bool suspendCleanDeadReferences = false;

		protected virtual bool CleanDeadReferences()
		{
			IKeyed<K> keyed;

			if (suspendCleanDeadReferences)
			{
				return false;
			}

			bool retval = false;

			lock (this.SyncLock)
			{
				suspendCleanDeadReferences = true;

				while ((keyed = (IKeyed<K>)referenceQueue.Dequeue(0)) != null)
				{
					Remove(keyed.Key);

					retval = true;
				}

				suspendCleanDeadReferences = false;
			}

			return retval;
		}

		public override void Add(K key, V value)
		{
			lock (this.SyncLock)
			{
				CleanDeadReferences();

				dictionary.Add(key, CreateReference(key, value));
			}
		}

		public override bool Remove(K key)
		{
			lock (this.SyncLock)
			{
				CleanDeadReferences();

				return dictionary.Remove(key);
			}
		}

		public override V this[K key]
		{
			get
			{
				V value;

				if (!TryGetValue(key, out value))
				{
					throw new KeyNotFoundException(key.ToString());
				}
				else
				{
					return value;
				}
			}
			set
			{
				lock (this.SyncLock)
				{
					dictionary[key] = CreateReference(key, value);
				}
			}
		}

		public override bool TryGetValue(K key, out V value)
		{
			lock (this.SyncLock)
			{
				R reference;

				if (!dictionary.TryGetValue(key, out reference))
				{
					value = null;

					return false;
				}

				value = reference.Target;

				return value != null;
			}
		}

		public override void Clear()
		{
			lock (this.SyncLock)
			{
				dictionary.Clear();
			}
		}

		public override int Count
		{
			get
			{
				lock (this.SyncLock)
				{
					CleanDeadReferences();

					return dictionary.Count;
				}
			}
		}

		public override bool Remove(KeyValuePair<K, V> item)
		{
			lock (this.SyncLock)
			{
				return dictionary.Remove(new KeyValuePair<K, R>(item.Key, CreateReference(item.Key, item.Value)));
			}
		}

		public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			foreach (KeyValuePair<K, R> keyValuePair in dictionary)
			{
				V value;

				value = keyValuePair.Value.Target;

				if (value != null)
				{
					yield return new KeyValuePair<K, V>(keyValuePair.Key, value);
				}
			}
		}

		/// <summary>
		/// Adds an object to the cache.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The object</param>
		public virtual void Push(K key, V value)
		{
			lock (this.SyncLock)
			{
				this[key] = value;
			}
		}

		/// <summary>
		/// Evicts an object from the cache
		/// </summary>
		/// <param name="key">The key</param>
		/// <returns>True if the object was found and removed</returns>
		public bool Evict(K key)
		{
			lock (this.SyncLock)
			{
				return this.Remove(key);
			}
		}

		/// <summary>
		/// Clears the cache.
		/// </summary>
		public void Flush()
		{
			lock (this.SyncLock)
			{
				this.Clear();
			}
		}

		/// <summary>
		/// Returns <see cref="int.MaxValue"/>.
		/// </summary>
		public int MaximumCapacity
		{
			get
			{
				return Int32.MaxValue;
			}
		}
	}
}
