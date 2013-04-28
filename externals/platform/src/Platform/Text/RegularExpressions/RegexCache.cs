using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Platform.Collections;

namespace Platform.Text.RegularExpressions
{
	public class RegexCache
	{
		public static RegexCache GlobalCache
		{
			get
			{
				return globalCache;
			}
		}
		private static readonly RegexCache globalCache = new RegexCache(TimeSpan.FromMinutes(60));

		private struct CacheKey
		{
		    private string regex;
		    private RegexOptions options;

			public CacheKey(string regex, RegexOptions options)
			{
				this.regex = regex;
				this.options = options;
			}
		}

		private readonly ILDictionary<CacheKey, Regex> cache;

		public RegexCache(TimeSpan timeout)
		{
			cache = new TimedReferenceDictionary<CacheKey, Regex>
			(
				timeout, typeof(Dictionary<,>)
			);
		}

		public Regex NewRegex(string regex, RegexOptions options)
		{
			lock (cache.SyncLock)
			{
				Regex retval;
				var key = new CacheKey(regex, options);

				if (cache.TryGetValue(key, out retval))
				{
					return retval;
				}

				options |= RegexOptions.Compiled;

				retval = new Regex(regex, options);

				cache[key] = retval;

				return retval;
			}
		}
	}
}

