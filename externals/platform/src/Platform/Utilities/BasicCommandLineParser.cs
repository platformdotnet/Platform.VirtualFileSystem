using System;
using System.Collections;
using System.Text;

namespace Platform.Utilities
{
	/// <summary>
	/// A simple Unix-style command line parser that supports quoted arguments.
	/// </summary>
	/// <example>
	/// string[] args = BasicCommandLineParser.Default.Parse("-name \"Jack Daniels\"");
	/// </example>
	public class BasicCommandLineParser
	{
		/// <summary>
		/// Gets the default singleton instance of the command line parser
		/// </summary>
		/// <remarks>
		/// Command Line Parser
		/// </remarks>
		public static BasicCommandLineParser Default
		{
			get
			{
				return instance;
			}
		}
		private static readonly BasicCommandLineParser instance = new BasicCommandLineParser();

		/// <summary>
		/// Parse the supplied command line
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public virtual string[] Parse(string commandLine)
		{
			bool instring = false;
			var list = new ArrayList();
			var buffer = new StringBuilder();

			foreach (char c in commandLine)
			{
				if (c == '\"')
				{
					if (instring)
					{
						instring = false;
						list.Add(buffer.ToString());
						buffer.Length = 0;
					}
					else if (buffer.Length == 0)
					{
						instring = true;
					}
					else
					{
						buffer.Append('\"');
					}
				}
				else if (c == ' ')
				{
					if (instring)
					{
						buffer.Append(c);
					}
					else
					{
						if (buffer.Length != 0)
						{
							list.Add(buffer.ToString());
							buffer.Length = 0;
						}
					}
				}
				else
				{
					buffer.Append(c);
				}
			}

			if (buffer.Length != 0)
			{
				list.Add(buffer.ToString());
			}

			return (string[])list.ToArray(typeof(string));
		}
	}
}
