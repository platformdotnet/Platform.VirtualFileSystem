using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Base class for all <see cref="ILDictionary{K,V}"/> implementers.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public abstract class AbstractDictionary<K, V>
		: AbstractCollection<KeyValuePair<K, V>>, ILDictionary<K, V>
	{
		protected IEqualityComparer<K> KeyCompare;

		public virtual event EventHandler<DictionaryEventArgs<K, V>> AfterItemChanged;
		public virtual event EventHandler<DictionaryEventArgs<K, V>> BeforeItemChanged;

		protected virtual void OnAfterItemChanged(DictionaryEventArgs<K, V> eventArgs)
		{
			if (this.AfterItemChanged != null)
			{
				this.AfterItemChanged(this, eventArgs);
			}
		}

		protected virtual void OnBeforeItemChanged(DictionaryEventArgs<K, V> eventArgs)
		{
			if (this.BeforeItemChanged != null)
			{
				this.BeforeItemChanged(this, eventArgs);
			}
		}

	    protected AbstractDictionary()
			: this(null)
		{
		}

	    protected AbstractDictionary(IEqualityComparer<K> keyCompare)
		{
            if (keyCompare == null)
            {
                keyCompare = EqualityComparer<K>.Default;
            }

			this.KeyCompare = keyCompare;
		}

		public override void Add(KeyValuePair<K, V> item)
		{
			this.Add(item.Key, item.Value);
		}

		#region IDictionary<K,V> Members

		public abstract void Add(K key, V value);

		public virtual bool ContainsKey(K key)
		{
            V value;

            return TryGetValue(key, out value);
		}

		ICollection<K> IDictionary<K, V>.Keys
		{
			get
			{
				return new EnumerableCollectionAdapter<K>
				(
					this.Keys,
					this.Count
				);
			}
		}

		public abstract bool Remove(K key);

		public virtual bool TryGetValueOrNot(K key, ref V value)
		{
			try
			{
				value = this[key];

				return true;
			}
			catch (KeyNotFoundException)
			{
				return false;
			}
		}

		public virtual bool TryGetValue(K key, out V value)
		{
			value = default(V);

			return TryGetValueOrNot(key, ref value);
		}

		ICollection<V> IDictionary<K, V>.Values
		{
			get
			{
				return new EnumerableCollectionAdapter<V>
				(
					this.Values,
					this.Count
				);
			}
		}

		public abstract V this[K key]
		{
			get;
			set;
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion

		#region ILDictionary<K,V> Members

		public virtual IEnumerable<K> Keys
		{
			get
			{
				foreach (KeyValuePair<K, V> keyValue in this)
				{
					yield return keyValue.Key;
				}
			}
		}

		public virtual IEnumerable<V> Values
		{
			get
			{
				foreach (KeyValuePair<K, V> keyValue in this)
				{
					yield return keyValue.Value;
				}
			}
		}

		#endregion

		public new ILDictionary<K, V> ToReadOnly()
		{
			return new ReadOnlyDictionary<K, V>(this);
		}
	}
}
