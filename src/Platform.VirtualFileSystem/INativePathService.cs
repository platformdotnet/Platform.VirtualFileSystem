using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for a service that provides a native path for a node.
	/// </summary>
	/// <remarks>
	/// Often, for interop reasons, the operating-system/native path of a file
	/// or directory is required.  Virtual File Systems nodes that represent a
	/// node that is addressable at the operating-system level should implement
	/// this service.
	/// </remarks>
	public interface INativePathService
		: IService
	{
		string GetNativePath();
		string GetNativeShortPath();
	}
}
