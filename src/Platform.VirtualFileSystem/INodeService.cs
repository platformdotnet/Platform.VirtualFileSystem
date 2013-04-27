namespace Platform.VirtualFileSystem
{
	public interface INodeService
		: IService
	{
		INode OperatingNode
		{
			get;
		}
	}
}
