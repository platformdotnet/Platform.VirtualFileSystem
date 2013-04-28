using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// An interface implemented by all platform collections.
	/// </summary>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public interface ILDictionary<K, V>
		: ILCollection<KeyValuePair<K, V>>, IDictionary<K, V>
	{
		/// <summary>
		/// An event that is raised before an item in the dictionary changes.
		/// </summary>
		event EventHandler<DictionaryEventArgs<K, V>> AfterItemChanged;

		/// <summary>
		/// An event that is raised after an item in the dictionary changes.
		/// </summary>
		event EventHandler<DictionaryEventArgs<K, V>> BeforeItemChanged;

		/// <summary>
		/// Gets an enumeration of keys.
		/// </summary>
		new IEnumerable<K> Keys
		{
			get;
		}

		/// <summary>
		/// Gets an enumeration of values.
		/// </summary>
		new IEnumerable<V> Values
		{
			get;
		}

		/// <summary>
		/// Converts the dictionary to a read only version.
		/// </summary>
		/// <returns></returns>
		new ILDictionary<K, V> ToReadOnly();

		/// <summary>
		/// Tries to get a value form the dictionary.  If a value is not available
		/// <see cref="value"/> will remain unchanged.
		/// </summary>
		/// <param name="key">The key for the value to retrieve</param>
		/// <param name="value">A variable to store the value</param>
		/// <returns>True if a value is returned otherwise False</returns>
		bool TryGetValueOrNot(K key, ref V value);
	}
}
