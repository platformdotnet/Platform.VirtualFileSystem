using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace Platform.IO
{
	/// <summary>
	/// Provides extension methods for <see cref="TextReader"/> classes
	/// </summary>
	public static class TextReaderUtils
	{
		private static readonly char[] discardBuffer = new char[1024];

		public static string ReadToEndThenClose(this TextReader reader)
		{
			using (reader)
			{
				return reader.ReadToEnd();
			}
		}

		public static void DiscardToEnd(this TextReader reader)
		{
			while (reader.Read(discardBuffer, 0, discardBuffer.Length) != 0) ;
		}

		/// <summary>
		/// Dumps all the data in the given <see cref="reader"/> to the given <see cref="writer"/>;
		/// converting each character using the given <see cref="Converter{TInput,TOutput}"/>
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to read from</param>
		/// <param name="writer">The <see cref="TextWriter"/> to write to</param>
		/// <param name="convert">The <see cref="Converter{TInput,TOutput}"/> to use</param>
		/// <returns>
		/// The same <see cref="TextWriter"/> that was passed in as <see cref="writer"/>
		/// </returns>
		public static TextWriter ConvertAndDump(this TextReader reader, TextWriter writer, Converter<char, char> convert)
		{
		    while (true)
			{
				var charValue = reader.Read();

				if (charValue == -1)
				{
					break;
				}

				writer.Write(convert((char)charValue));
			}

			return writer;
		}

		/// <summary>
		/// Dumps all the data in the given <see cref="reader"/> to the given <see cref="writer"/>;
		/// using the provided routine (<see cref="convertWriter"/>) to perform the conversion and writing.
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to read from</param>
		/// <param name="writer">The <see cref="TextWriter"/> to write to</param>
		/// <param name="convertWriter">
		/// A routine that takes the current character and the <see cref="writer"/>.  The routine should
		/// convert the character into one or more characters and write it to the <see cref="writer"/>.
		/// </param>
		/// <returns>
		/// The same <see cref="TextWriter"/> that was passed in as <see cref="writer"/>
		/// </returns>
		public static TextWriter ConvertAndDump(this TextReader reader, TextWriter writer, Action<char, TextWriter> convertWriter)
		{
			while (true)
			{
			    var charValue = reader.Read();

				if (charValue == -1)
				{
					break;
				}

				convertWriter((char)charValue, writer);
			}

			return writer;
		}
		
		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> until a space is encountered
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <returns>The string that was read</returns>
		public static string ReadWord(this TextReader reader)
		{
			return reader.ReadWhile(PredicateUtils.ObjectNotEquals(' '));
		}

		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> until a space is encountered
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="maximumLength">The maximum number of characters to read</param>
		/// <returns>The string that was read</returns>
		public static string ReadWord(TextReader reader, int maximumLength)
		{
			return reader.ReadWhile(PredicateUtils.ObjectNotEquals(' '), maximumLength);
		}

		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> until a space is encountered
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="maximumLength">The maximum number of characters to read</param>
		/// <param name="overflow">A bool that will return True if the maximumLength was reached before a full word was read</param>
		/// <returns>The string that was read</returns>
		public static string ReadWord(this TextReader reader, int maximumLength, out bool overflow)
		{
			return reader.ReadWhile(PredicateUtils.ObjectNotEquals(' '), maximumLength, out overflow);
		}

		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> while specific characters are accepted by a predicate.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="acceptChar">
		/// A predicate that accepts the current character and returns True if a given character should be returned.
		/// When the predicate returns false, the given character is pushed back onto the read and the method returns
		/// with a string made up of the currently accepted characters.
		/// </param>
		/// <returns>The string that was read</returns>
		public static string ReadWhile(this TextReader reader, Predicate<char> acceptChar)
		{
			return reader.ReadWhile(acceptChar, -1);
		}

		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> while specific characters are accepted by a predicate.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="acceptChar">
		/// A predicate that accepts the current character and returns True if a given character should be returned.
		/// When the predicate returns false, the given character is pushed back onto the read and the method returns
		/// with a string made up of the currently accepted characters.
		/// </param>
		/// <param name="maximumLength">The maximum length of the string to read</param>
		/// <returns>The string that was read</returns>
		public static string ReadWhile(this TextReader reader, Predicate<char> acceptChar, int maximumLength)
		{
			string s;
			bool overflow;

			s = reader.ReadWhile(acceptChar, maximumLength, out overflow);

			if (overflow)
			{
				throw new OverflowException();
			}

			return s;
		}

		/// <summary>
		/// Reads a string form the <see cref="TextReader"/> while specific characters are accepted by a predicate.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="acceptChar">
		/// A predicate that accepts the current character and returns True if a given character should be returned.
		/// When the predicate returns false, the given character is pushed back onto the read and the method returns
		/// with a string made up of the currently accepted characters.
		/// </param>
		/// <param name="maximumLength">The maximum length of the string to read</param>
		/// <param name="overflow">A bool that will return True if the maximumLength was reached before a full word was read</param>
		/// <returns>The string that was read</returns>
		public static string ReadWhile(this TextReader reader, Predicate<char> acceptChar, int maximumLength, out bool overflow)
		{
		    var length = 0;

		    overflow = false;

			if (maximumLength == 0)
			{
				return String.Empty;
			}

			var buffer = new StringBuilder();

			for (;;)
			{
				var x = reader.Read();

				if (x != -1 && acceptChar((char)x))
				{
					buffer.Append(x);

					length++;
				}
				else
				{
					return buffer.ToString();
				}

				if (maximumLength > 0 && buffer.Length == maximumLength)
				{
					if (reader.Peek() != -1)
					{
						overflow = true;
					}

					return buffer.ToString();
				}
			}
		}

		/// <summary>
		/// Reads a line from the given reader.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="maximumLength">The maximum length of the string to return</param>
		/// <returns>The read line</returns>
		public static string ReadLine(this TextReader reader, int maximumLength)
		{
			string s;
			bool overflow;

			s = reader.ReadLine(maximumLength, out overflow);

			if (overflow)
			{
				throw new OverflowException();
			}

			return s;
		}

		/// <summary>
		/// Reads a line from the given reader.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="maximumLength">The maximum length of the string to return</param>
		/// <param name="overflow">A bool that will return True if the maximumLength was reached before a full word was read</param>
		/// <returns>The read line</returns>
		public static string ReadLine(this TextReader reader, int maximumLength, out bool overflow)
		{
			int x, y;
			StringBuilder buffer;

			overflow = false;
			buffer = new StringBuilder(Math.Min(64, maximumLength));

			for (;;)
			{
				x = reader.Read();

				if (x == -1)
				{
					if (buffer.Length == 0)
					{
						return null;
					}

					break;
				}

				if (x == '\r')
				{
					y = reader.Peek();

					if (y == '\n')
					{
						reader.Read();

						break;
					}
					else
					{
						buffer.Append(x);

						if (buffer.Length == maximumLength)
						{
							break;
						}

						x = reader.Read();
					}
				}

				buffer.Append((char)x);

				if (buffer.Length == maximumLength)
				{
					break;
				}
			}

			if (buffer.Length == maximumLength)
			{
				if (reader.Peek() != -1)
				{
					overflow = true;
				}
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Reads an enumeration of lines from the given <see cref="reader"/>
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <returns>An enumeration of lines (computed lazily)</returns>
		public static IEnumerable<string> ReadLines(this TextReader reader)
		{
			for (;;)
			{
				string s;

				s = reader.ReadLine();

				if (s == null)
				{
					yield break;
				}

				yield return s;
			}
		}

		/// <summary>
		/// Class to support <see cref="TextReaderUtils.MergeTextReadersAsLineReader"/>
		/// </summary>
		private class MergedLineReader
			: ILineReader
		{
			private int endOfInput;
			private string newLine = null;						
			private object lockobject = new object();
			private Exception errorException = null;
			private volatile bool disposed = false;

			public MergedLineReader(MergeTextReadersAsLineReaderParameters parameters, params TextReader[] readers)
			{
			    endOfInput = readers.Length;

				Action<TextReader> routine = delegate(TextReader reader)
				                  {
				                      try
				                      {
				                          while (true)
				                          {
				                              bool exit = false;
				                              string line;

				                              lock (lockobject)
				                              {
				                                  while (newLine != null)
				                                  {
				                                      Monitor.Wait(lockobject, 500);

				                                      if (disposed)
				                                      {
				                                          exit = true;

				                                          break;
				                                      }
				                                  }
				                              }

				                              if (exit)
				                              {
				                                  break;
				                              }

				                              line = reader.ReadLine();

				                              lock (lockobject)
				                              {
				                                  while (newLine != null)
				                                  {
				                                      Monitor.Wait(lockobject, 500);

				                                      if (disposed)
				                                      {
				                                          exit = true;

				                                          break;
				                                      }
				                                  }

				                                  if (exit)
				                                  {
				                                      break;
				                                  }

				                                  newLine = line;

				                                  Monitor.PulseAll(lockobject);

				                                  if (newLine == null)
				                                  {
				                                      break;
				                                  }
				                              }
				                          }
				                      }
				                      catch (Exception e)
				                      {
				                          errorException = e;
				                      }
				                      finally
				                      {
				                          lock (lockobject)
				                          {
				                              newLine = null;

				                              Interlocked.Decrement(ref endOfInput);

				                              Monitor.PulseAll(lockobject);
				                          }
				                      }
				                  };

				foreach (var reader in readers)
				{
					Thread thread;
				    var currentReader = reader;

					if (parameters == MergeTextReadersAsLineReaderParameters.UseNewThreads)
					{
						thread = new Thread(() => routine(currentReader));

						thread.Start();
					}
					else if (parameters == MergeTextReadersAsLineReaderParameters.UseSystemThreadPool)
					{
						ThreadPool.QueueUserWorkItem(delegate { routine(currentReader); });
					}
				}
			}

			public virtual string ReadLine()
			{
				string line;

				ReadLine(TimeSpan.FromMilliseconds(-1), out line);

				return line;
			}

			public virtual bool ReadLine(TimeSpan timeout, out string outputLine)
			{
				lock (lockobject)
				{
					while (true)
					{
						if (newLine != null)
						{
							string s;

							s = newLine;

							newLine = null;

							Monitor.PulseAll(lockobject);

							outputLine = s;

							return true;
						}
						else if (errorException != null)
						{
							throw errorException;
						}
						else if (endOfInput == 0)
						{
							outputLine = null;

							return false;
						}
						else
						{
							if (!Monitor.Wait(lockobject, timeout))
							{
								outputLine = null;

								return false;
							}
						}
					}
				}
			}

			public virtual void Dispose()
			{
				disposed = true;
			}
		}

		/// <summary>
		/// Parameters for the <see cref="TextReaderUtils.MergeTextReadersAsLineReader"/> method
		/// </summary>
		public enum MergeTextReadersAsLineReaderParameters
		{
			/// <summary>
			/// Use new threads
			/// </summary>
			UseNewThreads = 1,

			/// <summary>
			/// Use threads from the <see cref="ThreadPool"/>
			/// </summary>
			UseSystemThreadPool = 2
		}

		/// <summary>
		/// Merge the given <see cref="TextReader"/> objects as a single <see cref="TextReader"/> for
		/// the purpose of reading lines using the <see cref="ILineReader"/> interface from all the
		/// given <see cref="TextReader"/> objects without blocking on any one <see cref="TextReader"/>.
		/// Lines form the <see cref="TextReader"/> will generally (not guarranteed) be read in the 
		/// order in which they become available.
		/// </summary>
		/// <param name="parameters">The paramters for reading</param>
		/// <param name="readers">The <see cref="TextReader"/> objects to read from</param>
		/// <returns>An <see cref="ILineReader"/> for reading lines from the given <see cref="readers"/></returns>
		public static ILineReader MergeTextReadersAsLineReader(MergeTextReadersAsLineReaderParameters parameters, params TextReader[] readers)
		{
			return new MergedLineReader(parameters, readers);
		}
	}
}
