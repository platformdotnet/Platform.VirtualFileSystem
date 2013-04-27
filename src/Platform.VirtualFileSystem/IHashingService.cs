namespace Platform.VirtualFileSystem
{
	public interface IHashingService
		: INodeService
	{
		HashValue ComputeHash();
		HashValue ComputeHash(long offset, long length);
		HashValue ComputeHash(HashValue inputResult);
		HashValue ComputeHash(HashValue inputResult, long outputOffset, long outputLength);
	}
}
