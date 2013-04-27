namespace Platform.VirtualFileSystem.Providers
{
	public class StandardNodeDecorator
		: INodeDecorator
	{
		public virtual INode Decorate(INode node)
		{
			return node;
		}
	}
}
