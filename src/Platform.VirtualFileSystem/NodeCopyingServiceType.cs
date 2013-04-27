using System;

namespace Platform.VirtualFileSystem
{
	public class NodeCopyingServiceType
		: NodeCopyingMovingServiceType
	{
		public NodeCopyingServiceType(INode destination, bool overwrite)
			: base(destination, overwrite)
		{
		}

		public NodeCopyingServiceType(INode destination, bool overwrite, int bufferSize)
			: base(destination, overwrite, bufferSize)
		{
		}

		public NodeCopyingServiceType(INode destination, bool overwrite, int bufferSize, int chunkSize)
			: base(destination, overwrite, bufferSize, chunkSize)
		{
		}
	}
}
