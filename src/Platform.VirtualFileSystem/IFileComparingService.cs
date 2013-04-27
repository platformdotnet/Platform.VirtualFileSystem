namespace Platform.VirtualFileSystem
{
	public interface IFileComparingService
		: INodeTaskServiceWithTarget, IValued
	{
		bool Compare();
	}
}
