namespace Platform.VirtualFileSystem.Multimedia
{
	public class VolumeFaderNodeServiceProvider
		: INodeServiceProvider
	{
		public virtual IService GetService(INode node, ServiceType serviceType)
		{
			var typedServiceType = serviceType as VolumeFaderServiceType;

			if (typedServiceType == null)
			{
				return null;
			}

			return null;
		}
	}
}
