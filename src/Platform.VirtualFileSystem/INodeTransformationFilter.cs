namespace Platform.VirtualFileSystem
{
	public interface INodeTransformationFilter
	{
		INode Filter(INode node);
	}
}
