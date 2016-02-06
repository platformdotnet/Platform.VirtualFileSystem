using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.VirtualFileSystem.Providers.Overlayed
{
	public delegate INode OverlayedNodeSelectorFunction(OverlayedFileSystem fileSystem, INodeAddress address, NodeType nodeType);

	#region DelegateBasedOverlayedNodeSelector

	public class DelegateBasedOverlayedNodeSelector
		: OverlayedNodeSelector
	{
		private readonly OverlayedNodeSelectorFunction getReadNodeFunction;
		private readonly OverlayedNodeSelectorFunction getWriteNodeFunction;
		
		public DelegateBasedOverlayedNodeSelector(OverlayedNodeSelectorFunction getReadNodeFunction, OverlayedNodeSelectorFunction getWriteNodeFunction)
		{
			this.getReadNodeFunction = getReadNodeFunction;
			this.getWriteNodeFunction = getWriteNodeFunction;
		}

		public override INode SelectWriteNode(OverlayedFileSystem fileSystem, INodeAddress address, NodeType nodeType)
		{
			return getWriteNodeFunction(fileSystem, address, nodeType);
		}

		public override INode SelectReadNode(OverlayedFileSystem fileSystem, INodeAddress address, NodeType nodeType)
		{
			return getReadNodeFunction(fileSystem, address, nodeType);
		}
	}

	public abstract class OverlayedNodeSelector
	{
		public virtual INode SelectWriteNode(OverlayedFileSystem fileSystem, INodeAddress address, NodeType nodeType)
		{
			return SelectReadNode(fileSystem, address, nodeType);
		}

		public abstract INode SelectReadNode(OverlayedFileSystem fileSystem, INodeAddress address, NodeType nodeType);

		public virtual bool SelectNodeForOperation(OverlayedFileSystem fileSystem, FileSystemActivity operation, INodeAddress address, NodeType nodeType, out INode[] nodes)
		{
			nodes = null;

			return false;
		}
	}

	#endregion

	public class OverlayedFileSystemEventArgs
		: EventArgs
	{
	}

	public class OverlayedFileSystem
		: AbstractFileSystem
	{
		private readonly IList<IFileSystem> fileSystems;
		
		public override bool SupportsActivityEvents => true;

		public override event FileSystemActivityEventHandler Activity
		{
			add
			{
				lock (activityLock)
				{
					if (activityEvents == null)
					{
						AttachActivityHandlers();
					}
					
					activityEvents = (FileSystemActivityEventHandler)Delegate.Combine(activityEvents, value);
				}
			}
			remove
			{
				lock (activityLock)
				{
					activityEvents = (FileSystemActivityEventHandler)Delegate.Remove(activityEvents, value);

					if (activityEvents == null)
					{
						RemoveActivityHandlers();
					}					
				}
			}
		}
		private readonly object activityLock = new object();
		private FileSystemActivityEventHandler activityEvents;

		private void AttachActivityHandlers()
		{
			foreach (var fileSystem in this.FileSystems)
			{
				fileSystem.Activity += FileSystemsActivity;
			}
		}

		private void RemoveActivityHandlers()
		{
			foreach (var fileSystem in this.FileSystems)
			{
				fileSystem.Activity -= FileSystemsActivity;
			}
		}

		private void FileSystemsActivity(object sender, FileSystemActivityEventArgs eventArgs)
		{
			if (activityEvents != null)
			{				
				if (this.SecurityManager.CurrentContext.IsEmpty)
				{
					activityEvents(this, eventArgs);
				}
				else
				{
					var node = this.Resolve(eventArgs.Path, eventArgs.NodeType);

					if (this.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(node, FileSystemSecuredOperation.View)))
					{
						activityEvents(this, eventArgs);
					}
				}
			}
		}

		public OverlayedNodeSelector OverlayedNodeSelector { get; }
		public virtual IList<IFileSystem> FileSystems => this.fileSystems;
		
		private static FileSystemOptions VerifyOptions(FileSystemOptions options)
		{
			return options.ChangeNodeCacheType(typeof(PathKeyedNodeCache));
		}

		public override void Close()
		{
			RemoveActivityHandlers();

			foreach (var fs in this.FileSystems)
			{
				fs.Close();
			}
		}

		/// <summary>
		/// Checks if an address is ok.
		/// </summary>
		/// <remarks>
		/// <see cref="AbstractFileSystem.AddressOk(INodeAddress)"/>
		/// </remarks>
		protected override bool AddressOk(INodeAddress address)
		{
			return true;
		}

		/// <summary>
		/// Callback handler that removes file systems as they are closed.
		/// </summary>
		protected virtual void FileSystem_Closed(object sender, EventArgs e)
		{
			((IFileSystem)sender).Activity -= this.FileSystemsActivity;

			lock (this.SyncLock)
			{
				this.fileSystems.Remove((IFileSystem)sender);
			}
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		public OverlayedFileSystem(params IFileSystem[] fileSystems)
			: this(fileSystems, FileSystemOptions.Default, new StandardOverlayedNodeSelector())
		{
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		/// <param name="options">The options for the file system.</param>
		public OverlayedFileSystem(FileSystemOptions options, params IFileSystem[] fileSystems)
			: this(fileSystems, options, new StandardOverlayedNodeSelector())
		{
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		/// <param name="options">The options for the file system.</param>
		public OverlayedFileSystem(IEnumerable<IFileSystem> fileSystems, FileSystemOptions options)
			: this(fileSystems, options, new StandardOverlayedNodeSelector())
		{
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="name">The name of the file system.</param>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		/// <param name="options">The options for the file system.</param>
		public OverlayedFileSystem(string name, IEnumerable<IFileSystem> fileSystems, FileSystemOptions options)
			: this(name, fileSystems, options, new StandardOverlayedNodeSelector())
		{			
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		/// <param name="options">The options for the file system.</param>
		/// <param name="overlayedNodeSelector">The <c>OverlayedNodeSelector</c> to use to select/resolve files.</param>
		public OverlayedFileSystem(IEnumerable<IFileSystem> fileSystems, FileSystemOptions options, OverlayedNodeSelector overlayedNodeSelector)
			: this(null, fileSystems, options, overlayedNodeSelector)
		{
		}

		/// <summary>
		/// Construct a new <see cref="OverlayedFileSystem"/>.
		/// </summary>
		/// <param name="name">The name of the file system.</param>
		/// <param name="fileSystems">The file systems to initially add to this file system.</param>
		/// <param name="options">The options for the file system.</param>
		/// <param name="overlayedNodeSelector">The <c>OverlayedNodeSelector</c> to use to select/resolve files.</param>
		public OverlayedFileSystem(string name, IEnumerable<IFileSystem> fileSystems, FileSystemOptions options, OverlayedNodeSelector overlayedNodeSelector)
			: base(new StandardNodeAddress(name ?? "overlayed", name != null ? null : OverlayedNodeProvider.NextUniqueName(), 0, 0, "", "", "/", "") , null, VerifyOptions(options))
		{
			this.fileSystems = new List<IFileSystem>();
			
			foreach (var fs in fileSystems)
			{
				fs.Closed += this.FileSystem_Closed;

				this.fileSystems.Add(fs);
			}
			
			this.OverlayedNodeSelector = overlayedNodeSelector;
		}

		public class StandardOverlayedNodeSelector
			: OverlayedNodeSelector
		{
			public override INode SelectReadNode(OverlayedFileSystem overlayedFileSystem, INodeAddress address, NodeType nodeType)
			{
				lock (overlayedFileSystem.FileSystems)
				{
					if (overlayedFileSystem.FileSystems.Count == 0)
					{
						throw new InvalidOperationException("Overlayed FileSystem isn't initialized with any file systems");
					}

					foreach (var node in overlayedFileSystem.FileSystems.Select(fs => fs.Resolve(address.AbsolutePath, nodeType)).Where(node => node.Exists))
					{
						return node;
					}

					return overlayedFileSystem.FileSystems[0].Resolve(address.AbsolutePath, nodeType);
				}
			}
		}

		/// <summary>
		/// Gets a list of alternates for a certain node.
		/// </summary>
		/// <remarks>
		/// Alternates of an <see cref="OverlayedFileSystem"/> are all the nodes on the underlying file systems that
		/// make up the <see cref="OverlayedFileSystem"/>.
		/// </remarks>
		/// <param name="node">The node to get the alternates for.</param>
		/// <returns>A list of the alternates.</returns>
		internal IEnumerable<INode> GetAlternates(INode node)
		{
			var retvals = new List<INode>();

			lock (this.SyncLock)
			{
				retvals.AddRange(this.fileSystems.Select(fileSystem => fileSystem.Resolve(node.Address.AbsolutePath, node.NodeType)));
			}

			return retvals;
		}

		protected internal INode RefreshNode(INode node)
		{
			lock (this.FileSystems)
			{
				return this.OverlayedNodeSelector.SelectReadNode(this, node.Address, node.NodeType);
			}
		}

		protected internal INode RefreshNodeForWrite(INode node)
		{
			lock (this.FileSystems)
			{
				return this.OverlayedNodeSelector.SelectWriteNode(this, node.Address, node.NodeType);
			}
		}

		internal INode GetOverlay(INodeAddress nodeAddress, NodeType nodeType)
		{
			lock (this.FileSystems)
			{
				return OverlayNode(nodeAddress, this.OverlayedNodeSelector.SelectReadNode(this, nodeAddress, nodeType));
			}
		}

		protected virtual INode OverlayNode(INodeAddress nodeAddress, INode node)
		{
			if (node.NodeType.Equals(NodeType.File))
			{
				return new OverlayedFile(this, nodeAddress, (IFile)node);
			}

			if (node.NodeType.Equals(NodeType.Directory))
			{
				return new OverlayedDirectory(this, nodeAddress, (IDirectory)node);
			}

			throw new NodeTypeNotSupportedException(node.NodeType);
		}

		protected override INode CreateNode(INodeAddress address, NodeType nodeType) => this.GetOverlay(address, nodeType);
	}
}
