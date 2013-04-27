using System;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class FileSystemServerException
		: Exception
	{
		public virtual string ErrorCode { get; set; }

		public FileSystemServerException(string errorCode)
			: this(errorCode, "")
		{
		}

		public FileSystemServerException(string errorCode, string message)
			: base(message)
		{
			this.ErrorCode = errorCode;
		}
	}
}
