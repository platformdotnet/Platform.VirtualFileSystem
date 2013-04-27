using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Platform;
using Platform.IO;
using Platform.Text;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public interface IFileHashingService
		: IHashingService
	{
		new IFile OperatingNode
		{
			get;
		}
	}
}
