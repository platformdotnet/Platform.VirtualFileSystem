using System;
using System.Collections.Generic;

namespace Platform
{
	/// <summary>
	/// Provides extension methods for array classes
	/// </summary>
	public static class ArrayUtils
	{
		/// <summary>
		/// Creates a new array that is made up of the elements of the given array
		/// after they have been converted by the given converter.
		/// </summary>
		/// <typeparam name="T">The element type of the original array</typeparam>
		/// <typeparam name="U">The element type of the new array</typeparam>
		/// <param name="self">The array to convert</param>
		/// <param name="convert">The converter that will convert the elements of
		/// original array into the element of the new array</param>
		/// <returns>
		/// A new array
		/// </returns>
		public static U[] NewArray<T, U>(this T[] self, Converter<T, U> convert)
		{
			var retval = new U[self.Length];

			for (int i = 0; i < self.Length; i++)
			{
				retval[i] = convert(self[i]);
			}

			return retval;
		}

		/// <summary>
		/// Checks to see if the elements of array1 is equal to the elements of array2
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="array1">The first array to compare</param>
		/// <param name="array2">The second array to compare</param>
		/// <returns>
		/// True if all the elements if <c>array1</c> equal the corresponding
		/// elements of <c>array2</c>.
		/// False if arrays are of inequal length or any of the elements don't match
		/// their corresponding elements.
		/// </returns>
		public static bool ElementsAreEqual<T>(this T[] array1, T[] array2)
		{
			if (array1.Length != array2.Length)
			{
				return false;
			}

			for (var i = 0; i < array1.Length; i++)
			{
				if (!Object.Equals(array1[i], array2[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Returns the index of the first element that contains any of the elements
		/// provided in <c>values</c>
		/// </summary>
		/// <typeparam name="T">The type of array to work with</typeparam>
		/// <param name="array">The array to search</param>
		/// <param name="values">An array of values</param>
		/// <returns>The index of the element if found or -1</returns>
		public static int IndexOfAny<T>(this T[] array, params T[] values)
		{
			for (var i = 0; i < array.Length; i++)
			{
				var x = Array.IndexOf<T>(values, array[i]);

				if (x >= 0)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the first element that is accepted by the given
		/// predicate
		/// </summary>
		/// <typeparam name="T">The type of array to work with</typeparam>
		/// <param name="array">The array to search</param>
		/// <param name="accept">The predicate that will validate if the right character is found</param>
		/// <returns>The index of the element if found or -1</returns>
		public static int IndexOfAny<T>(this T[] array, Predicate<T> accept)
		{
			for (var i = 0; i < array.Length; i++)
			{
				if (accept(array[i]))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Creates a new array that is the combination of the current array and the given
		/// enumeration of elements
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="array1">The array whose elements will make up the start of the new array</param>
		/// <param name="enumerable">The enumerable whose elements will make up the second half of the new array</param>
		/// <returns>A new array</returns>
		public static T[] Combine<T>(this T[] array1, IEnumerable<T> enumerable)
		{
			var list = new List<T>(Math.Max(16, array1.Length * 2));

			list.AddRange(array1);

			foreach (T value in enumerable)
			{
				list.Add(value);
			}

			return list.ToArray();
		}

		/// <summary>
		/// Creates a new array that is the combination of the current array and the given
		/// array or elements
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="array1">The array whose elements will make up the start of the new array</param>
		/// <param name="array2">The array whose elements will make up the second half of the new array</param>
		/// <returns>A new array</returns>
		public static T[] Combine<T>(this T[] array1, params T[] array2)
		{
			var retval = new T[array1.Length + array2.Length];

			Array.Copy(array1, retval, array1.Length);
			Array.Copy(array2, 0, retval, array1.Length, array2.Length);

			return retval;
		}

		/// <summary>
		/// Creates a new array that is made up of the current array with a new element prepended.
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="array1">The array to create the new array from</param>
		/// <param name="obj1">The last element to be prepended onto the new array</param>
		/// <returns>A new array</returns>
		public static T[] Prepend<T>(this T[] array1, T obj1)
		{
			T[] retval;

			retval = new T[array1.Length + 1];

			retval[0] = obj1;
			Array.Copy(array1, 0, retval, 1, array1.Length);

			return retval;
		}

		/// <summary>
		/// Creates a new array that is made up of the current array with a new element appeneded.
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="array1">The array to create the new array from</param>
		/// <param name="obj1">The last element to be appeneded onto the new array</param>
		/// <returns>A new array</returns>
		public static T[] Append<T>(this T[] array1, T obj1)
		{
			var retval = new T[array1.Length + 1];

			Array.Copy(array1, 0, retval, 0, array1.Length);
			retval[array1.Length] = obj1;

			return retval;
		}
        
		/// <summary>
		/// Creates a new array from elements of another array
		/// </summary>
		/// <typeparam name="T">The type of array to create</typeparam>
		/// <param name="array">The array whose elements will make up the new array</param>
		/// <param name="offset">
		/// The offset to the element in <c>array</c> that will make up the first
		/// element of the new array
		/// </param>
		/// <param name="length">The length of the new array</param>
		/// <returns>A new array</returns>
		public static T[] NewArray<T>(this T[] array, int offset, int length)
		{
			T[] retval = new T[length];

			Array.Copy(array, 0, retval, 0, length);

			return retval;
		}

		/// <summary>
		/// Creates a new array from a generic <c>ICollection</c>
		/// </summary>
		/// <typeparam name="T">The type of array to create</typeparam>
		/// <param name="collection">The collection to create the array from</param>
		/// <returns>A new array</returns>
		public static T[] NewArray<T>(this ICollection<T> collection)
		{
			return NewArray<T, T>(collection, Converters<T, T>.NoConvert);
		}

		/// <summary>
		/// Creates a new array from a generic <c>ICollection</c>; optionally converting
		/// the collection's elements.
		/// </summary>
		/// <typeparam name="T">The type of array to create</typeparam>
		/// <param name="collection">The collection to create the array from</param>
		/// <param name="convert">
		/// The converter used to convert elements from the collection type to the array type
		/// </param>
		/// <returns>A new array</returns>
		public static D[] NewArray<T, D>(ICollection<T> collection, Converter<T, D> convert)
		{
			var i = 0;

			var retval = new D[collection.Count];

			foreach (T value in collection)
			{
				retval[i++] = convert(value);
			}

			return retval;
		}

		/// <summary>
		/// Creates a new array from a generic <c>IEnumerable</c>
		/// </summary>
		/// <typeparam name="T">The type of array to create</typeparam>
		/// <param name="enumerable">The enumerable to create the array from</param>
		/// <returns></returns>
		public static T[] NewArray<T>(IEnumerable<T> enumerable)
		{
			var list = new List<T>();

			foreach (T value in enumerable)
			{
				list.Add(value);
			}

			return list.ToArray();
		}

		/// <summary>
		/// Creates a new array from an <c>IEnumerable</c>
		/// </summary>
		/// <typeparam name="T">The type of array to create</typeparam>
		/// <param name="enumerable">The enumerable to create the array from</param>
		/// <returns></returns>
		public static T[] NewArray<T>(System.Collections.IEnumerable enumerable)
		{
			var list = new List<T>();

			foreach (T value in enumerable)
			{
				list.Add(value);
			}

			return list.ToArray();
		}
	}
}
