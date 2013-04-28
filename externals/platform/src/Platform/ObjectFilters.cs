using System;

namespace Platform
{
	/// <summary>
	/// Provides methods for creating useful predicates
	/// </summary>
	public static class ObjectFilters
	{
		/// <summary>
		/// Returns a predicate that returns true if a given <see cref="INamed"/>
		/// has a <see cref="INamed.Name"/> that matches the provided <see cref="name"/>.
		/// </summary>
		/// <typeparam name="T">The object type the predicate should accept</typeparam>
		/// <param name="name">The name to match with</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> ByName<T>(string name)
			where T : INamed
		{
			return obj => obj.Name == name;
		}

		/// <summary>
		/// Returns a predicate that returns true if a given <see cref="IValued"/>
		/// has a <see cref="IValued.Value"/> that matches the provided <see cref="value"/>.
		/// </summary>
		/// <typeparam name="T">The object type the predicate should accept</typeparam>
		/// <param name="value">The value to match with</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> ByValue<T, V>(V value)
			where T : IValued<V>

		{
			return obj => Object.Equals(obj.Value, value);
		}

		/// <summary>
		/// Returns a predicate that returns true if a given <see cref="INamed"/>
		/// has a <see cref="IKeyed.Key"/> that matches the provided <see cref="key"/>.
		/// </summary>
		/// <typeparam name="T">The object type the predicate should accept</typeparam>
		/// <param name="key">The key to match with</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> ByKey<T, K>(K key)
			where T : IKeyed<K>
		{
			return obj => Object.Equals(obj.Key, key);
		}
	}
}
