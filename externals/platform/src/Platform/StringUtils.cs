using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and static utility methods for strings.
	/// </summary>
	public static class StringUtils
	{
		public static Func<string, Func<string>> NewAdder(char appendChar)
		{
			return NewAdder(appendChar, true);
		}

		public static Func<string, Func<string>> NewAdder(char appendChar, bool appendAtEnd)
		{
			Func<string> retval;
			var builder = new StringBuilder();

			if (appendAtEnd)
			{
				retval = builder.ToString;
			}
			else
			{
				retval = delegate
				{
					if (builder.Length > 0)
					{
					   builder.Length--;
					}

					return builder.ToString();
				};
			}

			return delegate(string value)
			{
				builder.Append(value).Append(appendChar);

				return retval;
			};
		}

		/// <summary>
		/// Returns a new string that is made up of part of the current string.
		/// This method will return an empty or smaller than expected string if 
		/// the index or lengths overflow.
		/// </summary>
		/// <param name="s">The current string</param>
		/// <param name="index">The index of the first character to return</param>
		/// <param name="length">The length of the string to return</param>
		/// <returns>
		/// A new string
		/// </returns>
		public static string MidString(this string s, int index, int length)
		{
			if (length < 0)
			{
				length = 0;
			}

			if (index < 0)
			{
				index = 0;
			}
			else if (index >= s.Length)
			{
				return string.Empty;
			}

			int y;

			y = index + length;

			if (y >= s.Length)
			{
				length = Math.Max(length - (y - s.Length), 0);
			}

			return s.Substring(index, length);
		}

		public static Pair<string, string> SplitAroundFirstStringFromLeft(this string s, string searchString)
		{
			int x;

			x = s.IndexOf(searchString);

			if (x < 0)
			{
				return new Pair<string, string>(s, String.Empty);
			}

			return new Pair<string, string>(s.Substring(0, x), s.Substring(x + searchString.Length));
		}

		public static Pair<string, string> SplitAroundFirstStringFromRight(this string s, string searchString)
		{
			var x = s.LastIndexOf(searchString);

			if (x < 0)
			{
				return new Pair<string, string>(String.Empty, s);
			}

			return new Pair<string, string>(s.Substring(0, x), s.Substring(x + searchString.Length));
		}

		public static Pair<string, string> SplitAroundFirstCharFromLeft(this string s, char c)
		{
			return s.SplitAroundCharFromLeft(PredicateUtils.ObjectEquals(c));
		}

		public static Pair<string, string> SplitAroundFirstCharFromRight(this string s, char c)
		{
			return s.SplitAroundCharFromRight(PredicateUtils.ObjectEquals(c));
		}

		public static Pair<string, string> SplitAroundCharFromLeft(this string s, char c)
		{
			return s.SplitAroundCharFromLeft(PredicateUtils.ObjectEquals(c));
		}

		public static Pair<string, string> SplitAroundCharFromLeft(this string s, Predicate<char> isSplitChar)
		{
			return s.SplitAroundCharFromLeft(0, isSplitChar);
		}

		public static Pair<string, string> SplitAroundCharFromLeft(this string s, int startIndex, Predicate<char> isSplitChar)
		{
			for (var i = startIndex; i < s.Length; i++)
			{
				if (isSplitChar(s[i]))
				{
					return new Pair<string, string>(s.Left(i), Right(s, s.Length - i - 1));
				}
			}

			return new Pair<string, string>(s, String.Empty);
		}

		public static Pair<string, string> SplitAroundCharFromRight(this string s, char c)
		{
			return s.SplitAroundCharFromRight(PredicateUtils.ObjectEquals(c));
		}

		public static Pair<string, string> SplitAroundCharFromRight(this string s, Predicate<char> isSplitChar)
		{
			for (var i = s.Length - 1; i >= 0; i--)
			{
				if (isSplitChar(s[i]))
				{
					return new Pair<string, string>(s.Left(i), Right(s, s.Length - i - 1));
				}
			}

			return new Pair<string, string>(String.Empty, s);
		}

		public static bool IsNullOrEmpty(this string s)
		{
			return string.IsNullOrEmpty(s);
		}

		public static string[] Split(this string s, string regex, int count, int startIndex)
		{
			var re = new Regex(regex);

			return re.Split(s, count, startIndex);
		}

		public static string[] Split(this string s, string regex, int count)
		{
			var re = new Regex(regex);

			return re.Split(s, count);
		}

		public static string[] Split(this string s, string regex)
		{
			return Regex.Split(s, regex);
		}

		public static string Replace(this string s, string regex, string replacement, int count, int startIndex)
		{
			var re = new Regex(regex);
			
			return re.Replace(s, replacement, count, startIndex);
		}

		public static string Replace(this string s, string regex, string replacement, int count)
		{
			var re = new Regex(regex);
			
			return re.Replace(s, replacement, count);
		}

		public static string Replace(this string s, string regex, string replacement)
		{
			return Regex.Replace(s, regex, replacement);
		}

		public static string ReplaceFirst(this string s, string regex, string replacement)
		{
			var re = new Regex(regex);
			
			return re.Replace(s, replacement, 1);
		}

		public static string ReplaceLast(this string s, string regex, string replacement)
		{
			Regex re = new Regex(regex, RegexOptions.RightToLeft);
			
			return re.Replace(s, replacement, 1);
		}

		public static string Capitalize(this string s)
		{
			if (s.Length == 0)
			{
				return s;
			}

			if (Char.IsUpper(s[0]))
			{
				return s;
			}
			else
			{
				var builder = new StringBuilder(s.Length);

				builder.Append(Char.ToUpper(s[0]));
				builder.Append(s, 1, s.Length - 1);

				return builder.ToString();
			}
		}

		public static string Uncapitalize(this string s)
		{
			if (s.Length == 0)
			{
				return s;
			}

			if (Char.IsLower(s[0]))
			{
				return s;
			}
			else
			{
				var builder = new StringBuilder(s.Length);

				builder.Append(Char.ToLower(s[0]));
				builder.Append(s, 1, s.Length - 1);

				return builder.ToString();
			}
		}

		public static bool EqualsIgnoreCase(this string s1, string s2)
		{
			return s1.Equals(s2, StringComparison.CurrentCultureIgnoreCase);
		}

		public static bool EqualsIgnoreCaseInvariant(this string s1, string s2)
		{
			return s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase);
		}

		public static string Convert(this string s, Converter<char, char> converter)
		{
			return ConvertAndFilter(s, converter, null);
		}

		public static string Filter(this string s, Predicate<char> shouldFilter)
		{
			return s.FilterAndConvert(shouldFilter, null);
		}

		public static string ConvertAndFilter(this string s, Converter<char, char> convert, Predicate<char> filter)
		{
			StringBuilder builder;
			
			if (convert != null)
			{
				builder = new StringBuilder(s.Length * 2);
			}
			else
			{
				builder = new StringBuilder(s.Length);
			}

			var filtered = false;
						
			for (int i = 0; i < s.Length; i++)
			{				
				char value;
				
				if (convert != null)
				{
					value = convert(s[i]);
				}
				else
				{
					value = s[i];
				}

				if (filter == null)
				{
					builder.Append(value);
				}
				else if (filter(value))
				{
					filtered = true;

					builder.Append(value);
				}				
			}

			if (!filtered && convert == null)
			{
				return s;
			}
			else
			{
				return builder.ToString();				
			}
		}

		public static string FilterAndConvert(this string s, Predicate<char> acceptChar, Converter<char, char> convert)
		{
			StringBuilder builder;
			
			if (convert != null)
			{
				builder = new StringBuilder(s.Length * 2);
			}
			else
			{
				builder = new StringBuilder(s.Length);
			}

			var filtered = false;
						
			for (var i = 0; i < s.Length; i++)
			{				
				char c;

				c = s[i];

				if (acceptChar != null && acceptChar(c))
				{
					if (convert != null)
					{
						builder.Append(convert(c));
					}
					else
					{
						builder.Append(c);
					}

					continue;
				}
				
				filtered = true;
			}

			if (!filtered && convert == null)
			{
				return s;
			}
			else
			{
				return builder.ToString();				
			}
		}
		
		/// <summary>
		/// Gets the string made of all the characters on the left of the string while the
		/// predicate is satisfied.
		/// </summary>
		/// <param name="s">The string to operate on</param>
		/// <param name="acceptChar">A predicate that takes a char and returns false when left should return</param>
		/// <returns>A new string</returns>
		public static string Left(this string s, Predicate<char> acceptChar)
		{
			int i;

			for (i = 0; i < s.Length; i++)
			{				
				if (acceptChar(s[i]))
				{
					continue;
				}
				else
				{
					break;
				}
			}

			if (i >= s.Length)
			{
				return s;
			}

			return s.Substring(0, i);
		}

		/// <summary>
		/// Gets the string made up of all the characters on the right of all the characters
		/// on the left that match the predicate!
		/// </summary>
		/// <param name="s"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static string RightFromLeft(this string s, Predicate<char> predicate)
		{
			int i;

			for (i = 0; i < s.Length; i++)
			{				
				if (predicate(s[i]))
				{
					continue;
				}
				else
				{
					break;
				}
			}

			if (i >= s.Length)
			{
				return String.Empty;
			}

			return s.Substring(i + 1);
		}

		/// <summary>
		/// Gets the string that is made up of the right most characters of <c>s</c>
		/// that satisfy <c>predicate</c>.
		/// </summary>
		/// <remarks>
		/// Gets the string that is made up of the right most characters of <c>s</c>
		/// that satisfy <c>predicate</c>.  The method terminates and returns as soon
		/// as a character that doesn't satisfy <c>predicate</c> is found.
		/// <p>
		/// If every character satisfies the predicate then <c>s</c> is returned.
		/// </p>
		/// </remarks>
		/// <param name="s"></param>
		/// <param name="acceptChar"></param>
		/// <returns></returns>
		public static string Right(this string s, Predicate<char> acceptChar)
		{
			int i;

			for (i = s.Length - 1; i >= 0; i--)
			{				
				if (acceptChar(s[i]))
				{
					continue;
				}
				else
				{
					break;
				}
			}

			if (i  < 0)
			{
				return s;
			}

			return s.Substring(i + 1);
		}

		public static string LeftFromRight(this string s, Predicate<char> accept)
		{
			int i;

			for (i = s.Length - 1; i >= 0; i--)
			{
				if (accept(s[i]))
				{
					continue;
				}
				else
				{
					break;
				}
			}

			if (i  < 0)
			{
				return String.Empty;
			}

			return s.Substring(0, i + 1);
		}

		public static string Left(this string s, int count)
		{
			if (count >= s.Length)
			{
				return s;
			}

			return s.Substring(0, count);
		}

		public static string Right(this string s, int count)
		{
			if (count >= s.Length)
			{
				return s;
			}

			if (count < 0)
			{
				return String.Empty;
			}

			if (s.Length - count <= 0)
			{
				return String.Empty;
			}

			return s.Substring(s.Length - count, count);
		}

		public static int CountChars(this string s, Predicate<char> predicate)
		{
			return CountChars(s, predicate, 0, s.Length);
		}

		public static int CountChars(this string s, Predicate<char> acceptChar, int startIndex, int count)
		{
			int retval = 0;

			for (int i = startIndex; i < count; i++)
			{
				if (acceptChar(s[i]))
				{
					retval++;
				}
			}

			return retval;
		}

		public static string TrimLeft(this string s)
		{
			return s.TrimLeft(' ');
		}

		public static string TrimRight(this string s)
		{
			return s.TrimRight(' ');
		}

		public static string Trim(this string s)
		{
			return Trim(s, ' ');
		}

		public static string TrimLeft(this string s, char c)
		{
			int i;
			
			for (i = 0; i < s.Length; i++)
			{
				if (s[i] != c)
				{
					break;
				}
			}

			return s.Substring(i);
		}

		public static string TrimRight(this string s, char c)
		{
			int i;
			
			for (i = s.Length - 1; i >= 0; i--)
			{
				if (s[i] != c)
				{
					break;
				}
			}

			return s.Substring(0, i + 1);
		}

		public static string Trim(this string s, char c)
		{
			int x, y;
			
			for (x = 0; x < s.Length; x++)
			{
				if (s[x] != c)
				{
					break;
				}
			}

			for (y = s.Length - 1; y > x; y--)
			{
				if (s[y] != c)
				{
					break;
				}
			}

			return s.Substring(x, y + 1 - x);
		}

		public static string TrimLeft(this string s, Predicate<char> trimChar)
		{
			int i;
			
			for (i = 0; i < s.Length; i++)
			{
				if (!trimChar(s[i]))
				{
					break;
				}
			}

			return s.Substring(i);
		}

		public static string TrimRight(this string s, Predicate<char> trimChar)
		{
			int i;
			
			for (i = s.Length - 1; i >= 0; i--)
			{
				if (!trimChar(s[i]))
				{
					break;
				}
			}

			return s.Substring(0, i + 1);
		}

		public static string Trim(this string s, Predicate<char> trimChar)
		{
			int x, y;
			
			for (x = 0; x < s.Length; x++)
			{
				if (!trimChar(s[x]))
				{
					break;
				}
			}

			for (y = s.Length - 1; y > x; y--)
			{
				if (!trimChar(s[y]))
				{
					break;
				}
			}

			return s.Substring(x, y + 1 - x);
		}

		public static string TrimLeft(this string s, string match)
		{
			if (s.StartsWith(match))
			{
				return s.Substring(match.Length);
			}
			else
			{
				return s;
			}
		}

		public static string TrimRight(this string s, string match)
		{
			if (s.EndsWith(match))
			{
				return s.Substring(0, s.Length - match.Length);
			}
			else
			{
				return s;
			}
		}

		public static int IndexOf(this string s, Predicate<char> acceptChar)
		{
			return s.IndexOf(0, acceptChar);
		}

		//// <summary>
		/// Returns the index of the first character that satisfies the given predicate.
		/// </summary>
		/// <param name="s">The string to search.</param>
		/// <param name="acceptChar">
		/// The predicate that every character is asserted against.
		/// </param>
		/// <returns>
		/// The index of the first character that satisfies <c>predicate</c> or -1 if no
		/// characters satisfy <c>predicate</c>.
		/// </returns>
		public static int IndexOf(this string s, int startIndex, Predicate<char> acceptChar)
		{
			for (var i = startIndex; i < s.Length; i++)
			{
				if (acceptChar(s[i]))
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the last character that satisfies the given predicate.
		/// </summary>
		/// <param name="s">The string to search.</param>
		/// <param name="acceptChar">
		/// The predicate that every character is asserted against.
		/// </param>
		/// <returns>
		/// The index of the last character that satisfies <c>predicate</c> or -1 if no
		/// characters satisfy <c>predicate</c>.
		/// </returns>
		public static int LastIndexOf(this string s, Predicate<char> acceptChar)
		{
			for (var i = s.Length - 1; i >= 0; i--)
			{
				if (acceptChar(s[i]))
				{
					return i;
				}
			}

			return -1;
		}

		public static string LongHead(this string s)
		{
			if (s.Length <= 1)
			{
				return s;
			}

			return s.Substring(0, s.Length - 1);
		}

		public static string ShortTail(this string s)
		{
			if (s.Length == 0)
			{
				return s;
			}

			return s[s.Length - 1].ToString();
		}

		public static string Head(this string s)
		{
			if (s.Length == 0)
			{
				return s;
			}

			return s[0].ToString();
		}

		public static string Tail(this string s)
		{
			if (s.Length <= 1)
			{
				return s;
			}

			return s.Substring(1);
		}

		public static bool IsNumeric(this string s)
		{
			var i = 0;

			if (s[i] == '-')
			{
				i++;
			}

			for (; i < s.Length; i++)
			{
				if (!Char.IsNumber(s[i]))
				{
					return false;
				}
			}

			return true;
		}

		public static bool EndsWith(this string s, char value)
		{
			if (s.Length == 0)
			{
				return false;
			}

			return s[s.Length - 1] == value;
		}

		public static bool StartsWith(this string s, char value)
		{
			if (s.Length == 0)
			{
				return false;
			}

			return s[0] == value;
		}
	}
}
