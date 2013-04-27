using System;

namespace Platform.VirtualFileSystem.Network.Protocol
{
	/// <summary>
	/// Summary description for RemoteVirtualFileSystemException.
	/// </summary>
	public class RemoteVirtualFileSystemException
		: Exception
	{
		public RemoteVirtualFileSystemException(string errorCode)
			: this(errorCode, "")
		{
			
		}

		public RemoteVirtualFileSystemException(string errorCode, string message)
			: base(message.Length > 0 
				? String.Format("{0}: {1}", errorCode, message)
				: errorCode)
		{
		}
	}
}
