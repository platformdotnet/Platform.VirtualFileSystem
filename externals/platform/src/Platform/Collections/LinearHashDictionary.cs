using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A hash table based dictionary that resolves hash conflicts by linear probing.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public class LinearHashDictionary<K, V>
		: AbstractDictionary<K, V>
	{
        protected const float DefaultLoadFactor = 0.6f;
        protected const int DefaultInitialCapacity = 47;

		private int count;
        private readonly float loadFactor;
		private KeyValuePair<K, V>?[] table;

		public LinearHashDictionary()
			: this(DefaultInitialCapacity)
		{
		}

		public LinearHashDictionary(int initialCapacity)
			: this(initialCapacity, null)
		{
		}

        public LinearHashDictionary(int initialCapacity, IEqualityComparer<K> keyCompare)
            : this(initialCapacity, DefaultLoadFactor, keyCompare)
        {
        }

        public LinearHashDictionary(int initialCapacity, float loadFactor, IEqualityComparer<K> keyCompare)
			: base(keyCompare)
		{
			if (initialCapacity < DefaultInitialCapacity)
			{
				initialCapacity = DefaultInitialCapacity;
			}

            this.loadFactor = loadFactor;
			table = new KeyValuePair<K, V>?[initialCapacity];
		}

		private void Grow()
		{
			int newLength;
			var oldTable = table;

			if (count == Int32.MaxValue)
			{
				throw new OutOfMemoryException();
			}

			if (table.Length >= Int32.MaxValue / 2)
			{
				newLength = Int32.MaxValue;
			}
			else
			{
				newLength = oldTable.Length << 1;
			}

			count = 0;
			table = new KeyValuePair<K, V>?[newLength];

			foreach (var keyValuePair in oldTable)
			{
				if (keyValuePair.HasValue)
				{
					Add(keyValuePair.Value.Key, keyValuePair.Value.Value);
				}
			}	
		}

		public override void Add(K key, V value)
		{
		    if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			
			var currentTable = this.table;

			if (((float)count / (float)currentTable.Length) >= loadFactor)
			{
				Grow();

				currentTable = this.table;
			}

			if (this.InvokeBeforeItemAddedRequired)
			{
				OnBeforeItemAdded(new CollectionEventArgs<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value)));
			}

            var index = (this.KeyCompare.GetHashCode(key) & 0x7fffffff) % currentTable.Length;

			for (;;)
			{
				if (!currentTable[index].HasValue
					|| (currentTable[index].HasValue && currentTable[index].Value.Key.Equals(key)))
				{
					currentTable[index] = new KeyValuePair<K, V>(key, value);

					count++;

					break;
				}

                index++;
				index %= currentTable.Length;
			}

			if (this.InvokeAfterItemAddedRequired)
			{
				OnAfterItemAdded(new CollectionEventArgs<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value)));
			}
		}

		private int GetIndexFromKey(K key)
		{
			KeyValuePair<K, V>?[] table;

			return GetIndexFromKey(key, out table);
		}

		private int GetIndexFromKey(K key, out KeyValuePair<K, V>?[] table)
		{
			int index;
			table = this.table;

            var startIndex = index = (KeyCompare.GetHashCode(key) & 0x7fffffff) % table.Length;

			for (;;)
			{
                if (table[index].HasValue)
                {
                    if (KeyCompare.Equals(table[index].Value.Key, key))
                    {
                        return index;
                    }
                }
                else
                {
                    return -1;
                }

				index++;
				index %= table.Length;

				if (index == startIndex)
				{
					return -1;
				}
			}
		}

		public override bool Remove(K key)
		{
		    KeyValuePair<K, V>?[] currentTable;

			var index = GetIndexFromKey(key, out currentTable);

			if (index == -1)
			{
				return false;
			}

			return RemoveAt(index, currentTable);
		}

		public override bool Remove(KeyValuePair<K, V> item)
		{
			return Remove(item.Key);
		}

		private bool RemoveAt(int index, KeyValuePair<K, V>?[] currentTable)
		{
			KeyValuePair<K, V> keyValuePair;

			if (this.InvokeBeforeItemRemovedRequired)
			{
				keyValuePair = new KeyValuePair<K, V>(this.table[index].Value.Key, this.table[index].Value.Value);

				OnBeforeItemRemoved(new CollectionEventArgs<KeyValuePair<K, V>>(keyValuePair));
			}

			count--;
			currentTable[index] = null;
			
			if (this.InvokeAfterItemRemovedRequired)
			{
				keyValuePair = new KeyValuePair<K, V>(this.table[index].Value.Key, this.table[index].Value.Value);

				OnAfterItemRemoved(new CollectionEventArgs<KeyValuePair<K, V>>(keyValuePair));
			}

			return true;
		}

		public override V this[K key]
		{
			get
			{
			    KeyValuePair<K, V>?[] currentTable;

				var index = GetIndexFromKey(key, out currentTable);

				if (index < 0)
				{
					throw new KeyNotFoundException(key.ToString());
				}

				return currentTable[index].Value.Value;
			}
			set
			{
				Add(key, value);
			}
		}

		public override void Clear()
		{
			if (this.InvokeAfterItemRemovedRequired || this.InvokeBeforeItemRemovedRequired)
			{
				var currentTable = this.table;

				for (var i = 0; i < currentTable.Length; i++)
				{
					if (currentTable[i].HasValue)
					{
						RemoveAt(i, currentTable);
					}
				}
			}
			else
			{
				count = 0;

				Array.Clear(table, 0, table.Length);
			}
		}

		public override int Count
		{
			get
			{
				return count;
			}
		}

		public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			foreach (KeyValuePair<K, V>? keyValuePair in table)
			{
				if (keyValuePair.HasValue)
				{
					yield return keyValuePair.Value;
				}
			}
		}

		public override bool Contains(KeyValuePair<K, V> item)
		{
			return GetIndexFromKey(item.Key) >= 0;
		}

		public override bool TryGetValueOrNot(K key, ref V value)
		{
		    KeyValuePair<K, V>?[] currentTable;

			int index = GetIndexFromKey(key, out currentTable);

			if (index < 0)
			{
				return false;
			}

			value = currentTable[index].Value.Value;

			return true;
		}

		public override object Clone()
		{
		    var clone = (LinearHashDictionary<K, V>)this.MemberwiseClone();
			var newTable = new KeyValuePair<K, V>?[clone.table.Length];

			Array.Copy(clone.table, newTable, newTable.Length);

			clone.table = newTable;

			return clone;
		}
	}
}
