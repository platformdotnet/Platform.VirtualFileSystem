using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Collections
{
	/// <summary>
	/// An interface for objects that can supply enumerators that enumerate in reverse.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IReverseEnumerable<T>
	{
		/// <summary>
		/// Gets an enumerator that enumerates in reverse.
		/// </summary>
		/// <returns>An <see cref="IEnumerator{T}"/></returns>
		IEnumerator<T> GetReverseEnumerator();
	}
}
