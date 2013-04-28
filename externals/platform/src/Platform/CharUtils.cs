#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Platform
{
	/// <summary>
	/// Provides extension methods for chars
	/// </summary>
	public static class CharUtils
	{
		/// <summary>
		/// Returns True if the char is an ascii letter (A-Z, a-z) or a digit (0-9).
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>True if the char is an ascii letter or digit</returns>
		public static bool IsAsciiLetterOrDigit(this char c)
		{
			return (((c >= 48) && (c <= 57)) // 0-9
				|| ((c >= 65) && (c <= 90)) // A-Z
				|| ((c >= 97) && (c <= 122))); // a-z
		}

		/// <summary>
		/// Returns true if the char is a hexadecimal digit (0-9, A-F, a-f).
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>True if the char is a hexadecimal digit</returns>
		public static bool IsHexDigit(this char c)
		{
			return Uri.IsHexDigit(c);
		}
	}
}
