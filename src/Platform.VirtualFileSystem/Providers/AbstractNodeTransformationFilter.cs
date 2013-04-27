namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractNodeTransformationFilter
		: INodeTransformationFilter
	{
		public abstract INode Filter(INode node);
	}
}
