using System;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public abstract class NodeCopyingMovingServiceType
		: ServiceType
	{
		public virtual int ChunkSize { get; set; }
		public virtual INode Destination { get; set; }
		public virtual bool Overwrite { get; set; }
		public virtual int BufferSize { get; set; }

		protected NodeCopyingMovingServiceType(INode destination)
			: this(destination, false)
		{
			
		}

		protected NodeCopyingMovingServiceType(INode destination, int bufferSize)
			: this(destination, false, bufferSize)
		{
		}

		protected NodeCopyingMovingServiceType(INode destination, bool overwrite)
			: this(destination, overwrite, 128 * 1024)
		{
		}

		protected NodeCopyingMovingServiceType(INode destination, bool overwrite, int bufferSize)
			: this(destination, overwrite, bufferSize, 128 * 1024)
		{
		}

		protected NodeCopyingMovingServiceType(INode destination, bool overwrite, int bufferSize, int chunkSize)
		{
			this.Destination = destination;
			this.Overwrite = overwrite;
			this.BufferSize = bufferSize;
			this.ChunkSize = chunkSize;
		}
	}
}
