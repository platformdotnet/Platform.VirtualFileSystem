namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractDirectoryHashingService
		: AbstractHashingService, IDirectoryHashingService
	{
		public new DirectoryHashingServiceType ServiceType { get; private set; }

		public new IDirectory OperatingNode
		{
			get
			{
				return (IDirectory)base.OperatingNode;
			}
		}

		protected AbstractDirectoryHashingService(IDirectory dir, DirectoryHashingServiceType serviceType)
			: base(dir, serviceType)
		{
			ServiceType = serviceType;
		}
	}
}
