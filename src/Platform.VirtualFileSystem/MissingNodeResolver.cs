using System;

namespace Platform.VirtualFileSystem
{
	public delegate INode MissingNodeResolver(IFileSystem fileSystem, INodeAddress name, NodeType nodeType);
}
