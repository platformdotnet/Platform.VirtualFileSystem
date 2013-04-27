using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	internal class NetworkStreamHashingService
		: AbstractStreamHashingService
	{
		public NetworkStreamHashingService(IFile file, StreamHashingServiceType serviceType)
			: base(file, serviceType)
		{
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			NetworkFileSystem fileSystem;
			NetworkFileSystem.FreeClientContext clientContext;

			fileSystem = (NetworkFileSystem)this.OperatingNode.FileSystem;

			using (clientContext = fileSystem.GetFreeClientContext())
			{
				NetworkNodeAddress address;

				address = (NetworkNodeAddress)OperatingNode.Address;

				return clientContext.NetworkFileSystemClient.ComputeHash(ServiceType.Stream, ServiceType.AlgorithmName, offset, length);
			}
		}
	}
}
