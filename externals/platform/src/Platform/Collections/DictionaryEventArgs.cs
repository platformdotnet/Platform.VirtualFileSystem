using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// Holds event information for <see cref="Dictionary{TKey,TValue}"/> events.
	/// </summary>
	/// <typeparam name="K">The type for the keys in the dictionary</typeparam>
	/// <typeparam name="V">The type for values in the dictionary</typeparam>
	public class DictionaryEventArgs<K, V>
		: CollectionEventArgs<V>
	{
		/// <summary>
		/// The related key.
		/// </summary>
		public virtual K Key
		{
			get;
			set;
		}

		/// <summary>
		/// Constructs a new <see cref="DictionaryEventArgs{K,V}"/>.
		/// </summary>
		public DictionaryEventArgs()
		{
		}

		/// <summary>
		/// Constructs a new <see cref="DictionaryEventArgs{K,V}"/> with the provided
		/// key and a default value.
		/// </summary>
		/// <param name="key">The key</param>
		public DictionaryEventArgs(K key)
			: this(key, default(V))
		{
		}

		/// <summary>
		/// Constructs a new <see cref="DictionaryEventArgs{K,V}"/> with the provided key
		/// and value.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="item">The value</param>
		public DictionaryEventArgs(K key, V item)
			: base(item)
		{
		}
	}
}
