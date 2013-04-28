using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Implements the <see cref="IEqualityComparer{T}"/> interface using a function
	/// delegate.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class FunctionBasedEqualityComparer<T>
			: IEqualityComparer<T>
	{
		private readonly Func<T, int> hasher;
		private readonly Func<T, T, bool> comparerFunction;

		/// <summary>
		/// Returns the hashcode of an object using the object's <see cref="Object.GetHashCode()"/>
		/// implementation.
		/// </summary>
		/// <param name="obj">The object to generate the hash from</param>
		/// <returns>
		/// The hashcode of the object has returned by <see cref="Object.GetHashCode()"/>
		/// </returns>
		public static int DefaultHasher(T obj)
		{
			return obj.GetHashCode();
		}

		/// <summary>
		/// Create a new FunctionBasedEqualityComparer based on a <see cref="Comparison{T}"/>.
		/// </summary>
		/// <param name="comparisonFunction">The <see cref="Comparison{BT}"/> to use</param>
		public FunctionBasedEqualityComparer(Comparison<T> comparisonFunction)
			: this(delegate(T x, T y) { return comparisonFunction(x, y) == 0; }, DefaultHasher)
		{
		}

		/// <summary>
		/// Create a new FunctionBasedEqualityComparer.
		/// </summary>
		/// <param name="comparerFunction">
		/// The function used for performing comparisons.  The function takes two
		/// objects and returns true or false based on the object's equality.
		/// </param>
		/// <remarks>
		/// The <see cref="DefaultHasher"/> is used for implementing 
		/// <see cref="IEqualityComparer{T}.GetHashCode(object)"/>.
		/// </remarks>
		public FunctionBasedEqualityComparer(Func<T, T, bool> comparerFunction)
			: this(comparerFunction, DefaultHasher)
		{
		}

		/// <summary>
		/// Create a new FunctionBasedEqualityComparer.
		/// </summary>
		/// <param name="comparerFunction">
		/// The function used for performing comparisons.  The function takes two
		/// objects and returns true or false based on the object's equality.
		/// </param>
		/// <remarks>
		/// The <see cref="DefaultHasher"/> is used for implementing 
		/// <see cref="IEqualityComparer{T}.GetHashCode(object)"/>.
		/// </remarks>
		public FunctionBasedEqualityComparer(Func<T, T, bool> comparerFunction, Func<T, int> hasher)
		{
			this.hasher = hasher;
			this.comparerFunction = comparerFunction;
		}

		public virtual bool Equals(T x, T y)
		{
			return comparerFunction(x, y);
		}

		public virtual int GetHashCode(T obj)
		{
			return hasher(obj);
		}
	}
}
