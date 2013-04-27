using System;
using System.Text.RegularExpressions;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers.Local
{
	[Serializable]
	public class LocalNodeAddress
		: AbstractNodeAddressWithRootPart
	{
		private static readonly Regex localFileNameRegEx;		
		public const string LowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
		public const string UpperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private static readonly char[] validFileNameChars;
		private static readonly string validFileNameString;

		public static bool IsValidFileNameChar(char c)
		{
			return Array.BinarySearch<char>(validFileNameChars, c) >= 0;
		}

		static LocalNodeAddress()
		{
			string exp;

			validFileNameChars = (@"\-+,;=\[\].$%_@~`!(){}^#&-" + UpperAlphabet + LowerAlphabet).ToCharArray();

			Array.Sort<char>(validFileNameChars);

			validFileNameString = validFileNameChars.ToString();

			exp =


				@"
						# The optional file:// part

						[ ]*

						^(((?<scheme>[a-z]*)\:[\\/]{2,2})?)

						(
							# UNC paths

							(
								([\\/]{2,2})
								(?<uncserver>(([a-zA-Z\-\.0-9]*)|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})))
								#(?<path1>([ \\/][\-+,;=\[\].$%_@~`!(){}^#&a-zA-Z0-9\\/]*)*)  (\?(?<query>(.*)))?$
								#(?<path1>([^\?\""\<\>\|\/\\\*:]*)*)  (\?(?<query>(.*)))?$
								(?<path1>([/\\] [^\?\""\<\>\|\/\*:\\]+)+[/\\]? | [/\\]?)
								(\?(?<query>(.*)))?$
							)

							|

							# Windows paths

							
							(
								(?<root>([a-zA-Z]\:))
								(?<path2>([/\\] [^\?\""\<\>\|\/\*:\\]+)+[/\\]? | [/\\]?)
								#(?<path2>[/\\]? | ([/\\] [ \-\+,;=\[\].$%_@~`!(){}^#&\p{N}\p{L}]+)+[/\\]?)
								(\?(?<query>(.*)))?$
							)

							|

							# UNIX paths

							(
								#(?<path3>/ | /([ \-\+,;=\[\].$%_@~`!(){}^#&a-zA-Z0-9]+)+/?)     (\?(?<query>(.*)))?$
								#(?<path3>/ | /([^\?\""\<\>\|\/\\\*:]+)+/?)     (\?(?<query>(.*)))?$
								(?<path3>([/] [^\?\""\<\>\|\/\*:]+)+[/]? | [/\\]?)
								(\?(?<query>(.*)))?$
							)

							|

							# Relative Paths

							(
								(?<path4>([/\\]? | (([\.]{1,2}[/\\]?)|([\.]{1,2}[/\\])+) (([/] [^\?\""\<\>\|\/\*:]+)+[/]? | [/\\]?)?  ))
								(\?(?<query>(.*)))?$
							)
						)

						[ ]*
				";
				
			localFileNameRegEx = new Regex
			(	
				exp,
				RegexOptions.ExplicitCapture |
				RegexOptions.IgnoreCase |
				RegexOptions.IgnorePatternWhitespace |
				RegexOptions.Compiled |				
				RegexOptions.CultureInvariant
			);
		}

		private readonly bool includeRootPartInUri;

		public LocalNodeAddress(string scheme, string rootPart, bool includeRootPartInUri, string path)
			: this(scheme, rootPart, includeRootPartInUri, path, "")
		{
		}

		public LocalNodeAddress(string scheme, string rootPart, bool includeRootPartInUri, string path, string query)
			: base(scheme, rootPart, path, query)
		{
			this.includeRootPartInUri = includeRootPartInUri;
		}
		
		[ThreadStatic]
		private static string lastCanParseUri;

		[ThreadStatic]
		private static Match lastCanParseMatch;

		public static bool CanParse(string uri)
		{
			return CanParse(uri, "file");
		}

		public static bool CanParse(string uri, string scheme)
		{
			if (!((uri.Length >= 2 && Char.IsLetter(uri[0]) && uri[1] == ':')
				|| (uri == null || (uri != null 
				&& uri.StartsWith(scheme, StringComparison.CurrentCultureIgnoreCase)))
				|| uri.StartsWith("/") 
				|| uri.StartsWith(@"\")
				|| uri == "."
				|| uri == ".."
				|| uri.StartsWith("./")
				|| uri.StartsWith("../")
				|| uri.StartsWith(".\\")
				|| uri.StartsWith("..\\")))
			{
				// Fast fail path out.
                
				return false;
			}
						
			if (lastCanParseUri == uri && lastCanParseMatch != null && lastCanParseMatch.Success)
			{
				return true;
			}

			// Use regular expression to totally verify.

			lastCanParseUri = uri;
			lastCanParseMatch = localFileNameRegEx.Match(uri);

			return lastCanParseMatch.Success;
		}

		public static LocalNodeAddress Parse(string uri)
		{
			Group group;
			Match match;
			string root, scheme, query;
			
			// Often Parse will be called with the exact same URI reference that was last passed
			// to CanParse.  If this is the case then use the results cached by the last call to
			// CanParse from this thread.

			if ((object)uri == (object)lastCanParseUri)
			{
				match = lastCanParseMatch;
			}
			else
			{
				match = localFileNameRegEx.Match(uri);
			}

			if (!match.Success)
			{
				throw new MalformedUriException(uri);
			}

			bool schemeExplicitlyProvided;

			group = match.Groups["scheme"];			
			
			if (group.Value == "")
			{
				scheme = "file";
				schemeExplicitlyProvided = false;
			}
			else
			{				
				scheme = group.Value;
				schemeExplicitlyProvided = true;
			}

			group = match.Groups["uncserver"];
			
			if (group.Success)
			{
				string path;
				Pair<string, string> result;

				path = match.Groups["path1"].Value;

				result = path.SplitAroundCharFromLeft(1, PredicateUtils.ObjectEqualsAny('\\', '/'));

				root = "//" + group.Value + result.Left.Replace('\\', '/');
				path = "/" + result.Right;
												
				if (path == "")
				{
					path = "/";
				}

				query = match.Groups["query"].Value;

				return new LocalNodeAddress(scheme, root, true, StringUriUtils.NormalizePath(path), query);
			}
			else
			{
				string path;

				group = match.Groups["root"];

				if (group.Captures.Count > 0)
				{
					///
					/// Windows path specification
					///

					root = group.Value;

					path = match.Groups["path2"].Value;

					if (path.Length == 0)
					{
						path = FileSystemManager.SeperatorString;
					}

					query = match.Groups["query"].Value;

					path = StringUriUtils.NormalizePath(path);

					if (schemeExplicitlyProvided)
					{
						///
						/// Explicitly provided scheme means
						/// special characters are hexcoded
						///

						path = TextConversion.FromEscapedHexString(path);
						query = TextConversion.FromEscapedHexString(query);
					}

					return new LocalNodeAddress(scheme, root, true, path, query);
				}
				else if (match.Groups["path3"].Value != "")
				{
					///
					/// Unix path specification
					///

					path = match.Groups["path3"].Value;
					query = match.Groups["query"].Value;

					path = StringUriUtils.NormalizePath(path);

					if (schemeExplicitlyProvided)
					{
						///
						/// Explicitly provided scheme means
						/// special characters are hexcoded
						///

						path = TextConversion.FromEscapedHexString(path);
						query = TextConversion.FromEscapedHexString(query);
					}

					return new LocalNodeAddress(scheme, "", true, path, query);
				}
				else
				{
					///
					/// Relative path specification
					///

					path = match.Groups["path4"].Value;
					query = match.Groups["query"].Value;

					path = StringUriUtils.Combine(Environment.CurrentDirectory, path);

					path = StringUriUtils.NormalizePath(path);

					if (schemeExplicitlyProvided)
					{
						///
						/// Explicitly provided scheme means
						/// special characters are hexcoded
						///

						path = TextConversion.FromEscapedHexString(path);
						query = TextConversion.FromEscapedHexString(query);
					}

					return new LocalNodeAddress(scheme, "", true, path, query);
				}
			}
		}

		protected override INodeAddress CreateAddress(string path, string query)
		{
			if (this.GetType() == typeof(LocalNodeAddress))
			{
				return new LocalNodeAddress(this.Scheme, this.RootPart, this.includeRootPartInUri, path, query);
			}
			else
			{
				return (LocalNodeAddress)Activator.CreateInstance(GetType(), this.Scheme, this.RootPart, this.includeRootPartInUri, path, query);
			}
		}

		protected override string GetRootUri()
		{
			if (this.includeRootPartInUri)
			{
				return this.Scheme + "://" + this.RootPart;
			}
			else
			{
				return this.Scheme + "://";
			}
		}

		public override INodeAddress CreateAsRoot(string scheme)
		{
			if (this.AbsolutePath == FileSystemManager.RootPath)
			{
				return new LocalNodeAddress(scheme, this.RootPart, false, "/");
			}
			else
			{
				return new LocalNodeAddress(scheme, this.RootPart + this.AbsolutePath, false, "/");
			}
		}

		public virtual string AbsoluteNativePath
		{
			get
			{
				string s;

				s = this.RootPart + TextConversion.FromEscapedHexString(this.AbsolutePath);

				if (System.IO.Path.DirectorySeparatorChar != '/')
				{
					s = s.Replace(System.IO.Path.DirectorySeparatorChar, '/');
				}

				s = System.IO.Path.GetFullPath(s);

				return s;
			}
		}
	}
}
