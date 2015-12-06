using System;

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
