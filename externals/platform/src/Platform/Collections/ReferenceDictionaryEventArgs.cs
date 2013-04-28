using System;

namespace Platform.Collections
{
	public class ReferenceDictionaryEventArgs<K, V>
		: EventArgs
	{
		public K Key
		{
			get;
			private set;
		}

		public V Value
		{
			get;
			private set;
		}

		public ReferenceDictionaryEventArgs(K key, V value)
		{
			this.Key = key;
			this.Value = value;
		}
	}
}
