using System;

namespace Platform.VirtualFileSystem
{
	[Flags]
	public enum FileComparingFlags
	{
		None = 0,
		CompareLength = 1,
		CompareCreationDate = 2,
		CompareLastWriteDate = 4,
		CompareLastAccessDate = 8,
		CompareContents = 16,
		CompareContentsExact = 32,
		CompareAllButContent = CompareLength | CompareCreationDate | CompareLastWriteDate | CompareLastAccessDate,
		CompareAll = CompareAllButContent | CompareContents,
		CompareAllExact = CompareContents | CompareContentsExact
	}
}