using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform
{
	/// <summary>
	/// A helper class for creating predicate based on a regular expression.
	/// Use the <see cref="PredicateUtils"/> to create actual regular expression predicate. 
	/// </summary>
	public class RegexBasedPredicateHelper
	{
		/// <summary>
		/// The regular expression
		/// </summary>
		public virtual Regex Regex
		{
			get;
			private set;
		}

		/// <summary>
		/// Creates a new <see cref="RegexBasedPredicateHelper"/> from the given <see cref="regex"/>.
		/// </summary>
		/// <param name="regex">The <see cref="regex"/></param>
		internal protected RegexBasedPredicateHelper(string regex)
			: this(new Regex(regex))
		{
		}

		/// <summary>
		/// Creates a new <see cref="RegexBasedPredicateHelper"/> from the given <see cref="regex"/>.
		/// </summary>
		/// <param name="regex">The <see cref="regex"/></param>
		internal protected RegexBasedPredicateHelper(Regex regex)
		{
			this.Regex = regex;
		}

		public static bool IsRegexBasedPredicate(Predicate<string> regexBasedPredicate)
		{
			return regexBasedPredicate.Target as RegexBasedPredicateHelper != null;
		}

		/// <summary>
		/// Gets the regular expression from the given predicate.
		/// </summary>
		/// <param name="regexBasedPredicate"></param>
		/// <exception cref="ArgumentException">The given predicate was not created by<see cref="RegexBasedPredicateHelper"/></exception>
		/// <returns>The <see cref="Regex"/> associated with the <see cref="Predicate{T}"/></returns>
		public static Regex GetRegexFromPredicate(Predicate<string> regexBasedPredicate)
		{
			RegexBasedPredicateHelper predicateHelper = regexBasedPredicate.Target as RegexBasedPredicateHelper;

			if (predicateHelper == null)
			{
				throw new ArgumentException(String.Format("Must be a predicateHelper created from {0}", typeof(RegexBasedPredicateHelper).Name));
			}

			return predicateHelper.Regex;
		}

		public virtual bool Accept(string value)
		{
			return this.Regex.IsMatch(value);
		}

		public virtual Predicate<string> ToPredicate()
		{
			return (Predicate<string>)this;
		}

		public static implicit operator Predicate<string>(RegexBasedPredicateHelper predicateHelper)
		{
			return predicateHelper.Accept;
		}
	}
}
