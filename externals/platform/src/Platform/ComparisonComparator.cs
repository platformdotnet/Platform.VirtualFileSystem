using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Wraps a <see cref="Comparison{T}"/> and converts it into an <see cref="IComparer{T}"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class ComparisonComparer<T>
		: IComparer<T>
	{
		private readonly Comparison<T> comparison;

		/// <summary>
		/// Constructs a new <see cref="ComparisonComparer{T}"/>.
		/// </summary>
		/// <param name="comparison">The <see cref="Comparison{T}"/> the current object will wrap.</param>
		public ComparisonComparer(Comparison<T> comparison)
		{
			this.comparison = comparison;
		}

		/// <summary>
		/// Compares <see cref="x"/> and <see cref="y"/> using the inner <see cref="Comparison{T}"/>.
		/// </summary>
		public virtual int Compare(T x, T y)
		{
			return comparison(x, y);
		}
	}
}
