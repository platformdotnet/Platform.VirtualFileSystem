using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// A <see cref="Comparison{T}"/> like class that only checks for equality
	/// rather than size.
	/// </summary>
	/// <typeparam name="T">The type of object to check</typeparam>
	/// <param name="a">The first item to check</param>
	/// <param name="b">The second item to check</param>
	/// <returns>True if <c>a</c> and <c>b</c> are equal</returns>
	public delegate bool EqualityComparison<T>(T a, T b);
}
