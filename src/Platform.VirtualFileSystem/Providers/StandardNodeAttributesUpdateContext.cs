namespace Platform.VirtualFileSystem.Providers
{
	public class StandardNodeAttributesUpdateContext
		: INodeAttributesUpdateContext
	{
		private readonly IAutoLock syncLock;
		private INodeAttributes nodeAttributes;

		public StandardNodeAttributesUpdateContext(INodeAttributes nodeAttributes)
		{
			this.nodeAttributes = nodeAttributes;

			this.syncLock = nodeAttributes.GetAutoLock().Lock();
		}

		public virtual void Dispose()
		{
			this.syncLock.Unlock();
		}
	}
}
