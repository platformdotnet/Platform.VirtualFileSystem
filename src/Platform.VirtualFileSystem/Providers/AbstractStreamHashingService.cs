using System.IO;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractStreamHashingService
		: AbstractHashingService, IStreamHashingService
	{
		public virtual Stream OperatingStream
		{
			get
			{
				return this.ServiceType.Stream;
			}
		}

		public new StreamHashingServiceType ServiceType { get; private set; }

		public new IFile OperatingNode
		{
			get
			{
				return (IFile)base.OperatingNode;
			}
		}

		protected AbstractStreamHashingService(IFile file, StreamHashingServiceType serviceType)
			: base(file, serviceType)
		{			
			this.ServiceType = serviceType;
		}
	}
}
