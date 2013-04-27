using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Summary description for VolumeFaderNodeServiceProvider.
	/// </summary>
	public class VolumeFaderNodeServiceProvider
		: INodeServiceProvider
	{
		public virtual IService GetService(INode node, ServiceType serviceType)
		{
			VolumeFaderServiceType typedServiceType;

			typedServiceType = serviceType as VolumeFaderServiceType;

			if (typedServiceType == null)
			{
				return null;
			}

			return null;
		}
	}
}
