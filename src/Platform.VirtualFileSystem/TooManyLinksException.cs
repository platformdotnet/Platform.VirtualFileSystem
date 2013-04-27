using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
