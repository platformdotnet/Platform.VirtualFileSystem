using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Multimedia
{
	public struct VolumeLevel
	{
		public static readonly VolumeLevel Zero = new VolumeLevel(0, 0);

		public int Left
		{
			get
			{
				return m_Value.Left;
			}
			set
			{
				m_Value.Left = value;
			}
		}

		public int Right
		{
			get
			{
				return m_Value.Right;
			}
			set
			{
				m_Value.Right = value;
			}
		}

		public Pair<int, int> AsPair()
		{
			return m_Value;
		}

		public Pair<int, int> AsPairAdjusted(int maximum)
		{
			return new Pair<int, int>((int)(Left * ((double)maximum / Int32.MaxValue)), (int)(Right * ((double)maximum / Int32.MaxValue)));
		}

		public static VolumeLevel FromPairAdjusted(Pair<int, int> value, int maximum)
		{
			return new VolumeLevel((int)(value.Left * (Int32.MaxValue / (double)maximum)), (int)(value.Right * (Int32.MaxValue / (double)maximum)));
		}

		public static implicit operator Pair<int, int>(VolumeLevel value)
		{
			return value.AsPair();
		}

		public static implicit operator VolumeLevel(Pair<int, int> value)
		{
			return new VolumeLevel(value);
		}

		private Pair<int, int> m_Value;

		public VolumeLevel(float percent)
			: this(percent, percent)
		{
		}

		public VolumeLevel(float leftPercent, float rightPercent)
			: this((int)(leftPercent * Int32.MaxValue), (int)(rightPercent * Int32.MaxValue))
		{
		}

		public VolumeLevel(int value)
			: this(value, value)
		{
		}

		public VolumeLevel(Pair<int, int> value)
			: this(value.Left, value.Right)
		{		
		}

		public VolumeLevel(int left, int right)
		{
			if (left < 0 || left > Int32.MaxValue)
			{
				throw new ArgumentOutOfRangeException("left", left, "Must be between 0 and 100 inclusive");
			}

			if (right < 0 || right > Int32.MaxValue)
			{
				throw new ArgumentOutOfRangeException("right", right, "Must be between 0 and 100 inclusive");
			}

			m_Value = new Pair<int, int>(left, right);
		}

		public int Average
		{
			get
			{
				return (this.Left/2) + (this.Right/2);
			}
		}

		public override string ToString()
		{
			return m_Value.ToString();
		}
	}
}
