using System;

namespace Platform.VirtualFileSystem.Providers.Overlayed
{
	internal class OverlayedFile
		: FileWrapper
	{
		private readonly INodeAddress nodeAddress;
		private readonly OverlayedFileSystem fileSystem;

		public override IFileSystem FileSystem
		{
			get
			{
				return fileSystem;
			}
		}

		public OverlayedFile(OverlayedFileSystem fileSystem, INodeAddress nodeAddress, IFile file)
			: base(file)
		{
			this.fileSystem = fileSystem;
			this.nodeAddress = nodeAddress;
		}

		public override INodeAddress Address
		{
			get
			{
				return nodeAddress;
			}
		}

		public override INode Refresh()
		{
			SetWrappee(fileSystem.RefreshNode(this.Wrappee));

			base.Refresh();

			return this;
		}

		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			var address = this.Address.ResolveAddress(name, scope);

			return this.FileSystem.Resolve(address, nodeType);
		}

		public override System.Collections.Generic.IEnumerable<INode> GetAlternates()
		{
			return ((OverlayedFileSystem)this.FileSystem).GetAlternates(this);
		}

		internal INodeContent GetBaseContent()
		{
			return base.GetContent();
		}

		public override INodeContent GetContent()
		{
			if (this.content == null)
			{
				lock (this.SyncLock)
				{
					this.content = FunctionUtils.VolatileAssign(() => new OverlayedNodeContent(this));
				}
			}

			return this.content;
		}
		private OverlayedNodeContent content;

		public override INode Create(bool createParent)
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation((OverlayedFileSystem)this.FileSystem, FileSystemActivity.Created, this.Address, this.NodeType, out nodes))
			{
				var count = 0;

				foreach (var node in nodes)
				{
					try
					{
						node.Create(createParent);
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.Create(createParent);
			}

			return this;
		}

		public override INode Delete()
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation((OverlayedFileSystem)this.FileSystem, FileSystemActivity.Deleted, this.Address, this.NodeType, out nodes))
			{
				var count = 0;

				foreach (var node in nodes)
				{
					try
					{
						node.Delete();
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.Delete();
			}

			return this;
		}

		public override INode RenameTo(string name, bool overwrite)
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation(
					(OverlayedFileSystem)this.FileSystem, FileSystemActivity.Renamed, this.Address, this.NodeType, out nodes))
			{
				var count = 0;

				foreach (var node in nodes)
				{
					try
					{
						node.RenameTo(name, overwrite);
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.RenameTo(name, overwrite);
			}

			return this;
		}
	}
}
