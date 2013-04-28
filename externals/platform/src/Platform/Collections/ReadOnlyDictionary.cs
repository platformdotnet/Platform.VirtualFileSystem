using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A class for creating read-only dictionaries.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	internal class ReadOnlyDictionary<K, V>
		: DictionaryWrapper<K, V>
	{
		public ReadOnlyDictionary(ILDictionary<K, V> wrappee)
			: base(wrappee)
		{
		}

		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public override V this[K key]
		{
			get
			{
				return this.Wrappee[key];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override void Add(K key, V value)
		{
			throw new NotSupportedException();
		}

		public override void Add(KeyValuePair<K, V> item)
		{
			throw new NotSupportedException();
		}

		public override void AddAll(IEnumerable<KeyValuePair<K, V>> items)
		{
			throw new NotSupportedException();
		}

		public override void AddAll(KeyValuePair<K, V>[] items, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void Clear()
		{
			throw new NotSupportedException();
		}

		public override bool Remove(K key)
		{
			throw new NotSupportedException();
		}

		public override bool Remove(KeyValuePair<K, V> item)
		{
			throw new NotSupportedException();
		}

		public override int RemoveAll(IEnumerable<KeyValuePair<K, V>> items)
		{
			throw new NotSupportedException();
		}
	}
}
