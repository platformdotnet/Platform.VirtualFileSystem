using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Provides useful <see cref="INamed"/> related utility methods.
	/// </summary>
	public static class NamedUtils
	{
		/// <summary>
		/// Gets a predicate that validates whether an object has a certain name.
		/// </summary>
		/// <typeparam name="T">The object type (must implement <see cref="INamed"/>)</typeparam>
		/// <param name="name">The name to validate against the object</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> IsName<T>(string name)
				where T : INamed
		{
			return IsName<T>(name, StringComparison.CurrentCulture);
		}

		/// <summary>
		/// Gets a predicate that validates whether an object has a certain name.
		/// </summary>
		/// <typeparam name="T">The object type (must implement <see cref="INamed"/>)</typeparam>
		/// <param name="name">The name to validate against the object</param>
		/// <param name="comparisonType">The type of string comparison to use</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> IsName<T>(string name, StringComparison comparisonType)
			where T : INamed
		{
			return value => value.Name.Equals(name, comparisonType);
		}
	}
}
