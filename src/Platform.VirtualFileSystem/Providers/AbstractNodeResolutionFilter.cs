namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractNodeResolutionFilter
		: INodeResolutionFilter
	{
		public abstract INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache);
	}
}
