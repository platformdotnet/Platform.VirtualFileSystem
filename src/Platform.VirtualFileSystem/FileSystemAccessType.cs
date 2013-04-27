using System;

namespace Platform.VirtualFileSystem
{
	[Flags]
	public enum FileSystemAccessType
	{
		Allow,
		Deny
	}
}