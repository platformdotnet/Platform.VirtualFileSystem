using System;
using System.Collections.Generic;
using System.Threading;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkNodeAndFileAttributes
		: DictionaryBasedNodeAttributes
	{
		private readonly INode node;

		public NetworkNodeAndFileAttributes(INode node)
		{
			this.node = node;

			this.updateContextCount = 0;
		}

		public override void OnAttributeValueChanged(DictionaryBasedNodeAttributesEventArgs eventArgs)
		{
			if (eventArgs.UserSet && this.updateContextCount == 0)
			{
				Update();
			}

			base.OnAttributeValueChanged(eventArgs);			
		}

		private sealed class NetworkNodeAttributesUpdateContext
			: INodeAttributesUpdateContext
		{
			private readonly NetworkNodeAndFileAttributes nodeAndFileAttributes;

			public NetworkNodeAttributesUpdateContext(NetworkNodeAndFileAttributes nodeAndFileAttributes)
			{
				this.nodeAndFileAttributes = nodeAndFileAttributes;

				Monitor.Enter(this.nodeAndFileAttributes.SyncLock);

				this.nodeAndFileAttributes.updateContextCount++;
			}

			private bool alreadyDisposed = false;

			public void Dispose()
			{
				lock (this)
				{
					if (this.alreadyDisposed)
					{
						return;
					}

					this.alreadyDisposed = true;
				}

				Monitor.Exit(this.nodeAndFileAttributes.SyncLock);

				this.alreadyDisposed = true;

				this.nodeAndFileAttributes.Update();
				this.nodeAndFileAttributes.updateContextCount--;					
			}
		}

		[ThreadStatic]
		private int updateContextCount = 0;

		public override INodeAttributesUpdateContext AquireUpdateContext()
		{
			return new NetworkNodeAttributesUpdateContext(this);
		}

		private IEnumerable<Pair<string, object>> GetAttributesPairs()
		{
			foreach (var attribute in this.node.Attributes)
			{
				if (!attribute.Key.Equals("exists")
					&& !attribute.Key.Equals("length"))
				{
					yield return new Pair<string, object>(attribute.Key, attribute.Value);
				}
			}
		}

		private bool refresh = false;

		public override INodeAttributes Refresh()
		{
			lock (this.SyncLock)
			{
				this.refresh = true;
			}
						
			return this;
		}

		public override T GetValue<T>(string attributeName)
		{
			lock (this.SyncLock)
			{
				if (this.refresh)
				{
					this.refresh = false;

					lock (this.SyncLock)
					{
						var networkFileSystem = (NetworkFileSystem)this.node.FileSystem;

						this.Clear();

						try
						{
							using (var clientContext = networkFileSystem.GetFreeClientContext())
							{
								foreach (var attribute in clientContext.NetworkFileSystemClient.GetAttributes(((NetworkNodeAddress)this.node.Address).RemoteUri, this.node.NodeType))
								{
									SetValue(attribute.Name, attribute.Value, false);
								}
							}
						}
						catch (NodeNotFoundException)
						{
                            // Shouldn't actually get here

							SetValue<bool>("exists", false);
						}
					}
				}
			}

			return base.GetValue<T>(attributeName);
		}

		public virtual void Update()
		{
			lock (this.SyncLock)
			{
				var networkFileSystem = (NetworkFileSystem)this.node.FileSystem;

				using (var clientContext = networkFileSystem.GetFreeClientContext())
				{
					clientContext.NetworkFileSystemClient.SetAttributes
					(
						((NetworkNodeAddress)this.node.Address).RemoteUri,
						this.node.NodeType,
						GetAttributesPairs()
					);
				}

				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)this.node.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Changed, this.node.NodeType, this.node.Name, this.node.Address.AbsolutePath));
			}
		}
	}
}
