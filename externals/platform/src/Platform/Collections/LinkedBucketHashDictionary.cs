using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// A hash table based dictionary that resolves hash conflicts
	/// by using a singly linked list based bucket.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
    public class LinkedBucketHashDictionary<K, V>
        : AbstractDictionary<K, V>
    {
        protected const float DefaultLoadFactor = 0.75f;
        protected const int DefaultInitialCapacity = 47;

        private int count;
        private readonly float loadFactor;

        private Bucket[] table;

        private class Bucket
        {
            public Bucket Next;

            public KeyValuePair<K, V> KeyValuePair;
        }

        public LinkedBucketHashDictionary()
            : this(DefaultInitialCapacity)
        {
        }

        public LinkedBucketHashDictionary(int initialCapacity)
            : this(initialCapacity, null)
        {
        }

        public LinkedBucketHashDictionary(int initialCapacity, IEqualityComparer<K> keyCompare)
            : this(initialCapacity, DefaultLoadFactor, keyCompare)
        {
        }

        public LinkedBucketHashDictionary(int initialCapacity, float loadFactor, IEqualityComparer<K> keyCompare)
            : base(keyCompare)
        {
            if (initialCapacity < DefaultInitialCapacity)
            {
                initialCapacity = DefaultInitialCapacity;
            }

            this.loadFactor = loadFactor;
            table = new Bucket[initialCapacity];
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
            table = new Bucket[newLength];

            foreach (var bucket in oldTable)
            {
                var subBucket = bucket;

                while (subBucket != null)
                {
                    Add(subBucket.KeyValuePair.Key, subBucket.KeyValuePair.Value);

                    subBucket = subBucket.Next;
                }
            }
        }

		public override void Add(K key, V value)
		{
			Add(key, value, true);
		}

        private void Add(K key, V value, bool add)
        {
        	if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var localBuckets = this.table;

            if (((float)count / (float)localBuckets.Length) >= loadFactor)
            {
                Grow();

                localBuckets = this.table;
            }
                        
            if (this.InvokeBeforeItemAddedRequired)
            {
                OnBeforeItemAdded(new CollectionEventArgs<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value)));
            }

            var index = (this.KeyCompare.GetHashCode(key) & 0x7fffffff) % localBuckets.Length;

            var bucket = localBuckets[index];

            if (bucket == null)
            {
                bucket = localBuckets[index] = new Bucket();                
            }
            else
            {
                Bucket previousBucket = null;

                do
                {
                    if (KeyCompare.Equals(key, bucket.KeyValuePair.Key))
					{
						if (add)
						{
							throw new ArgumentException("Duplicate Key");
						}

						break;
                    }

                    previousBucket = bucket;
                    bucket = bucket.Next;
                }
                while (bucket != null);

				if (bucket == null)
				{
					bucket = new Bucket();

					count++;

					if (previousBucket != null)
					{
						previousBucket.Next = bucket;
					}
				}
            }

            bucket.KeyValuePair = new KeyValuePair<K, V>(key, value);
            
            if (this.InvokeAfterItemAddedRequired)
            {
                OnAfterItemAdded(new CollectionEventArgs<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value)));
            }
        }
        
        public override bool Remove(KeyValuePair<K, V> item)
        {
            return Remove(item.Key);
        }

        public override bool Remove(K key)
        {
        	KeyValuePair<K, V> keyValuePair;

            var localTable = this.table;
            var index = (this.KeyCompare.GetHashCode(key) & 0x7fffffff) % localTable.Length;

            Bucket parentBucket = null;
            var bucket = localTable[index];

            while (bucket != null)
            {
                if (this.KeyCompare.Equals(bucket.KeyValuePair.Key, key))
                {
                    break;
                }

                parentBucket = bucket;
                bucket = bucket.Next;                
            }

            if (bucket == null)
            {
                return false;
            }
            
            if (this.InvokeBeforeItemRemovedRequired)
            {
                keyValuePair = new KeyValuePair<K, V>(key, bucket.KeyValuePair.Value);

                OnBeforeItemRemoved(new CollectionEventArgs<KeyValuePair<K, V>>(keyValuePair));
            }

            if (parentBucket == null)
            {
                localTable[index] = null;
            }
            else
            {
                parentBucket.Next = bucket.Next;
            }

            count--;

            if (this.InvokeAfterItemRemovedRequired)
            {
                keyValuePair = new KeyValuePair<K, V>(key, bucket.KeyValuePair.Value);

                OnAfterItemRemoved(new CollectionEventArgs<KeyValuePair<K, V>>(keyValuePair));
            }

            return true;
        }

        public override V this[K key]
        {
            get
            {
                V retval = default(V);

                if (TryGetValueOrNot(key, ref retval))
                {
                    return retval;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                Add(key, value, false);
            }
        }

        public override void Clear()
        {
            if (this.InvokeAfterItemRemovedRequired || this.InvokeBeforeItemRemovedRequired)
            {
               
            }
            else
            {
               
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
            Bucket bucket;
            var localTable = this.table;

            for (int i = 0; i < localTable.Length; i++)
            {
                bucket = localTable[i];

                while (bucket != null)
                {
                    yield return bucket.KeyValuePair;

                    bucket = bucket.Next;
                }
            }
        }

        public override bool Contains(KeyValuePair<K, V> item)
        {
            V value;

            if (TryGetValue(item.Key, out value))
            {
                return Object.Equals(item.Value, value);
            }

            return false;            
        }

        public override bool TryGetValueOrNot(K key, ref V value)
        {
        	var localTable = this.table;
			var index = (this.KeyCompare.GetHashCode(key) & 0x7fffffff) % localTable.Length;
            var bucket = localTable[index];

            while (bucket != null)
            {
                if (this.KeyCompare.Equals(bucket.KeyValuePair.Key, key))
                {
                    value = bucket.KeyValuePair.Value;

                    return true;
                }

                bucket = bucket.Next;
            }

            return false;
        }

        public override object Clone()
        {
			var clone = new LinkedBucketHashDictionary<K, V>(this.table.Length, this.loadFactor, this.KeyCompare);

			foreach (var keyValuePair in this)
			{
				clone.Add(keyValuePair);
			}

			return clone;
        }
    }
}
