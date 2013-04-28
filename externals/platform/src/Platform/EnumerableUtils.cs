using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and static utility methods for enumerable types.
	/// </summary>
	public static class EnumerableUtils
	{
		/// <summary>
		/// Returns an empty IEnumerable
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> Null<T>()
		{
			yield break;
		}

		/// <summary>
		/// Compares two enumerables to see if their elements are equal
		/// </summary>
		/// <param name="left">The first enumerable to compare</param>
		/// <param name="right">The second enumerable to compare</param>
		/// <returns>
		/// True if the elements in both enumerables are equal
		/// </returns>
		public static bool ElementsAreEqual(this IEnumerable left, IEnumerable right)
		{
			var rightEnumerator = right.GetEnumerator();

			foreach (object value in left)
			{
				if (!rightEnumerator.MoveNext())
				{
					return false;
				}

				if (!Object.Equals(value, rightEnumerator.Current))
				{
					return false;
				}
			}

			if (rightEnumerator.MoveNext())
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Performs an action on each item in the supplied <see cref="IEnumerable"/>
		/// </summary>
		/// <param name="enumerables">The source of items</param>
		/// <param name="action">The action to perform on each item</param>
		public static void ForEach<T>(this IEnumerable<T> enumerables, Action<T> action)
		{
			foreach (T value in enumerables)
			{
				action(value);
			}
		}

		/// <summary>
		/// Prints all elements in the enumerable to the Console.
		/// </summary>
		/// <typeparam name="T">The type of enumerable</typeparam>
		/// <param name="enumerable">The enumerable to print</param>
		public static void Print<T>(this IEnumerable<T> enumerable)
		{
			ForEach(enumerable, (T value) => Console.WriteLine(value) );
		}

		/// <summary>
		/// Joins all the elements in the enumerable as a string
		/// </summary>
		/// <typeparam name="T">The element type of the enumerable</typeparam>
		/// <param name="enumerable">The eumerator</param>
		/// <param name="binder">The string that will appear between elements</param>
		/// <returns>
		/// A string representation of the enumerable
		/// </returns>
		public static string JoinToString<T>(this IEnumerable<T> enumerable, string binder)
		{
			var builder = new StringBuilder();

			return enumerable.Convert<T, string>(ObjectUtils.ToString).ComplexFold
				(
					delegate(string value)
					{
						if (builder.Length != 0)
						{
							builder.Append(binder);
						}

						builder.Append(value);

						return () => builder.ToString();
					}
				);
		}

		/// <summary>
		/// Fold all the elements in the enumerable using the given combiner
		/// </summary>
		/// <remarks>
		/// Each element in the enumerable will be passed to the combiner.  The combiner
		/// should return a function that on will evaluate to the current result if called.
		/// The function returned by the combiner will only be called once the last element
		/// of the enumerable has been evaluated by the combiner.
		/// </remarks>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable">The enumerable</param>
		/// <param name="combiner">The combiner</param>
		/// <returns>
		/// The result of the addition
		/// </returns>
		public static T ComplexFold<T>(this IEnumerable<T> enumerable, Func<T, Func<T>> combiner)
		{
			Func<T> retval = null;

			foreach (T value in enumerable)
			{
				retval = combiner(value);
			}

			if (retval != null)
			{
				return retval();
			}
			else
			{
				return default(T);
			}
		}

		public static T Fold<T>(this IEnumerable<T> enumerable, Func<T, T, T> operation)
		{
			T retval;
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				return default(T);
			}

			retval = enumerator.Current;

			while (enumerator.MoveNext())
			{
				retval = operation(retval, enumerator.Current);
			}

			return retval;
		}

		public static T Fold<T>(this IEnumerable<T> enumerable, T initial, Func<T, T, T> operation)
		{
			T retval = initial;

			foreach (T value in enumerable)
			{
				retval = operation(retval, value);
			}

			return retval;
		}

		public static IEnumerable<T> Tail<T>(this IEnumerable<T> enumerable)
		{
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				yield break;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		public static U Fold<T, U>(this IEnumerable<T> enumerable, Converter<T, U> converter, Func<U, U, U> operation)
		{
			U retval;
			var enumerator = enumerable.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				return default(U);
			}

			retval = converter(enumerator.Current);

			while (enumerator.MoveNext())
			{
				retval = operation(retval, converter(enumerator.Current));
			}

			return retval;
		}

		public static U Fold<T, U>(this IEnumerable<T> enumerable, Converter<T, U> converter, U initial, Func<U, U, U> operation)
		{
			U retval = initial;

			foreach (T value in enumerable)
			{
				retval = operation(retval, converter(value));
			}

			return retval;
		}

		/// <summary>
		/// Walks through an enumeration performing no action on each item
		/// </summary>
		/// <remarks>
		/// This method is equivalent to the following call:
		/// <para>
		/// <code>
		/// ForEach(enumerables, ActionUtils<T>.Null);
		/// </code>
		/// </para>
		/// </remarks>
		/// <param name="enumerables"></param>
		public static void Consume<T>(this IEnumerable<T> enumerables)
		{
			ForEach(enumerables, ActionUtils<T>.Null);
		}

		/// <summary>
		/// Converts an IEnumerable from one type to another using the supplied converter
		/// </summary>
		/// <typeparam name="T">The original type of the enumerable</typeparam>
		/// <typeparam name="U">The resultant type of the enumerable</typeparam>
		/// <param name="source">The source enumerable</param>
		/// <param name="converter">
		/// The converter that will convert elements of the source enumerable
		/// type into elements of the return enumerable type
		/// </param>
		/// <returns>
		/// A new enumerable containing the converted elements from the original enumerable
		/// </returns>
		public static IEnumerable<U> Convert<T, U>(this IEnumerable<T> source, Converter<T, U> converter)
		{
			foreach (T value in source)
			{
				yield return converter(value);
			}
		}

		/// <summary>
		/// Converts an IEnumerable from one type to another using the supplied converter
		/// </summary>
		/// <typeparam name="T">The original type of the enumerable</typeparam>
		/// <typeparam name="U">The resultant type of the enumerable</typeparam>
		/// <param name="source">The source enumerable</param>
		/// <param name="converter">
		/// The converter that will convert elements of the source enumerable
		/// type into elements of the return enumerable type
		/// </param>
		/// <returns>
		/// A new enumerable containing the converted elements from the original enumerable
		/// </returns>
		public static IEnumerable<U> Convert<T, U>(this IEnumerable source, Converter<T, U> converter)
		{
			foreach (T value in source)
			{
				yield return converter(value);
			}
		}

		/// <summary>
		/// Copy an IEnumerable to a collection class
		/// </summary>
		/// <typeparam name="T">
		/// The type of the enumerable to copy into a collection
		/// </typeparam>
		/// <typeparam name="R">
		/// The collection type to return
		/// </typeparam>
		public static R CopyTo<T, R>(this IEnumerable<T> enumerable)
			where R : ICollection<T>, new()
		{
			R retval = new R();

			foreach (T value in enumerable)
			{
				retval.Add(value);
			}

			return retval;
		}

		/// <summary>
		/// Chains two or more enumerables into a single enumerable
		/// </summary>
		/// <param name="enumerables">The array of enumerable objects to chain</param>		
		public static IEnumerable<T> Chain<T>(params IEnumerable<T>[] enumerables)
		{
			foreach(IEnumerable<T> enumerable in enumerables)
			{
				foreach (T value in enumerable)
				{
					yield return value;
				}
			}
		}

		/// <summary>
		/// Appends one or more enumerables to the curren enumerable
		/// </summary>
		/// <param name="first">The first enumerable to append the other enumerables to</param>
		/// <param name="enumerables">The enumerables to append to the first</param>		
		public static IEnumerable<T> Append<T>(this IEnumerable<T> first, params IEnumerable<T>[] enumerables)
		{
			foreach (T eValue in first)
			{
				yield return eValue;
			}

			foreach (IEnumerable<T> enumerable in enumerables)
			{
				foreach (T value in enumerable)
				{
					yield return value;
				}
			}
		}

		/// <summary>
		/// Appends a single element into the end of a given enumerable
		/// </summary>
		/// <param name="enumerable">The enumerable to append the element to</param>
		/// <param name="value">The value to append to the end of the enumerable</param>
		public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T value)
		{
			foreach (T eValue in enumerable)
			{
				yield return eValue;
			}

			yield return value;
		}

		/// <summary>
		/// Prepends a single element into the end of a given enumerable
		/// </summary>
		/// <param name="enumerable">The enumerable to append the element to</param>
		/// <param name="value">The value to prepend to the end of the enumerable</param>
		public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T value)
		{
			yield return value;

			foreach (T eValue in enumerable)
			{
				yield return eValue;
			}
		}

		/// <summary>
		/// Converts an untyped IEnumerable into a typed IEnumerable<T>
		/// </summary>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static IEnumerable<T> ToTyped<T>(this IEnumerable enumerable)
		{
			foreach (T value in enumerable)
			{
				yield return value;
			}
		}

		/// <summary>
		/// Checks if an IEnumerable contains a value
		/// </summary>
		public static bool Contains<T>(this IEnumerable<T> enumerable, T value)
		{
			return Contains
				(
					enumerable,
					value,
					(left, right) => left.Equals(right) ? 0 : 1
				);
		}

		/// <summary>
		/// Checks if an enumerable contains a given value using the given comparison
		/// </summary>
		/// <typeparam name="T">The element type for the enumerable</typeparam>
		/// <param name="enumerable">The enumerable to search</param>
		/// <param name="value">The value to search for</param>
		/// <param name="comparison">The comparison to use when searching</param>
		/// <returns>
		/// True if the <c>enumerable</c> contains the given <c>value</c>.
		/// </returns>
		public static bool Contains<T>(this IEnumerable<T> enumerable, T value, Comparison<T> comparison)
		{
			foreach (T item in enumerable)
			{
				if (comparison(value, item) == 0)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a new IEnumerable with items filtered based on the provided predicate
		/// </summary>
		public static IEnumerable<T> Filter<T>(this IEnumerable source, Predicate<T> accept)
		{
			foreach (T value in source)
			{
				if (accept(value))
				{
					yield return value;
				}
			}
		}

		/// <summary>
		/// Creates a new IEnumerable with items filtered based on the provided predicate
		/// </summary>
		public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Predicate<T> accept)
		{
			foreach (T value in source)
			{
				if (accept(value))
				{
					yield return value;
				}
			}
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the <see cref="Comparer<T>.Default"/>
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable)
		{
			return Sorted(enumerable, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the provided comparison
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable, Comparison<T> comparison)
		{
			List<T> list = new List<T>(enumerable);

			list.Sort(comparison);

			foreach (T item in list)
			{
				yield return item;
			}
		}

		/// <summary>
		/// Creates a new IEnumerable with items sorted based on the provided Comparer.
		/// Elements will not be calculated lazily.
		/// </summary>
		public static IEnumerable<T> Sorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
		{
			List<T> list = new List<T>(enumerable);

			list.Sort(comparer);

			foreach (T item in list)
			{
				yield return item;
			}
		}

		/// <summary>
		/// Creates a new IEnumerable where duplicate items are removed
		/// </summary>
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumerable)
		{
			return DistinctSorted(enumerable, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a new IEnumerable where duplicate items are removed.
		/// Elements are generated on the fly (calculated lazily).
		/// </summary>
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
		{
			IDictionary<T, T> dictionary = new Dictionary<T, T>(comparer);

			foreach (T value in enumerable)
			{
				if (!dictionary.ContainsKey(value))
				{
					dictionary[value] = value;

					yield return value;
				}
			}
		}

		/// <summary>
		/// Creates a new sorted IEnumerable where duplicate items are removed.
		/// Elements will not be calculated lazily.
		/// </summary>
		public static IEnumerable<T> DistinctSorted<T>(this IEnumerable<T> enumerable)
		{
			return DistinctSorted(enumerable, Comparer<T>.Default);
		}

		/// <summary>
		/// Creates a new sorted IEnumerable where duplicate items are removed
		/// </summary>
		public static IEnumerable<T> DistinctSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
		{
			IDictionary<T, T> dictionary = new SortedDictionary<T, T>(comparer);

			foreach (T value in enumerable)
			{
				dictionary[value] = value;
			}

			foreach (T value in dictionary.Values)
			{
				yield return value;
			}
		}

		/// <summary>
		/// Trys to get the last value of an IEnumerable
		/// </summary>
		/// <param name="enumerable">The enumeration to get the last value from</param>
		/// <param name="lastValue">The variable to store the last value</param>
		/// <returns>
		/// True if the last element was returned or False if the enumerable was empty.
		/// </returns>
		public static bool TryGetLast<T>(this IEnumerable<T> enumerable, out T lastValue)
		{
			bool empty = true;

			lastValue = default(T);

			foreach (T value in enumerable)
			{
				lastValue = value;
				empty = false;
			}

			return !empty;
		}

		/// <summary>
		/// Trys to get the first value of an IEnumerable
		/// </summary>
		/// <param name="enumerable">The enumeration to get the last value from</param>
		/// <param name="firstValue">The variable to store the first value</param>
		/// <returns>
		/// True if the first element was returned or False if the enumerable was empty.
		/// </returns>
		public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, out T firstValue)
		{
			foreach (T value in enumerable)
			{
				firstValue = value;

				return true;
			}

			firstValue = default(T);

			return false;
		}

		/// <summary>
		/// Finds an item based on a predicate
		/// </summary>
		/// <typeparam name="T">The element type of the enumerable</typeparam>
		/// <param name="enumerable">The enumerable to search</param>
		/// <param name="match">The predicate that will validate if the element is found</param>
		/// <param name="retval">The variable to store the element if found</param>
		/// <returns>True if the element was found otherwise False</returns>
		public static bool TryFind<T>(this IEnumerable<T> enumerable, Predicate<T> match, out T retval)
		{
			foreach (T item in enumerable)
			{
				if (match(item))
				{
					retval = item;

					return true;
				}
			}

			retval = default(T);

			return false;
		}

		/// <summary>
		/// Converts a indexed range of items from an array to an IEnumerable.
		/// Elements will be calculated lazily.
		/// </summary>
		/// <remarks>
		/// The returned enumerable will by empty or smaller than expected if the range is invalid or overflows.
		/// </remarks>
		/// <param name="array">The array to get elements from</param>
		/// <param name="startOffset">The offset to the first element in the array to return</param>
		/// <param name="count">The number of elements from the array the enumerable should return</param>
		/// <returns>A new enumerable</returns>
		public static IEnumerable<T> Range<T>(T[] array, int startOffset, int count)
		{
			for (int i = startOffset; i < startOffset + count; i++)
			{
				yield return array[i];
			}
		}

		/// <summary>
		/// Returns a new enumerable that will be made up of the elements of another
		/// eumerable within a given range. Elements will be calculated lazily.
		/// </summary>
		/// <remarks>
		/// The returned enumerable will by empty or smaller than expected if the range is invalid or overflows.
		/// </remarks>
		/// <param name="enumerable">The enumerable to get elements from</param>
		/// <param name="startOffset">The offset to the first element in the array to return</param>
		/// <param name="count">The number of elements from the array the enumerable should return</param>
		/// <returns>A new enumerable</returns>
		public static IEnumerable<T> Range<T>(this IEnumerable<T> enumerable, int startOffset, int count)
		{
			IEnumerator<T> enumerator;

			enumerator = enumerable.GetEnumerator();

			using (enumerator)
			{
				for (int i = 0; i < startOffset; i++)
				{
					if (!enumerator.MoveNext())
					{
						yield break;
					}
				}

				while (enumerator.MoveNext())
				{
					yield return enumerator.Current;
				}
			}
		}

		/// <summary>
		/// Converts the enumerable to a dictionary using the enumerable values
		/// as the values in the dictionary.
		/// </summary>
		/// <typeparam name="K">The key type</typeparam>
		/// <typeparam name="V">The value type</typeparam>
		/// <param name="enumerable">The enumeration of values</param>
		/// <param name="valueToKey">A converter that converts values to keys</param>
		/// <returns>A new dictionary</returns>
		public static Dictionary<K, V> ToDictionaryWithValues<K, V>(this IEnumerable<V> enumerable, Converter<V, K> valueToKey)
		{
			var retval = new Dictionary<K, V>();

			foreach (V value in enumerable)
			{
				retval[valueToKey(value)] = value;
			}

			return retval;
		}

        /// <summary>
		/// Converts the enumerable to a dictionary using the enumerable values
		/// as the values in the dictionary.
		/// </summary>
		/// <typeparam name="K">The key type</typeparam>
		/// <typeparam name="V">The value type</typeparam>
		/// <typeparam name="D">The type of dictionary to create</typeparam>
		/// <param name="enumerable">The enumeration of values</param>
		/// <param name="valueToKey">A converter that converts values to keys</param>
		/// <returns>A new dictionary</returns>
		public static D ToDictionaryWithValues<K, V, D>(this IEnumerable<V> enumerable, Converter<V, K> valueToKey)
			where D : IDictionary<K, V>, new()
		{
        	var retval = new D();
            
			foreach (V value in enumerable)
			{
				retval[valueToKey(value)] = value;
			}

			return retval;
		}

		/// <summary>
		/// Converts the enumerable to a dictionary using the enumerable values
		/// as the values in the dictionary.
		/// </summary>
		/// <typeparam name="K">The key type</typeparam>
		/// <typeparam name="V">The value type</typeparam>
		/// <param name="enumerable">The enumeration of values</param>
		/// <param name="keyToValue">A converter that converts keys to values</param>
		/// <returns>A new dictionary</returns>
		public static Dictionary<K, V> ToDictionaryWithKeys<K, V>(this IEnumerable<K> enumerable, Converter<K, V> keyToValue)
		{
			var retval = new Dictionary<K, V>();

			foreach (K key in enumerable)
			{
				retval[key] = keyToValue(key);
			}

			return retval;
		}


		/// <summary>
		/// Converts the enumerable to a dictionary using the enumerable values
		/// as the values in the dictionary.
		/// </summary>
		/// <typeparam name="K">The key type</typeparam>
		/// <typeparam name="V">The value type</typeparam>
		/// <typeparam name="D">The type of dictionary to create</typeparam>
		/// <param name="enumerable">The enumeration of values</param>
		/// <param name="keyToValue">A converter that converts keys to values</param>
		/// <returns>A new dictionary</returns>
		public static D ToDictionaryWithKeys<K, V, D>(this IEnumerable<K> enumerable, Converter<K, V> keyToValue)
			where D : IDictionary<K, V>, new()
		{
			var retval = new D();

			foreach (K key in enumerable)
			{
				retval[key] = keyToValue(key);
			}

			return retval;
		}
	}
}