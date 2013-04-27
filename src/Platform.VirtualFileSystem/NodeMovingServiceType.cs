using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Summary description for NodeMovingServiceType.
	/// </summary>
	public class NodeMovingServiceType
		: NodeCopyingMovingServiceType
	{
		public NodeMovingServiceType(INode destination)
			: base(destination)
		{
		}

		public NodeMovingServiceType(INode destination, int bufferSize)
			: base(destination, bufferSize)
		{
		}

		public NodeMovingServiceType(INode destination, bool overwrite)
			: base(destination, overwrite)
		{
		}

		public NodeMovingServiceType(INode destination, bool overwrite, int bufferSize)
			: base(destination, overwrite, bufferSize)
		{
		}

		public NodeMovingServiceType(INode destination, bool overwrite, int bufferSize, int chunkSize)
			: base(destination, overwrite, bufferSize, chunkSize)
		{
		}
	}
}
