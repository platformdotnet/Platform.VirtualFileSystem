namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// </summary>
	public interface ILockingService
		: IService
	{
		void Lock();
		void Lock(int timeout);		
		void Unlock();
	}
}
