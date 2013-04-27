using System;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public class CoreNodeServicesProvider
		: INodeServiceProvider
	{
		public CoreNodeServicesProvider(IFileSystem fileSystem)
		{
		}

		public virtual IService GetService(INode node, ServiceType serviceType)
		{
			if (node is IFile)
			{
				if (serviceType is NodeCopyingMovingServiceType)
				{
					if (serviceType is NodeCopyingServiceType)
					{
						return new StandardFileCopyingService
							(
								(IFile)node,
								(NodeCopyingServiceType)serviceType
							);
					}
					else if (serviceType is NodeMovingServiceType)
					{
						return new StandardFileMovingService
							(
								(IFile)node,
								(NodeMovingServiceType)serviceType
							);
					}
					else
					{
						throw new NotSupportedException(serviceType.GetType().Name);
					}
				}
				else if (serviceType is StreamHashingServiceType)
				{
					return new StandardStreamHashingService((IFile)node, (StreamHashingServiceType)serviceType);
				}
				else if (serviceType is NodeDeletingServiceType)
				{
					return new NodeBackedFileDeletingService((IFile)node, false);
				}
				else if (serviceType is FileHashingServiceType)
				{
					return new StandardFileHashingService((IFile)node, (FileHashingServiceType)serviceType);
				}
				else if (serviceType is FileTransferServiceType)
				{
					return new StandardFileTransferService((IFile)node, (FileTransferServiceType)serviceType);
				}
				else if (serviceType is FileComparingServiceType)
				{
					return new StandardFileComparingService((IFile)node, (FileComparingServiceType)serviceType);
				}
				else if (serviceType is TempIdentityFileServiceType)
				{
					return new StandardTempIdentityFileService((IFile)node, (TempIdentityFileServiceType)serviceType);
				}
			}
			else if (node is IDirectory)
			{				
				if (serviceType is DirectoryHashingServiceType)
				{
					return new StandardDirectoryHashingService((IDirectory)node, (DirectoryHashingServiceType)serviceType);
				}
				else if (serviceType is NodeDeletingServiceType)
				{
					return new NodeBackedFileDeletingService(node, ((NodeDeletingServiceType)serviceType).Recursive);
				}
			}

			return null;
		}
	}
}
