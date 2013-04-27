namespace Platform.VirtualFileSystem
{
	public interface INodeServiceProvider
	{
		IService GetService(INode node, ServiceType serviceType);
	}
}