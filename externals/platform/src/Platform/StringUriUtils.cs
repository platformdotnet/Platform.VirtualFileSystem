using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Platform.Text;

namespace Platform
{
	/// <summary>
	/// Provides useful methods for dealing with string based URIs.
	/// </summary>
	public sealed class StringUriUtils
	{
		/// <summary>
		/// Array of standard <c>acceptable</c> seperator chars for use when normalizing a path.
		/// </summary>
		/// <remarks>
		/// This array holds the chars <c>'/'</c> and <c>'\'</c>.
		/// </remarks>
		public static readonly char[] AcceptableSeperatorChars = new char[] { '/', '\\' };

		/// <summary>
		/// Returns the name part of a URL
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static string GetName(string uri)
		{
			return uri.SplitAroundCharFromRight(PredicateUtils.ObjectEqualsAny<char>('/', '\\')).Right;
		}

		public static Pair<string, string> GetSchemeAndPath(string uri)
		{
			var x = uri.IndexOf("://");

			if (x < 0)
			{
				return new Pair<string, string>("", uri);
			}
			else
			{
				return new Pair<string, string>(uri.Substring(0, x), uri.Substring(x + 1));
			}
		}

		public static string GetPath(string uri)
		{
			int x = uri.IndexOf("://");

			if (x < 0)
			{
				return uri;
			}
			else
			{
				return uri.Substring(x + 3);
			}
		}

		public static string GetScheme(string uri)
		{
			int x = uri.IndexOf("://");

			if (x < 0)
			{
				return "";
			}
			else
			{
				return uri.Substring(0, x);
			}
		}

		/// <summary>
		/// <see cref="NormalizePath(string, char[])"/>
		/// </summary>
		/// <remarks>
		/// Calls <see cref="NormalizePath(string, char[])"/> with the array
		/// <see cref="AcceptableSeperatorChars"/> which contains <c>'/'</c> and <c>'\'</c>.
		/// </remarks>
		public static string NormalizePath(string path)
		{
			return NormalizePath(path, 0, path.Length, AcceptableSeperatorChars, false);
		}

		public static string NormalizePath(string path, int startIndex, int count)
		{
			return NormalizePath(path, startIndex, count, AcceptableSeperatorChars, false);
		}

