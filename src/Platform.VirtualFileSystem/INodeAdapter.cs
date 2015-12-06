namespace Platform.VirtualFileSystem
{
	public interface INodeDecorator
	{
		INode Decorate(INode node);
	}
}
