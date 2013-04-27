namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractRunnableService
		: AbstractTask, ITaskService
	{
		private AutoLock autoLock;

		protected AbstractRunnableService()
		{
			this.autoLock = new AutoLock(this.SyncLock);
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
