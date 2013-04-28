using System;

namespace Platform
{
	/// <summary>
	/// Provides math related static utility methods.
	/// </summary>
	public class MathUtils
	{
		/// <summary>
		/// Swaps the two given values
		/// </summary>
		/// <typeparam name="T">The type of the values to swap</typeparam>
		/// <param name="value1">The first value</param>
		/// <param name="value2">The second value</param>
		public static void Swap<T>(ref T value1, ref T value2)
		{
			T t3;

			t3 = value1;
			value1 = value2;
			value2 = t3;
		}

		/// <summary>
		/// Returns the midpoint of two values
		/// </summary>
		/// <param name="low">The low value</param>
		/// <param name="high">The high value</param>
		/// <returns>The mid point of the low and high value</returns>
		public static int MidPoint(int low, int high)
		{
			return low + ((high - low) / 2);
		}

		/// <summary>
		/// Returns a value if it is between a minimum or maximum value
		/// otherwise returns the minimum or maximum values respectively.
		/// </summary>
		/// <param name="value">The value to return</param>
		/// <param name="min">The minimum value to return</param>
		/// <param name="max">The maximum value to return</param>
		/// <returns>Either value, min or max</returns>
		public static int MinMax(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}

			if (value > max)
			{
				return max;
			}

			return value;
		}

		/// <summary>
		/// Returns the minimum of two values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <returns>The minimum of <c>a1</c> or <c>a2</c></returns>
		public static int Min(int a1, int a2)
		{
			return Math.Min(a1, a2);
		}

		/// <summary>
		/// Returns the minimum of three values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <param name="a3">The third value</param>
		/// <returns>The minimum of <c>a1</c> or <c>a2</c> or <c>a3</c></returns>
		public static int Min(int a1, int a2, int a3)
		{
			return Math.Min(Math.Min(a1, a2), a3);
		}

		/// <summary>
		/// Returns the minimum of four values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <param name="a3">The third value</param>
		/// <param name="a4">The fourth value</param>
		/// <returns>The minimum of <c>a1</c> or <c>a2</c> or <c>a3</c> or <c>a4</c></returns>
		public static int Min(int a1, int a2, int a3, int a4)
		{
			return Math.Min(Math.Min(Math.Min(a1, a2), a3), a4);
		}

		/// <summary>
		/// Returns the minimum of multiple values
		/// </summary>
		/// <param name="args">The values</param>
		/// <returns>The minimum of all the provided values</returns>
		public static int Min(params int[] args)
		{
			int retval;

			if (args.Length < 2)
			{
				throw new ArgumentException("Must provide at least two values");
			}

			retval = int.MaxValue;

			foreach (int value in args)
			{
				if (value < retval)
				{
					retval = value;	
				}
			}

			return retval;
		}

		/// <summary>
		/// Returns the maximum of two values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <returns>The maximum of <c>a1</c> or <c>a2</c></returns>
		public static int Max(int a1, int a2)
		{
			return Math.Max(a1, a2);
		}

		/// <summary>
		/// Returns the minimum of three values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <param name="a3">The third value</param>
		/// <returns>The minimum of <c>a1</c> or <c>a2</c> or <c>a3</c></returns>
		public static int Max(int a1, int a2, int a3)
		{
			return Math.Max(Math.Max(a1, a2), a3);
		}

		/// <summary>
		/// Returns the maximum of four values
		/// </summary>
		/// <param name="a1">The first value</param>
		/// <param name="a2">The second value</param>
		/// <param name="a3">The third value</param>
		/// /// <param name="a4">The fourth value</param>
		/// <returns>The minimum of <c>a1</c> or <c>a2</c> or <c>a3</c> or <c>a4</c></returns>
		public static int Max(int a1, int a2, int a3, int a4)
		{
			return Math.Max(Math.Max(Math.Max(a1, a2), a3), a4);
		}

		/// <summary>
		/// Returns the maximum of multiple values
		/// </summary>
		/// <param name="args">The values</param>
		/// <returns>The maximum of all the provided values</returns>
		public static int Max(params int[] args)
		{
			int retval;

			if (args.Length < 2)
			{
				throw new ArgumentException("Must provide at least two values");
			}

			retval = int.MinValue;

			foreach (int value in args)
			{
				if (value > retval)
				{
					retval = value;
				}
			}

			return retval;
		}	
	}
}
