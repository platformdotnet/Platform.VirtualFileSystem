using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Platform.Text;
using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text
{
	public class CommandResponse
	{
		public virtual string ResponseType { get; private set; }
		public virtual string ResponseTupleString { get; private set; }
		public virtual IDictionary<string, string> ResponseTuples { get; private set; }

		private static readonly Regex responseTupleRegex;

		static CommandResponse()
		{
			responseTupleRegex = new Regex(@"
					([ ]*
						(?<keyvalue>(
							(("" (?<key>([^\""\=])+) "") | (?<key>([^\""]([^ \=])+)))
							\=
							(("" (?<value>(([^""])*)) "") | (?<value>([^\""]([^ ])*)))
						))
					)
					|(?<other>.*)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);
		}

		public static void ParseTupleString(string tupleString, IDictionary<string, string> tupleDictionary)
		{
			foreach (var keyValuePair in ParseTupleString(tupleString))
			{
				tupleDictionary[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		public static IEnumerable<KeyValuePair<string, string>> ParseTupleString(string tupleString)
		{
			var match = responseTupleRegex.Match(tupleString);

			while (match.Success)
			{
				string s;

				var group = match.Groups["other"];

				s = group.Value;

				if (group.Captures.Count > 0)
				{
					if (group.Length > 0)
					{
						throw new FormatException(tupleString);
					}
					else
					{
						match = match.NextMatch();

						continue;
					}
				}

				var key = match.Groups["key"].Value;
				var value = match.Groups["value"].Value;

				yield return new KeyValuePair<string, string>(key, value);

				match = match.NextMatch();
			}
		}

		public CommandResponse(string responseType, string responseTupleString)
		{
			this.ResponseType = responseType;
			this.ResponseTupleString = responseTupleString;
			this.ResponseTuples = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

			ParseTupleString(responseTupleString, this.ResponseTuples);
		}

		public virtual CommandResponse ProcessError()
		{
			var e = GetErrorException();

			if (e != null)
			{
				throw e;
			}

			return this;
		}

		public virtual Exception GetErrorException()
		{
			if (this.ResponseType == ResponseCodes.ERROR)
			{
				var errorCode = this.ResponseTuples["code"];

				if (errorCode.Equals(ErrorCodes.END_OF_FILE, StringComparison.CurrentCultureIgnoreCase))
				{
					return new EndOfStreamException();
				}
				else if (errorCode.Equals(ErrorCodes.FILE_NOT_FOUND, StringComparison.CurrentCultureIgnoreCase))
				{
					return new FileNodeNotFoundException(this.ResponseTuples["uri"]);
				}
				else if (errorCode.Equals(ErrorCodes.DIRECTORY_NOT_FOUND, StringComparison.CurrentCultureIgnoreCase))
				{
					return new DirectoryNodeNotFoundException(this.ResponseTuples["uri"]);
				}
				else if (errorCode.Equals(ErrorCodes.UNAUTHORISED, StringComparison.CurrentCultureIgnoreCase))
				{
					return new UnauthorizedAccessException(TextConversion.FromEscapedHexString(this.ResponseTuples["details"]));
				}
				else
				{
					return new TextNetworkProtocolErrorResponseException(TextConversion.FromEscapedHexString(this.ResponseTupleString));
				}
			}

			return null;
		}
	}
}
