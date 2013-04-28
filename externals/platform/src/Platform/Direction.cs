using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An enumeration of directions.  North is equivalent to Top
	/// and Left is equivalent to West.
	/// </summary>
	[Flags]
	public enum Direction
	{
		None = 0,
		North = 1,
		South = 2,
		West = 4,
		East = 8,
		Top = 1,
		Bottom = 2,
		Left = 4,
		Right = 8
	}
}
