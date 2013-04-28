using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Platform
{
	/// <summary>
	/// Provides access to common constant predicates.
	/// </summary>
	public static class PredicateUtils<T>
	{
		/// <summary>
		/// Returns a predicate that always returns true
		/// </summary>
		/// <typeparam name="T">The type the predicate works with</typeparam>
		/// <returns>A predicate that always returns true</returns>
		public static readonly Predicate<T> AlwaysTrue = value => true;
		
		/// <summary>
		/// Returns a predicate that always returns false
		/// </summary>
		/// <typeparam name="T">The type the predicate works with</typeparam>
		/// <returns>A predicate that always returns false</returns>
		public static readonly Predicate<T> AlwaysFalse = value => false;
	}

	/// <summary>
	/// Provides extension methods and static utility methods for enumerable types.
	/// </summary>
	public static class PredicateUtils
	{
		/// <summary>
		/// Creates a new <see cref="Regex"/> based predicate.
		/// </summary>
		/// <param name="regex">A regex string</param>
		/// <returns>The new predicate</returns>
		public static Predicate<string> NewRegex(string regex)
		{
			return new RegexBasedPredicateHelper(regex);
		}

		/// <summary>
		/// Creates a new <see cref="Regex"/> based predicate.
		/// </summary>
		/// <param name="regex">A regex</param>
		/// <returns>The new predicate</returns>
		public static Predicate<string> NewRegex(Regex regex)
		{
			return new RegexBasedPredicateHelper(regex);
		}

		/// <summary>
		/// Gets a <see cref="Regex"/> form a predicate.  The predicate must have
		/// been created with the <see cref="NewRegex(string)"/> or <see cref="NewRegex(Regex)"/> methods.
		/// </summary>
		/// <param name="regexBasedPredicate">The regular expression based predicate</param>
		/// <returns>The regular expression associated with the predicate</returns>
		public static Regex GetRegexFromPredicate(Predicate<string> regexBasedPredicate)
		{
			return RegexBasedPredicateHelper.GetRegexFromPredicate(regexBasedPredicate);
		}

		/// <summary>
		/// Returns true if the given predicate was created with the 
		/// <see cref="NewRegex(string)"/> or <see cref="NewRegex(Regex)"/> methods.
		/// </summary>
		/// <param name="regexBasedPredicate">The regular expression based predicate</param>
		/// <returns>True if the given predicate is a regex based predicate</returns>
		public static bool IsRegexBasedPredicate(Predicate<string> regexBasedPredicate)
		{
			return RegexBasedPredicateHelper.IsRegexBasedPredicate(regexBasedPredicate);
		}

		/// <summary>
		/// Returns a pedicate that returns the logical not of a given predicate
		/// </summary>
		/// <typeparam name="T">The predicate type</typeparam>
		/// <param name="predicate">The predicate</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> Not<T>(this Predicate<T> predicate)
		{
			return value => !predicate(value);
		}

		/// <summary>
		/// Returns a predicate that is the logical and of two given predicates
		/// </summary>
		/// <typeparam name="T">The predicate type</typeparam>
		/// <param name="predicate1">The first predicate</param>
		/// <param name="predicate2">The second predicate</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> And<T>(this Predicate<T> predicate1, Predicate<T> predicate2)
		{
			return value => predicate1(value) && predicate2(value);
		}

		/// <summary>
		/// Returns a predicate that is the logical and of one or more predicates
		/// </summary>
		/// <typeparam name="T">The predicate type</typeparam>
		/// <param name="predicate1">The first predicate</param>
		/// <param name="predicates">An array of predicates to form part of the new predicate</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> And<T>(this Predicate<T> predicate1, params Predicate<T>[] predicates)
		{
			return delegate(T value)
			{
				if (!predicate1(value))
				{
					return false;
				}

				foreach (Predicate<T> predicate in predicates)
				{
					if (!predicate(value))
					{
						return false;
					}
				}

				return true;
			};
		}

		/// <summary>
		/// Returns a predicate that is the logical or of two given predicates
		/// </summary>
		/// <typeparam name="T">The predicate type</typeparam>
		/// <param name="predicate1">The first predicate</param>
		/// <param name="predicate2">The second predicate</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> Or<T>(this Predicate<T> predicate1, Predicate<T> predicate2)
		{
			return value => predicate1(value) || predicate2(value);
		}

		/// <summary>
		/// Returns a predicate that is the logical and of one or more predicates
		/// </summary>
		/// <typeparam name="T">The predicate type</typeparam>
		/// <param name="predicate1">The first predicate</param>
		/// <param name="predicates">An array of predicates to form part of the new predicate</param>
		/// <returns>A new predicate</returns>
		public static Predicate<T> Or<T>(this Predicate<T> predicate1, params Predicate<T>[] predicates)
		{
			return delegate(T value)
			{
				if (predicate1(value))
				{
					return true;
				}

				foreach (Predicate<T> predicate in predicates)
				{
					if (predicate(value))
					{
						return true;
					}
				}

				return false;
			};
		}

		/// <summary>
		/// Returns a <c>Predicate</c> that checks if the predicate argument is not equal to the 
		/// provided <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to compare</param>
		/// <returns>
		/// The new <c>Predicate</c>.  The predicate returns true of <paramref name="value"/>
		/// is not equal to the predicate argument.
		/// </returns>
		public static Predicate<T> ObjectNotEquals<T>(T value)
		{
			return value2 => !value.Equals(value2);
		}

		/// <summary>
		/// Returns a <c>Predicate</c> that checks if the predicate argument is equal to the 
		/// provided <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value to compare</param>
		/// <returns>
		/// The new <c>Predicate</c>.  The predicate returns true of <paramref name="value"/>
		/// is equal to the predicate argument.
		/// </returns>
		public static Predicate<T> ObjectEquals<T>(T value)
		{
			return value2 => value.Equals(value2);
		}

		/// <summary>
		/// Returns a <c>Predicate</c> that checks if the predicate argument is equal to any
		/// of the <paramref name="values"/>.
		/// </summary>
		/// <param name="values">The values to compare</param>
		/// <returns>
		/// The new <c>Predicate</c>.
		/// </returns>
		public static Predicate<T> ObjectEqualsAny<T>(IEnumerable<T> values)
		{
			return value => values.Contains(value);
		}

		/// <summary>
		/// Returns a <c>Predicate</c> that checks if the predicate argument is equal to any
		/// of the <paramref name="values"/>.
		/// </summary>
		/// <param name="values">The values to compare</param>
		/// <returns>
		/// The new <c>Predicate</c>.
		/// </returns>
		public static Predicate<T> ObjectEqualsAny<T>(params T[] values)
		{
			return value2 => Array.IndexOf(values, value2) >= 0;
		}

		/// <summary>
		/// Returns a <c>Predicate</c> that progressively compares the next object on the given array from the start.
		/// </summary>
		/// <remarks>
		/// The delegate maintains state and each time the delegate is called, the delegate returns true
		/// or false if the next object in the array (from the start/left) is equal to the predicate
		/// argument.
		/// </remarks>
		/// <param name="objects">
		/// The array of objects to compare.
		/// </param>
		/// <returns>
		/// A new predicate.
		/// </returns>
		public static Predicate<T> ObjectByObjectFromLeft<T>(params T[] objects)
		{
			int x = 0;

			return value => x >= objects.Length ? false : value.Equals(objects[x++]);
		}

		/// <summary>
		/// Returns a delegate that progressively compares the next object on the given array from the end.
		/// </summary>
		/// <remarks>
		/// The delegate maintains state and each time the delegate is called, the delegate returns true
		/// or false if the next object in the array (from the end/right) is equal to the predicate
		/// argument.
		/// </remarks>
		/// <param name="objects">
		/// The array of objects to compare.
		/// </param>
		/// <returns>
		/// A new predicate.
		/// </returns>
		public static Predicate<T> ObjectByObjectFromRight<T>(params T[] objects)
		{
			int x = objects.Length - 1;

			return value => x < 0 ? false : value.Equals(objects[x--]);
		}
	}
}
