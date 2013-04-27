using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	internal class NetworkDirectoryHashingService
		: AbstractDirectoryHashingService
	{
		public NetworkDirectoryHashingService(IDirectory directory, DirectoryHashingServiceType serviceType)
			: base(directory, serviceType)
		{
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			return NetworkNode.ComputeHash(this.OperatingNode, this.ServiceType.AlgorithmName ?? "null", this.ServiceType.Recursive, offset, length, this.ServiceType.IncludedFileAttributes, this.ServiceType.IncludedDirectoryAttributes);
		}
	}
}
