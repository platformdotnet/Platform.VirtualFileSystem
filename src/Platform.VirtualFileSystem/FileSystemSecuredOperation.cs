using System;

namespace Platform.VirtualFileSystem
{
	[Flags]
	public enum FileSystemSecuredOperation
	{
		None = 0,
		List = 1,
		View = 2,
		Read = 4,
		Write = 8
	}
}