namespace Platform.VirtualFileSystem
{
	public interface ITempIdentityFileService
		: IService
	{
		IFile GetTempFile();
		IFile GetOriginalFile();
	}
}
