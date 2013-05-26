using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipDirectoryInfo
		: ZipNodeInfo
	{
		public ZipDirectoryInfo(bool exists)
		{
			this.exists = exists;
		}
	}
}
