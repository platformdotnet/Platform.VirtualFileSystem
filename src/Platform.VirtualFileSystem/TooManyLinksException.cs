using System.IO;

namespace Platform.VirtualFileSystem
{
	public class TooManyLinksException
		: IOException
	{
		public TooManyLinksException()
		{
		}

		public TooManyLinksException(IFile linkfile, IFile target)			
		{
		}
	}
}
