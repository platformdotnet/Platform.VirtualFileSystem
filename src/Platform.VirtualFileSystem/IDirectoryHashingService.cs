namespace Platform.VirtualFileSystem
{
	public interface IDirectoryHashingService
		: IHashingService
	{
		new IDirectory OperatingNode
		{
			get;
		}
	}
}