using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Providers.Local
{
    public class LocalFileSystemEventArgs
        : EventArgs
    {
	    public IDirectory Directory { get; set; }

	    public LocalFileSystemEventArgs(IDirectory directory)
        {
            this.Directory = directory;
        }
    }
}
