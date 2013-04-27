namespace Platform.VirtualFileSystem.Providers
{
	public class NodeBackedFileDeletingService
		: AbstractTask, INodeTaskService, INodeDeletingService
	{
		private readonly INode node;
		private readonly bool recursive;

		public NodeBackedFileDeletingService(INode node, bool recursive)
		{
			this.node = node;
			this.recursive = recursive;
			this.meter = new MutableMeter(0, 1, 0, "nodes");
		}

		public override void DoRun()
		{
			lock (this)
			{
				this.meter.SetCurrentValue(0);

				if (this.node.NodeType == NodeType.Directory && this.recursive)
				{
					((IDirectory)this.node).Delete(this.recursive);
				}
				else
				{
					this.node.Delete();
				}

				this.meter.SetCurrentValue(1);
			}
		}

		public override IMeter Progress
		{
			get
			{
				return this.meter;
			}
		}
		private readonly MutableMeter meter;

		public INode OperatingNode
		{
			get
			{
				return this.node;
			}
		}

		public object SyncLock
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
	}
}