		private static bool CharArrayContains(char[] array, char c)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == c)
				{
					return true;
				}
			}

			return false;
		}

		[ThreadStatic]
		private static int[] backtrackStack = new int[0x100];

		/// <summary>
		/// Normalises a given path and returns the new normalised version.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The normalization process consists of the following:
		/// 
		/// Path elements consisting of '.' are removed.
		/// Path elements before path elements consisting of '..' are removed.
		/// The given chars are replaced with the standard URI seperator char '/'.
		/// </p>
		/// <p>
		/// Paths will always be returned without the trailing seperator char '/'.
		/// </p>
		/// </remarks>
		/// <param name="path">
		/// The path to normalise.
		/// </param>
		/// <returns>
		/// A normalised version of the path.
		/// </returns>
		public static string NormalizePath(string path, int startIndex, int count, char[] seperatorChars, bool preserveEndingSeperator)
		{
			int x, y, z, xi;
		    var startsWithSeperator = false;
			var endsWithSeperator = false;

			if (count == 0)
			{
				return "";
			}

			xi = 0;
			x = startIndex;

			if (StringUriUtils.backtrackStack == null)
			{
				lock (typeof(StringUriUtils))
				{
					if (StringUriUtils.backtrackStack == null)
					{
						StringUriUtils.backtrackStack = new int[100];
					}
				}
			}

			int[] localBackTrackStack = StringUriUtils.backtrackStack;

			localBackTrackStack[xi] = startIndex;

			startsWithSeperator = CharArrayContains(seperatorChars, path[startIndex]);
			endsWithSeperator = CharArrayContains(seperatorChars, path[path.Length - 1]);

			if (startsWithSeperator)
			{
				if  (count == 1)
				{
					return "/";
				}
				else
				{
					x = startIndex + 1;

					localBackTrackStack[xi] = startIndex;
				}
			}

			xi++;
			
			var builder = new StringBuilder(path.Length);
			
			while (x < (count + startIndex))
			{
				y = x;

				while (y < (count + startIndex))
				{
					if (CharArrayContains(seperatorChars, path[y]))
					{
						break;
					}
					
					y++;
				}

				z = y - x;

				if (z == 0)
				{
					// Ignore '//'.
				}
				else if (z == 1 && path[x] == '.')
				{
					// Ignore '/./'
				}
				else if (z == 2 && path[x] == '.' && path[x + 1] == '.')
				{
					if (x < 2)
					{
						throw new ArgumentException("Root (/) has no parent.", path);
					}

					if (builder.Length == 0)
					{
						throw new InvalidOperationException("Impossible attempt to go above path root (/).");
					}

					xi--;
					builder.Remove(StringUriUtils.backtrackStack[xi], builder.Length - StringUriUtils.backtrackStack[xi]);
				}
				else
				{
					localBackTrackStack[xi++] = builder.Length;

					if (x == 0)
					{
						if (startsWithSeperator)
						{
							builder.Append('/');
						}
					}
					else
					{
						builder.Append('/');
					}

					builder.Append(path, x, z);
				}

				x = y + 1;
			}
			
			if (builder.Length == 0)
			{
				if (startsWithSeperator)
				{
					return "/";
				}
				else
				{
					return ".";
				}
			}

			if (endsWithSeperator && preserveEndingSeperator)
			{
				builder.Append("/");
			}

			return builder.ToString();
		}

		public static string Combine(string left, string right)
		{
			if (left.EndsWith("/"))
			{
				if (right.StartsWith("/"))
				{
					return left.Substring(0, left.Length - 1) + right;
				}
				else
				{
					return left + right;
				}
			}
			else
			{
				if (right.StartsWith("/"))
				{
					return left + right;
				}
				else
				{
					return left + "/" + right;
				}
			}
		}

		public static string Unescape(string s)
		{
			var builder = new StringBuilder(s.Length + 10);

			for (var i = 0; i < s.Length; )
			{
				builder.Append(Uri.HexUnescape(s, ref i));
			}

			return builder.ToString();
		}

		public static string Escape(string s, Predicate<char> includeChar)
		{
		    var writer = new StringWriter();
			var reader = new StringReader(s);

			var charValue = reader.Read();

			while (charValue != -1)
			{
				if (((charValue >= 48) && (charValue <= 57)) // 0-9
					|| ((charValue >= 65) && (charValue <= 90)) // A-Z
					|| ((charValue >= 97) && (charValue <= 122))) // a-z
				{
					writer.Write((char)charValue);
				}
				else
				{
					writer.Write("%{0:x2}", charValue);
				}

				charValue = reader.Read();
			}

			return writer.ToString();
		}

		public static string UrlEncode(string instring)
		{
			return TextConversion.ToEscapedHexString(instring, TextConversion.IsStandardUrlEscapedChar);
		}

		public static string BuildQuery(NameValueCollection nameValueCollection)
		{
			return BuildQuery(Pair<string, string>.FromNameValueCollection(nameValueCollection), PredicateUtils<Pair<string, string>>.AlwaysTrue);
		}

		public static string BuildQuery(NameValueCollection nameValueCollection, Predicate<Pair<string, string>> acceptPair)
		{
			return BuildQuery(Pair<string, string>.FromNameValueCollection(nameValueCollection), acceptPair);
		}

		public static string BuildQuery(System.Collections.Generic.IEnumerable<KeyValuePair<string, string>> pairs)
		{
			return BuildQuery(pairs, PredicateUtils<Pair<string, string>>.AlwaysTrue);
		}

		public static string BuildQuery(System.Collections.Generic.IEnumerable<KeyValuePair<string, string>> pairs, Predicate<Pair<string, string>> acceptPair)
		{
			return BuildQuery(Pair<string, string>.FromKeyValuePairs(pairs), acceptPair);
		}

		public static string BuildQuery(System.Collections.Generic.IEnumerable<Pair<string, string>> pairs)
		{
			return BuildQuery(pairs, PredicateUtils<Pair<string, string>>.AlwaysTrue);
		}

		public static string BuildQuery(System.Collections.Generic.IEnumerable<Pair<string, string>> pairs, Predicate<Pair<string, string>> acceptPair)
		{
		    var builder = new StringBuilder();

			foreach (var pair in pairs)
			{
				if (acceptPair(pair))
				{
					builder.Append(UrlEncode(pair.Key));
					builder.Append('=');
					builder.Append(UrlEncode(pair.Value));
				}
			}

			return builder.ToString();
		}

		public static System.Collections.Generic.IEnumerable<Pair<string, string>> ParseQuery(string query)
		{
			string[] ss;
						
			if (query == "")
			{
				yield break;
			}

			ss = query.Split('&');

			for (var i = 0; i < ss.Length; i++)
			{
				int j;
				string key, value;

				j = ss[i].IndexOf('=');

				if (j < 0)
				{
					key = ss[i];
					value="";
				}
				else
				{
					key = ss[i].Substring(0, j);
					value = ss[i].Substring(j + 1);
				}

				yield return new Pair<string, string>(key, value);
			}
		}

		public static bool ContainsQuery(string uri)
		{
			return uri.LastIndexOf('?') >= 0;
		}

		public static string RemoveQuery(string uri)
		{
			return uri.Left(PredicateUtils.ObjectEquals<char>('?').Not());
		}

		public static string AppendQueryPart(string uri, string key, string value)
		{
			if (uri.LastIndexOf('?') > 0)
			{
				uri += "&";
			}
			else
			{
				uri += '?';
			}
			
			uri += UrlEncode(key) + "=" + UrlEncode(value);

			return uri;
		}
	}
}
