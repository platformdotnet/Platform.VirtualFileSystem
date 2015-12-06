namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractFileHashingService
		: AbstractHashingService, IFileHashingService
	{
		public new FileHashingServiceType ServiceType { get; private set; }

		public new IFile OperatingNode
		{
			get
			{
				return (IFile)base.OperatingNode;
			}
		}

		protected AbstractFileHashingService(IFile file, FileHashingServiceType serviceType)
			: base(file, serviceType)
		{
			this.ServiceType = serviceType;
		}
	}
}
