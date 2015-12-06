namespace Platform.VirtualFileSystem
{
	public interface IFileAttributes
		: INodeAttributes
	{
		long? Length
		{
			get;
		}

		new IFileAttributes Refresh();
	}
}
