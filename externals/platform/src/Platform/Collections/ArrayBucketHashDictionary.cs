using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A dictionary implementation that uses hashing with linear
	/// array based buckets for hash conflicts.
	/// </summary>
	/// <typeparam name="K">The key type for the dictionary</typeparam>
	/// <typeparam name="V">The value type for the dictionary</typeparam>
	public class ArrayBucketHashDictionary<K, V>
		: AbstractDictionary<K, V>
	{
		protected const int DefaultInitialCapacity = 0x10;

		private struct Bucket
		{
			public int Count;
			public KeyValuePair<K, V>?[] KeyValuePairs;

			public int GetIndexFromKey(K key, Comparison<K> keyCompare, out KeyValuePair<K, V>?[] keyValuePairs)
			{
				keyValuePairs = KeyValuePairs;

				for (int i = 0; i < keyValuePairs.Length; i++)
				{
					if (keyValuePairs[i] != null)
					{
						if (keyCompare(key, keyValuePairs[i].Value.Key) == 0)
						{
							return i;
						}
					}
				}

				return -1;
			}

			public bool RemoveByKey(K key, Comparison<K> keyCompare)
			{
				int index;
				KeyValuePair<K, V>?[] keyValuePairs;

				index = GetIndexFromKey(key, keyCompare, out keyValuePairs);

				if (index < 0)
				{
					return false;
				}

				Count--;
				keyValuePairs[index] = new KeyValuePair<K, V>?();
				
				return true;
			}

			private void Grow()
			{
				int newLength;
				KeyValuePair<K, V>?[] oldKeyValuePairs = this.KeyValuePairs;

				if (this.Count == Int32.MaxValue)
				{
					throw new OutOfMemoryException();
				}

				if (oldKeyValuePairs.Length >= Int32.MaxValue / 2)
				{
					newLength = Int32.MaxValue;
				}
				else
				{
					newLength = oldKeyValuePairs.Length << 1;
				}

				this.Count = 0;
				this.KeyValuePairs = new KeyValuePair<K, V>?[newLength];

				foreach (KeyValuePair<K, V>? keyValuePair in oldKeyValuePairs)
				{
					if (keyValuePair != null)
					{
						Add(keyValuePair.Value.Key, keyValuePair.Value.Value);
					}
				}
			}

			public void Add(K key, V value)
			{
				int index;
				KeyValuePair<K, V>?[] keyValuePairs = this.KeyValuePairs;

				if (keyValuePairs.Length >= Count)
				{
					Grow();
				}

				index = key.GetHashCode() % KeyValuePairs.Length;

				for (; ; )
				{
				}
			}
		}

		private Bucket[] buckets;

		public ArrayBucketHashDictionary()
			: this(DefaultInitialCapacity)
		{
		}

		public ArrayBucketHashDictionary(int initialCapacity)
			: base()
		{
			buckets = new Bucket[initialCapacity];
		}

		private int GetIndexFromKey(K key)
		{
			Bucket[] buckets;

			return GetIndexFromKey(key, out buckets);
		}

		private int GetIndexFromKey(K key, out Bucket[] buckets)
		{
			int startIndex, index;
			
			buckets = this.buckets;

			startIndex = index = Math.Abs(key.GetHashCode()) % buckets.Length;

			for (; ; )
			{
				if (buckets[index].Count > 0)
				{
					return index;					
				}

				index++;

				if (index == startIndex)
				{
					return -1;
				}
			}
		}

		public override void Add(K key, V value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override bool Remove(K key)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override V this[K key]
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
			set
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public override void Clear()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override int Count
		{
			get { throw new Exception("The method or operation is not implemented."); }
		}

		public override bool Remove(KeyValuePair<K, V> item)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}
}