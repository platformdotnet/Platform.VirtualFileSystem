namespace Platform.VirtualFileSystem
{
	public class DirectoryNodeNotFoundException
		: NodeNotFoundException
	{
		public DirectoryNodeNotFoundException()
			: this((INodeAddress)null)
		{
		}

		public DirectoryNodeNotFoundException(INodeAddress nodeAddress)
			: base(nodeAddress, NodeType.Directory, null)
		{
		}

		public DirectoryNodeNotFoundException(string uri)
			: base(uri)
		{
		}
	}
}