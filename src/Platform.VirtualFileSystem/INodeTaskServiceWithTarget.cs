namespace Platform.VirtualFileSystem
{
	public interface INodeTaskServiceWithTarget
		: INodeTaskService
	{
		INode TargetNode
		{
			get;
		}		
	}
}
