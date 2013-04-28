using System;

namespace Platform
{
	/// <summary>
	/// An enumeration of logical operators.
	/// </summary>
	[Flags]
	public enum LogicalOperation
	{
		None = 0,
		And = 1,		
		Or = 2,
		Any = 4,
		Nand = 8,
		Xor = 16,
		Nor = 32,
		All = And | Or | Any | Nand | Xor | Nor
	}
}
