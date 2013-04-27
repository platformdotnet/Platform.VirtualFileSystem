namespace Platform.VirtualFileSystem
{
	public class FileNodeNotFoundException
		: NodeNotFoundException
	{
		public FileNodeNotFoundException()
			: this((INodeAddress)null)
		{
		}

		public FileNodeNotFoundException(INodeAddress nodeAddress)
			: base(nodeAddress, NodeType.Directory, null)
		{
		}

		public FileNodeNotFoundException(string uri)
			: base(uri)
		{
		}
	}
}