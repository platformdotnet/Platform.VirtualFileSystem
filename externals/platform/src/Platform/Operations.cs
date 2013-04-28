using System;
using System.Collections.Generic;

namespace Platform
{
	public static class Operations
	{
		public static sbyte Add(sbyte x, sbyte y)
		{
			return (sbyte)(x + y);
		}

		public static byte Add(byte x, byte y)
		{
			return (byte)(x + y);
		}

		public static ushort Add(ushort x, ushort y)
		{
			return (ushort)(x + y);
		}

		public static short Add(short x, short y)
		{
			return (short)(x + y);
		}

		public static int Add(int x, int y)
		{
			return x + y;
		}

		public static uint Add(uint x, uint y)
		{
			return (uint)(x + y);
		}

		public static float Add(float x, float y)
		{
			return x + y;
		}

		public static double Add(double x, double y)
		{
			return x + y;
		}

		public static decimal Add(decimal x, decimal y)
		{
			return x + y;
		}

		public static long Add(long x, long y)
		{
			return x + y;
		}

		public static ulong Add(ulong x, ulong y)
		{
			return x + y;
		}

		public static IEnumerable<T> Add<T>(IEnumerable<T> x, IEnumerable<T> y)
		{
			foreach (var xe in x)
			{
				yield return xe;
			}

			foreach (var ye in y)
			{
				yield return ye;
			}
		}

		public static string Add(string x, string y)
		{
			return x + y;
		}
	}
}
