using System;
using System.IO;

namespace Platform.IO
{
	public class Buffer
	{
		
	}

	public abstract class ShortBuffer
	{
	}

	public abstract class BigEndianShortBuffer
		: ShortBuffer
	{
	}

	public abstract class LittleEndianShortBuffer
		: ShortBuffer
	{
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IInt16ByteArrayAdapter
	{
		/// <summary>
		/// 
		/// </summary>
		int Length
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		Int16 this[int x]
		{
			get;
			set;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Int16LittleEndianByteArrayAdapter
		: IInt16ByteArrayAdapter
	{
		private byte[] bytes;
		
		public int Length
		{
			get
			{
				return bytes.Length / 2;
			}
		}

		public Int16LittleEndianByteArrayAdapter(byte[] bytes)
		{
			this.bytes = bytes;
		}

		public Int16 this[int index]
		{
			get
			{			
				// Translate Int16 based index to Byte based index.

				index <<= 2;

				// Get and return Int16 value.

                return (short)(bytes[index] | (bytes[index + 1] << 8));
			}

			set
			{
				// Translate Int16 based index to Byte based index.

				index <<= 2;

				// Get and set Int16 value.

				bytes[index] = (byte)(value & 0xf);
				bytes[index + 1] = (byte)((value & 0xf0) >> 8);
			}
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class Int16BIgEndianByteArrayAdapter
		: IInt16ByteArrayAdapter
	{
		private byte[] bytes;
		
		public int Length
		{
			get
			{
				return bytes.Length / 2;
			}
		}

		public Int16BIgEndianByteArrayAdapter(byte[] bytes)
		{
			this.bytes = bytes;
		}

		/// <summary>
		/// 
		/// </summary>
		public Int16 this[int index]
		{
			get
			{
				// Translate Int16 based index to Byte based index.

				index <<= 2;

				// Get and return Int16 value.

				return (short)((bytes[index] << 8) | (bytes[index + 1]));
			}

			set
			{
				// Translate Int16 based index to Byte based index.

				index <<= 2;

				// Get and set Int16 value.
				
				bytes[index] = (byte)((value & 0xf0) >> 8);
				bytes[index + 1] = (byte)(value & 0xf);
			}
		}
	}
}