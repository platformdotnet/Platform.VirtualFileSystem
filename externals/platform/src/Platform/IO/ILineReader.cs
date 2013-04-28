using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.IO
{
	/// <summary>
	/// An interface for supporting of reading lines with timeouts.  Usually used
	/// with the <see cref="TextReaderUtils.MergeTextReadersAsLineReader"/> method.
	/// </summary>
	/// <seealso cref="TextReaderUtils.MergeTextReadersAsLineReader"/>
	public interface ILineReader
		: IDisposable
	{
		/// <summary>
		/// Reads a single line.
		/// </summary>
		/// <returns>The line read</returns>
		string ReadLine();

		/// <summary>
		/// Reads a single line
		/// </summary>
		/// <param name="timeout">The timeout period to cancel reading the line</param>
		/// <param name="line">The line read</param>
		/// <returns>True if a line was read before the timeout period</returns>
		bool ReadLine(TimeSpan timeout, out string line);
	}

}
