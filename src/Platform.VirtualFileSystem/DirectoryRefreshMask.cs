using System;

namespace Platform.VirtualFileSystem
{
	[Flags]
	public enum DirectoryRefreshMask
	{
		None,
		Attributes = 1,
		Children = 2,
		AllChildren = 6,
		All = Attributes | Children | AllChildren
	}
}