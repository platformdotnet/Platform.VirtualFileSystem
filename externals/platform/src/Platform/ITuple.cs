using System;
using System.Collections.Generic;

namespace Platform
{
	/// <summary>
	/// An interface for Tuples data types.
	/// </summary>
	public interface ITuple
	{
		/// <summary>
		/// Gets the number of elements held by the tuple.
		/// </summary>
		int Size
		{
			get;
		}

		/// <summary>
		/// Gets the tuple element at the given index.
		/// </summary>
		/// <typeparam name="T">The tuple type</typeparam>
		/// <param name="index">The index of the elment to get (0 based)</param>
		/// <returns>The element</returns>
		T GetAt<T>(int index);
	}
}
