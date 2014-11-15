namespace Platform.VirtualFileSystem.Providers.View
{
	public class ViewDirectory
		: DirectoryDelegationWrapper
	{
		public override INodeAddress Address
		{
			get
			{
				return this.address;
			}
		}
		private readonly ViewNodeAddress address;

		public override IFileSystem FileSystem
		{
			get
			{
				return this.fileSystem;
			}
		}
		private readonly IFileSystem fileSystem;

		public override IDirectory ParentDirectory
		{
			get
			{
				if (this.parentDirectory == null)
				{
					lock (this.SyncLock)
					{
						this.parentDirectory = FuncUtils.VolatileAssign(() => this.ResolveDirectory(".."));
					}
				}

				return this.parentDirectory;
			}
		}
		private IDirectory parentDirectory;

		public ViewDirectory(ViewFileSystem fileSystem, ViewNodeAddress address, IDirectory wrappee)
			: base(wrappee)
		{
			this.address = address;
			this.fileSystem = fileSystem;
						
			this.NodeResolver = new ViewResolver(fileSystem, this.Address);
			this.NodeAdapter = fileSystem.ViewNodeAdapter;
		}

		public override string ToString()
		{
			return this.Address.Uri;
		}
	}
}
