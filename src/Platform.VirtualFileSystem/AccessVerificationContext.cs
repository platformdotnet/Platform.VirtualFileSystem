namespace Platform.VirtualFileSystem
{
	public struct AccessVerificationContext
	{
		public INode Node { get; set; }
		public FileSystemSecuredOperation Operation { get; set; }

		public AccessVerificationContext(INode node, FileSystemSecuredOperation operation)
			: this()
		{
			this.Node = node;
			this.Operation = operation;
		}	
	}
}