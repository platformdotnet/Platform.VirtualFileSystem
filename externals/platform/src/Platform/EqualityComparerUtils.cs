using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Provides utility methods for the <see cref="IEqualityComparer{T}"/> interface.
	/// </summary>
	public static class EqualityComparerUtils
	{
		private class ComparisonBasedEqualityComparer<T>
			: IEqualityComparer<T>
		{
			private readonly EqualityComparison<T> comparison;

			public ComparisonBasedEqualityComparer(EqualityComparison<T> comparison)
			{
				this.comparison = comparison;
			}

			public bool Equals(T x, T y)
			{
				return comparison(x, y);
			}

			public int GetHashCode(T obj)
			{
				return obj.GetHashCode();
			}
		}

		/// <summary>
		/// Converts an <see cref="EqualityComparison{T}"/> into an <see cref="IEqualityComparer{T}"/>.
		/// The <see cref="IEqualityComparer{T}.GetHashCode(T)"/> implementation will use the
		/// <see cref="object.GetHashCode"/> implementation of the current object being compared.
		/// </summary>
		/// <typeparam name="T">The type iof object to compare</typeparam>
		/// <param name="comparison">The comparison to convert</param>
		/// <returns>A new <see cref="IEqualityComparer{T}"/></returns>
		public static IEqualityComparer<T> ToEqualityComparer<T>(this EqualityComparison<T> comparison)
		{
			return new ComparisonBasedEqualityComparer<T>(comparison);
		}
	}
}
