namespace Platform.VirtualFileSystem
{
	public interface IFileHashingService
		: IHashingService
	{
		new IFile OperatingNode
		{
			get;
		}
	}
}
