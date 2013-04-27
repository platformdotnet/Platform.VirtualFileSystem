using System;

namespace Platform.VirtualFileSystem
{
	public class NodeTypeNotSupportedException
		: NotSupportedException
	{
		public NodeTypeNotSupportedException(NodeType nodeType)
			: base("Node type not supported: " + nodeType)
		{
		}
	}
}
