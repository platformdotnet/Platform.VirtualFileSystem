namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractService
		: IService
	{
		private readonly AutoLock autoLock;

		protected AbstractService()
		{
			this.autoLock = new AutoLock(this.SyncLock);
		}

		public string Name
		{
			get
			{
				return GetType().Name;
			}
		}

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		public virtual IAutoLock GetAutoLock()
		{
			return this.autoLock;
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}
	}
}
