using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	internal class NetworkFileHashingService
		: AbstractFileHashingService
	{
		public NetworkFileHashingService(IFile file, FileHashingServiceType serviceType)
			: base(file, serviceType)
		{
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			try
			{
				return NetworkNode.ComputeHash(this.OperatingNode, this.ServiceType.AlgorithmName, false, offset, length, null, null);
			}
			catch (NodeNotFoundException)
			{
				NetworkNodeAndFileAttributes attributes;

				attributes = this.OperatingNode.Attributes as NetworkNodeAndFileAttributes;

				if (attributes != null)
				{
					attributes.SetValue<bool>("exists", false);
				}
				
				throw;
			}
		}
	}
}
