using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Summary description for MultimediaException.
	/// </summary>
	public class MultimediaException
		: Exception
	{
		public MultimediaException(string message)
			: base(message)
		{
		}

		public MultimediaException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
