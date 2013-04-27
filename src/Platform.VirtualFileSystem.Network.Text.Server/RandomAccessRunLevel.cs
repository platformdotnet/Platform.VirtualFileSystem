using System;
using System.IO;
using Platform.Network.ExtensibleServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class RandomAccessRunLevel
		: RunLevel, IDisposable
	{
		public const string NAME = "RANDOMACCESS";

		public virtual Stream Stream { get; set; }
		public virtual IFile FileNode { get; set; }


		public RandomAccessRunLevel(IFile file, Stream stream)
		{
			this.Stream = stream;
			this.FileNode = file;
		}

		public virtual void Dispose()
		{
			ActionUtils.IgnoreExceptions(() => this.Stream.Close());
		}
	}
}
