namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// </summary>
	public interface IRunnableNodeServiceWithTarget
		: IRunnableNodeService
	{
		INode TargetNode
		{
			get;
		}		
	}
}
