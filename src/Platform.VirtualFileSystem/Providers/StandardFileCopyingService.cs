using System;
using System.IO;
using Platform.IO;

namespace Platform.VirtualFileSystem.Providers
{
	public class StandardFileCopyingService
		: StreamCopier, INodeCopyingService, INodeMovingService, INodeDeletingService, StreamCopier.IStreamProvider
	{
		public new static readonly int DefaultBufferSize = 32 * 1024;

		public virtual NodeCopyingServiceType ServiceType
		{
			get
			{
				return this.serviceType;
			}
		}
		private readonly NodeCopyingServiceType serviceType;

		private readonly IFile sourceFile;

		public StandardFileCopyingService(IFile sourceFile, NodeCopyingServiceType serviceType)
			: base(true, true, 128)
		{
			this.sourceFile = sourceFile;
			this.serviceType = serviceType;

			if (this.serviceType.Destination.Equals(sourceFile))
			{
				throw new ArgumentException(String.Format("Source and destination must not be the same: {0}", this.ServiceType.Destination.Address.Uri));
			}

			InitializePump();
		}

		Stream StreamCopier.IStreamProvider.GetSourceStream()
		{
			if (ServiceType.BufferSize == 0)
			{
				return this.sourceFile.GetContent().GetInputStream();
			}
			else
			{
				return new BufferedStream(this.sourceFile.GetContent().GetInputStream(), this.ServiceType.BufferSize);
			}
		}

		Stream StreamCopier.IStreamProvider.GetDestinationStream()
		{
			if (this.ServiceType.BufferSize == 0)
			{
				return this.ServiceType.Destination.GetContent().GetOutputStream();
			}
			else
			{
				return new BufferedStream(this.ServiceType.Destination.GetContent().GetOutputStream(), this.ServiceType.BufferSize);
			}
		}

		long StreamCopier.IStreamProvider.GetSourceLength()
		{
			return this.sourceFile.Length ?? 0;
		}

		long StreamCopier.IStreamProvider.GetDestinationLength()
		{
			return ((IFile)this.ServiceType.Destination).Length ?? 0;
		}
		
		public virtual INode OperatingNode
		{
			get
			{
				return this.sourceFile;
			}
		}

		public virtual INode TargetNode
		{
			get
			{
				return this.ServiceType.Destination;
			}
		}

		protected override void SetTaskState(TaskState newState)
		{
			if (newState == TaskState.Finished)
			{
				return;
			}

			base.SetTaskState(newState);
		}

		public override void Run()
		{
			// The follow is not concurrent safe

			this.ServiceType.Destination.Refresh();
			this.sourceFile.Refresh();

			if (!this.ServiceType.Overwrite && this.ServiceType.Destination.Exists)
			{
				throw new IOException(String.Format("The file already exists: {0}", this.ServiceType.Destination.Address));
			}

			base.Run();
		}

		#region ISyncLocked Members

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		public virtual IAutoLock GetAutoLock()
		{
			return new AutoLock(this);
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}

		#endregion
	}
}
