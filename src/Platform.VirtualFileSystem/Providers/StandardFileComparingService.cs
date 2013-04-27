namespace Platform.VirtualFileSystem.Providers
{
	public class StandardFileComparingService
		: AbstractRunnableService, IFileComparingService
	{
		private readonly IFile file;
		private readonly bool refresh;
		private readonly FileComparingServiceType serviceType;

		public StandardFileComparingService(IFile file, FileComparingServiceType serviceType, bool refresh = true)
		{
			this.file = file;
			this.refresh = refresh;
			this.serviceType = serviceType;
		}

		public virtual INode OperatingNode
		{
			get
			{
				return this.file;
			}
		}

		public virtual INode TargetNode
		{
			get
			{
				return this.serviceType.TargetFile;
			}
		}

		public virtual object Value
		{
			get
			{
				return this.value;
			}
		}
		private bool value;

		public virtual bool Compare()
		{
			lock (this)
			{
				Run();

				return this.value;
			}
		}

		public override void DoRun()
		{
			lock (this)
			{
				if (refresh)
				{
					this.OperatingNode.Refresh();
					this.TargetNode.Refresh();
				}

				if ((this.serviceType.Flags & FileComparingFlags.CompareLength) != FileComparingFlags.None)
				{
					if (((IFile)this.OperatingNode).Attributes.Length != ((IFile)this.TargetNode).Attributes.Length)
					{
						this.value = false;

						return;
					}
				}
				
				if ((this.serviceType.Flags & FileComparingFlags.CompareCreationDate) != FileComparingFlags.None)
				{
					if (((IFile)this.OperatingNode).Attributes.CreationTime != ((IFile)this.TargetNode).Attributes.CreationTime)
					{
						this.value = false;

						return;
					}
				}
				
				if ((this.serviceType.Flags & FileComparingFlags.CompareLastAccessDate) != FileComparingFlags.None)
				{
					if (((IFile)this.OperatingNode).Attributes.LastAccessTime != ((IFile)this.TargetNode).Attributes.LastAccessTime)
					{
						this.value = false;

						return;
					}
				}
				
				if ((this.serviceType.Flags & FileComparingFlags.CompareLastWriteDate) != FileComparingFlags.None)
				{
					if (((IFile)this.OperatingNode).Attributes.LastWriteTime != ((IFile)this.TargetNode).Attributes.LastWriteTime)
					{
						this.value = false;

						return;
					}
				}
				
				if (((this.serviceType.Flags & FileComparingFlags.CompareContents) != FileComparingFlags.None)
					|| ((this.serviceType.Flags & FileComparingFlags.CompareAllExact) != FileComparingFlags.None))
				{
					var fileHasher = (IFileHashingService)this.OperatingNode.GetService(new FileHashingServiceType("md5"));
					var targetHasher = (IFileHashingService)this.TargetNode.GetService(new FileHashingServiceType("md5"));

					if (!fileHasher.ComputeHash().Value.ElementsAreEqual(targetHasher.ComputeHash().Value))
					{
						this.value = false;

						return;
					}
				}
			
				this.value = true;
			}
		}
	}
}
