namespace Platform.VirtualFileSystem.Providers
{
	public class NodeToFileAttributes
		: NodeAttributesWrapper, IFileAttributes
	{
		[NodeAttribute]
		public virtual long? Length
		{
			get
			{
				return (long?)this[FileAttributes.Length];
			}
		}

		public NodeToFileAttributes(INodeAttributes attributes)
			: base(attributes)
		{
		}

		IFileAttributes IFileAttributes.Refresh()
		{
			return (IFileAttributes)Refresh();
		}
	}
}
