using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform
{
	/// <summary>
	/// A four byte structure that is layeed out in memory using big endian.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct BigEndianByteQuad
	{
		[FieldOffset(0)]
		public byte Byte1;

		[FieldOffset(1)]
		public byte Byte2;

		[FieldOffset(2)]
		public byte Byte3;

		[FieldOffset(3)]
		public byte Byte4;

		/// <summary>
		/// Converts the structure into an <see cref="int"/>
		/// </summary>
		public int ToInt32()
		{
			return (((((Byte1 << 8) + Byte2) << 8) + Byte3) << 8) + Byte4;
		}

		/// <summary>
		/// Converts the structure into a <see cref="uint"/>
		/// </summary>
		public uint ToUInt32()
		{
			return ((((((uint)Byte1 << 8) + (uint)Byte2) << 8) + (uint)Byte3) << 8) + (uint)Byte4;
		}

		public TimeSpan ToTimeSpan()
		{
			return TimeSpan.FromMilliseconds(1000 * (((double)ToInt32()) / 0x10000));
		}

		public TimeSpan ToTimeSpanAsUInt()
		{
			return TimeSpan.FromMilliseconds(1000 * (((double)ToUInt32()) / 0x10000));
		}

		public void SetFrom(TimeSpan timeSpan)
		{
			this = FromTimeSpan(timeSpan);
		}

		public static BigEndianByteQuad FromInt32(int value)
		{
			BigEndianByteQuad retval;

			retval = new BigEndianByteQuad();

			retval.Byte1 = (byte)((value >> 24) & 0xff);
			retval.Byte2 = (byte)((value >> 16) & 0xff);
			retval.Byte3 = (byte)((value >> 8) & 0xff);
			retval.Byte4 = (byte)((value >> 0) & 0xff);

			return retval;
		}

		public static implicit operator int(BigEndianByteQuad value)
		{
			return value.ToInt32();
		}

		public static implicit operator uint(BigEndianByteQuad value)
		{
			return value.ToUInt32();
		}

		public static implicit operator BigEndianByteQuad(int value)
		{
			return FromInt32(value);
		}

		public static implicit operator BigEndianByteQuad(uint value)
		{
			return FromUInt32(value);
		}

		public static BigEndianByteQuad FromUInt32(uint value)
		{
			BigEndianByteQuad retval;

			retval = new BigEndianByteQuad();

			retval.Byte1 = (byte)((value >> 24) & 0xff);
			retval.Byte2 = (byte)((value >> 16) & 0xff);
			retval.Byte3 = (byte)((value >> 8) & 0xff);
			retval.Byte4 = (byte)((value >> 0) & 0xff);

			return retval;
		}

		public static BigEndianByteQuad FromTimeSpan(TimeSpan timeSpan)
		{
			int x;
			BigEndianByteQuad retval;
			double milliseconds;

			retval = new BigEndianByteQuad();

			milliseconds = timeSpan.TotalMilliseconds;

			milliseconds /= 1000;

			milliseconds *= 0x10000;

			x = (int)milliseconds;

			retval.Byte4 = (byte)(x & 0xf);
			retval.Byte3 = (byte)((x >> 8) & 0xf);
			retval.Byte2 = (byte)((x >> 16) & 0xf);
			retval.Byte1 = (byte)((x >> 24) & 0xf);

			return retval;
		}
	}
}
