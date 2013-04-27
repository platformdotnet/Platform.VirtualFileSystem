namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebFileSystem
		: AbstractFileSystem
	{
		internal protected WebFileSystem(INodeAddress rootAddress, FileSystemOptions options)
			: base(rootAddress, null, options)
		{			
		}

		protected override INode CreateNode(INodeAddress name, NodeType nodeType)
		{
			if (nodeType == NodeType.Directory)
			{
				return new WebDirectory(this, name);
			}
			else if (nodeType == NodeType.File || nodeType == NodeType.Any)
			{
				return new WebFile(this, name);
			}
			else
			{
				throw new NodeTypeNotSupportedException(nodeType);
			}
		}
	}
}