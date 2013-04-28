using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Platform.IO
{
	/// <summary>
	/// Provides extension methods and static utility methods for <see cref="Stream"/> objects.
	/// </summary>
	public static class StreamUtils
	{
		/// <summary>
		/// Writes the given buffer to the stream
		/// </summary>
		/// <param name="s">The stream to write to</param>
		/// <param name="buffer">The buffer to write</param>
		public static void WriteAll(this Stream s, byte[] buffer)
		{
			s.Write(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Gets an <see cref="IEnumerator{T}"/> for the given stream
		/// </summary>
		/// <param name="s">The stream</param>
		/// <returns>An <see cref="IEnumerator{T}"/></returns>
		public static IEnumerator<byte> GetEnumerator(this Stream s)
		{
			return s.ToEnumerable().GetEnumerator();
		}

		/// <summary>
		/// Converts the given stream into an <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <param name="s">The stream</param>
		/// <returns>An <see cref="IEnumerable{T}"/></returns>
		public static IEnumerable<byte> ToEnumerable(this Stream s)
		{
			int x;

			for (;;)
			{
				x = s.ReadByte();

				if (x == -1)
				{
					break;
				}

				yield return (byte)x;
			}
		}

		[ThreadStatic]
		private static byte[] dynamicBlockBuffer;

		/// <summary>
		/// Reads a block from a stream while the given predicate validates.
		/// </summary>
		/// <param name="stream">The stream to read from</param>
		/// <param name="keepGoing">
		/// A predicate that will be passed the current byte and evaluate to
		/// True the method should continue reading from the stream
		/// </param>
		/// <returns>
		/// A <see cref="Pair{A,B}"/> containing an array of bytes containing the block read
		/// and the total length of valid data read.  The array may be larger than the length
		/// of valid data read.  All elements in the array after the valid data will be 0.
		/// Use <see cref="Array.Resize{T}"/> to resize the array to the exact length if necessary.
		/// </returns>
		public static Pair<byte[], int> ReadDynamicBlock(this Stream stream, Predicate<byte> keepGoing)
		{
			int x;
			int length;
			byte[] buffer;

			if (dynamicBlockBuffer == null)
			{
				dynamicBlockBuffer = new byte[256];
			}

			buffer = dynamicBlockBuffer;

			length = 0;

			for (;;)
			{
				x = stream.ReadByte();

				if (x == -1 || !keepGoing((byte)x))
				{
					break;
				}

				if (length == buffer.Length)
				{
					byte[] newBuffer;

					newBuffer = new byte[buffer.Length * 2];

					Array.Copy(buffer, newBuffer, buffer.Length);

					buffer = newBuffer;
				}

				buffer[length++] = (byte)x;
			}

			return new Pair<byte[], int>(buffer, length);
		}

		[ThreadStatic]
		private static char[] t_CharBuffer = null;

		/// <summary>
		/// Reads a textual line from a stream without reading ahead and consuming more
		/// bytes than necessary to read the line.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is intended to be use where a stream is used with both binary
		/// data and encoded text.
		/// </para>
		/// <para>
		/// Because decoding without buffering requires detecting end of line characters
		/// reliabily without buffering, this method only supports the UTF-8 and ASCII
		/// encodings.
		/// </para>
		/// <para>
		/// This method uses TLS buffers and is thread safe.
		/// </para>
		/// </remarks>
		/// <param name="stream">
		/// The stream to read the textual line from.
		/// </param>
		/// <param name="encoding">
		/// The encoding to use.
		/// Only <see cref="Encoding.UTF8"/> and <see cref="Encoding.ASCII"/> are supported.
		/// </param>
		/// <param name="maxBytes">
		/// The maximum number of bytes to read before throwing an <see cref="OverflowException"/>.
		/// If maxbytes is encountered, the stream is left in an unstable state for reading
		/// text.
		/// </param>
		/// <returns></returns>
		public static string ReadLineUnbuffered(this Stream stream, Encoding encoding, int maxBytes)
		{			
			int charCount;
			int maxCharCount;
			int lastByte = -1;
			int byteCount = 0;
			Pair<byte[], int> retval;

			if (encoding != Encoding.UTF8 && encoding != Encoding.ASCII)
			{
				throw new ArgumentException(encoding.ToString(), "encoding");
			}

			retval = stream.ReadDynamicBlock(delegate(byte x)
				{
					if (x == '\n' && lastByte == '\r')
					{
						return false;
					}

					lastByte = x;
					byteCount++;

					if (byteCount > maxBytes)
					{
						throw new OverflowException();
					}

					return true;
				});

			maxCharCount = encoding.GetMaxCharCount(retval.Right);

			if (t_CharBuffer == null
				|| (t_CharBuffer != null && maxCharCount > t_CharBuffer.Length))
			{
				int newLength = t_CharBuffer == null ? 64 : t_CharBuffer.Length;

				while (newLength < maxCharCount)
				{
					newLength *= 2;
				}

				Array.Resize(ref t_CharBuffer, newLength);
			}

			charCount = Encoding.UTF8.GetChars(retval.Left, 0, lastByte == '\r' ? retval.Right - 1 : retval.Right, t_CharBuffer, 0);

			return new string(t_CharBuffer, 0, charCount);
		}
	}
}
