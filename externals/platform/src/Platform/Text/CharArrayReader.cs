using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Platform.Text
{
	/// <summary>
	/// A reader that reads from an array of <see cref="char"/>.
	/// </summary>
	public class CharArrayReader
		: TextReader
	{
		private int offset;
		private int endIndex;
		private char[] array;

		/// <summary>
		/// Constructs a new <see cref="CharArrayReader"/>
		/// </summary>
		/// <param name="array">The array of chars to read from</param>
		public CharArrayReader(char[] array)
			: this(array, 0)
		{
		}

		/// <summary>
		/// Constructs a new <see cref="CharArrayReader"/>
		/// </summary>
		/// <param name="array">The array of chars to read from</param>
		/// <param name="offset">The offset within the array to start reading from</param>
		public CharArrayReader(char[] array, int offset)
			: this(array, offset, array.Length - offset)
		{
		}

		/// <summary>
		/// Constructs a new <see cref="CharArrayReader"/>
		/// </summary>
		/// <param name="array">The array of chars to read from</param>
		/// <param name="offset">The offset within the array to start reading from</param>
		/// <param name="count">The number of chars to read</param>
		public CharArrayReader(char[] array, int offset, int count)
		{
			Attach(array, offset, count);
		}

		private void Attach(char[] array, int offset, int count)
		{			
			endIndex = offset + count - 1;

			if (endIndex > array.Length - 1)
			{
				throw new ArgumentOutOfRangeException("offset");
			}

			this.offset = offset;
			this.array = array;
		}

		public override int Peek()
		{
			if (offset > endIndex)
			{
				return -1;
			}

			return array[offset];
		}

		public override int Read()
		{
			if (offset > endIndex)
			{
				return -1;
			}

			return array[offset++];
		}

		public override int Read(char[] buffer, int index, int count)
		{
			if (offset > endIndex)
			{
				return -1;
			}

			count = Math.Min(count, endIndex - offset + 1);

			Array.Copy(array, offset, buffer, index, count);

			offset += count;

			return count;
		}

		public override int ReadBlock(char[] buffer, int index, int count)
		{
			return Read(buffer, index, count);
		}
	}
}
