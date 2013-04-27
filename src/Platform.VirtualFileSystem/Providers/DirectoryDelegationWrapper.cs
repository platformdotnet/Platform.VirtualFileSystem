using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class DirectoryDelegationWrapper
		: NodeDelegationWrapper, IDirectory
	{
		public virtual event NodeActivityEventHandler RecursiveActivity
		{
			add
			{
				lock (this)
				{
					if (RecursiveActivityEvent == null)
					{
						this.Wrappee.Renamed += new NodeActivityEventHandler(DelegateRecursiveActivityEvent);
					}

					RecursiveActivityEvent = (NodeActivityEventHandler)Delegate.Combine(RecursiveActivityEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					RecursiveActivityEvent = (NodeActivityEventHandler)Delegate.Remove(RecursiveActivityEvent, value);

					if (RecursiveActivityEvent == null)
					{
						this.Wrappee.Renamed -= new NodeActivityEventHandler(DelegateRecursiveActivityEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler RecursiveActivityEvent;

		private void DelegateRecursiveActivityEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnRecursiveActivityEvent(eventArgs);
		}

		protected void OnRecursiveActivityEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (RecursiveActivityEvent != null)
				{
					RecursiveActivityEvent(this, eventArgs);
				}
			}
		}

		public virtual event NodeActivityEventHandler DirectoryActivity
		{
			add
			{
				lock (this)
				{
					if (DirectoryActivityEvent == null)
					{
						this.Wrappee.Renamed += new NodeActivityEventHandler(DelegateDirectoryActivityEvent);
					}

					DirectoryActivityEvent = (NodeActivityEventHandler)Delegate.Combine(DirectoryActivityEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					DirectoryActivityEvent = (NodeActivityEventHandler)Delegate.Remove(DirectoryActivityEvent, value);

					if (DirectoryActivityEvent == null)
					{
						this.Wrappee.Renamed -= new NodeActivityEventHandler(DelegateDirectoryActivityEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler DirectoryActivityEvent;

		private void DelegateDirectoryActivityEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnDirectoryActivityEvent(eventArgs);
		}

		protected void OnDirectoryActivityEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (DirectoryActivityEvent != null)
				{
					DirectoryActivityEvent(this, eventArgs);
				}
			}
		}

		public virtual event JumpPointEventHandler JumpPointAdded
		{
			add
			{
				lock (this)
				{
					if (JumpPointAddedEvent == null)
					{
						this.Wrappee.JumpPointAdded += DelegateJumpPointAddedEvent;
					}

					JumpPointAddedEvent = (JumpPointEventHandler)Delegate.Combine(JumpPointAddedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					JumpPointAddedEvent = (JumpPointEventHandler)Delegate.Remove(JumpPointAddedEvent, value);

					if (JumpPointAddedEvent == null)
					{
						this.Wrappee.JumpPointAdded -= DelegateJumpPointAddedEvent;
					}
				}
			}
		}
		private JumpPointEventHandler JumpPointAddedEvent;

		private void DelegateJumpPointAddedEvent(object sender, JumpPointEventArgs eventArgs)
		{
			OnJumpPointAddedEvent(eventArgs);
		}

		protected void OnJumpPointAddedEvent(JumpPointEventArgs eventArgs)
		{
			lock (this)
			{
				if (JumpPointAddedEvent != null)
				{
					JumpPointAddedEvent(this, eventArgs);
				}
			}
		}

		public virtual event JumpPointEventHandler JumpPointRemoved
		{
			add
			{
				lock (this)
				{
					if (JumpPointRemovedEvent == null)
					{
						this.Wrappee.JumpPointRemoved += DelegateJumpPointRemovedEvent;
					}

					JumpPointRemovedEvent = (JumpPointEventHandler)Delegate.Combine(JumpPointRemovedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					JumpPointRemovedEvent = (JumpPointEventHandler)Delegate.Remove(JumpPointRemovedEvent, value);

					if (JumpPointRemovedEvent == null)
					{
						this.Wrappee.JumpPointRemoved -= DelegateJumpPointRemovedEvent;
					}
				}
			}
		}
		private JumpPointEventHandler JumpPointRemovedEvent;

		private void DelegateJumpPointRemovedEvent(object sender, JumpPointEventArgs eventArgs)
		{
			OnJumpPointRemovedEvent(eventArgs);
		}

		protected void OnJumpPointRemovedEvent(JumpPointEventArgs eventArgs)
		{
			lock (this)
			{
				if (JumpPointRemovedEvent != null)
				{
					JumpPointRemovedEvent(this, eventArgs);
				}
			}
		}

		protected DirectoryDelegationWrapper(IDirectory innerDirectory)
			: base(innerDirectory)
		{			
		}

		protected DirectoryDelegationWrapper(IDirectory innerDirectory, INodeResolver resolver, Converter<INode, INode> nodeAdapter)
			: base(innerDirectory, resolver, nodeAdapter)
		{
		}

		public virtual IEnumerable<INode> Walk()
		{
			if (this.NodeAdapter == Converters<INode, INode>.NoConvert)
			{
				return this.Wrappee.Walk();
			}
			else
			{
				return AdaptedWalk();
			}
		}

		private IEnumerable<INode> AdaptedWalk()
		{
			foreach (var node in this.Wrappee.Walk())
			{
				yield return NodeAdapter(node);
			}
		}

		IDirectory IDirectory.Refresh()
		{
			return (IDirectory)this.Refresh();
		}

		public virtual IEnumerable<INode> Walk(NodeType nodeType)
		{
			if (NodeAdapter == Converters<INode, INode>.NoConvert)
			{
				return this.Wrappee.Walk(nodeType);
			}
			else
			{
				return AdaptedWalk(nodeType);
			}
		}

		private IEnumerable<INode> AdaptedWalk(NodeType nodeType)
		{
			foreach (var node in this.Wrappee.Walk(nodeType))
			{
				yield return NodeAdapter(node);
			}
		}
		
		public new virtual IDirectory Wrappee
		{
			get
			{
				return (IDirectory)base.Wrappee;
			}
		}

		public virtual IEnumerable<IFile> GetFiles()
		{
			return GetFiles(PredicateUtils<IFile>.AlwaysTrue);
		}

		public virtual IEnumerable<IFile> GetFiles(Predicate<IFile> acceptFile)
		{
			foreach (INode node in this.GetChildren(NodeType.File, PredicateUtils<INode>.AlwaysTrue))
			{
				var file = (IFile)node;
				var adaptedFile = (IFile)this.NodeAdapter(file);

				if (acceptFile(adaptedFile))
				{
					yield return adaptedFile;
				}
			}
		}

		public virtual IEnumerable<IDirectory> GetDirectories()
		{
			return GetDirectories(PredicateUtils<IDirectory>.AlwaysTrue);
		}

		public virtual IEnumerable<IDirectory> GetDirectories(Predicate<IDirectory> acceptDirectory)
		{
			foreach (var node in this.GetChildren(NodeType.Directory, PredicateUtils<INode>.AlwaysTrue))
			{
				var dir = (IDirectory)node;
				var adaptedDir = (IDirectory)NodeAdapter(dir);

				if (acceptDirectory(adaptedDir))
				{
					yield return adaptedDir;
				}
			}
		}

		public virtual bool ChildExists(string name)
		{
			var node = this.Wrappee.Resolve(name);

			node.Refresh();

			node.CheckAccess(FileSystemSecuredOperation.View);

			return node.Exists;
		}

		public virtual IEnumerable<string> GetChildNames()
		{
			return GetChildNames(NodeType.Any);
		}

		public virtual IEnumerable<string> GetChildNames(NodeType nodeType)
		{
			return GetChildNames(nodeType, PredicateUtils<string>.AlwaysTrue);
		}

		public virtual IEnumerable<string> GetChildNames(Predicate<string> acceptName)
		{
			return GetChildNames(NodeType.Any, acceptName);
		}

		public virtual IEnumerable<string> GetChildNames(NodeType nodeType, Predicate<string> acceptName)
		{
			return this.Wrappee.GetChildNames(nodeType, acceptName);
		}

		public virtual IEnumerable<INode> GetChildren()
		{
			return GetChildren(NodeType.Any);
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType)
		{
			return GetChildren(nodeType, PredicateUtils<INode>.AlwaysTrue);
		}

		public virtual IEnumerable<INode> GetChildren(Predicate<INode> acceptNode)
		{
			return GetChildren(NodeType.Any, acceptNode);
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			foreach (var node in this.Wrappee.GetChildren(nodeType))
			{
				var adaptedNode = NodeAdapter(node);

				if (acceptNode(adaptedNode))
				{
					yield return adaptedNode;
				}
			}
		}

		public override INode Refresh()
		{
			Refresh(DirectoryRefreshMask.All);

			return this;
		}

		public virtual IDirectory Refresh(DirectoryRefreshMask mask)
		{
			this.Wrappee.Refresh(mask);

			return this;
		}
		
		public virtual IDirectory Delete(bool recursive)
		{
			this.Wrappee.Delete(recursive);

			return this;
		}

		IDirectory IDirectory.Create()
		{
			return this.Wrappee.Create();
		}

		IDirectory IDirectory.Create(bool createParent)
		{
			return this.Wrappee.Create(createParent);
		}

		public virtual IFileSystem CreateView()
		{
			return CreateView(this.Address.Scheme);
		}

		public virtual IFileSystem CreateView(string scheme, FileSystemOptions options)
		{
			return this.Wrappee.CreateView(scheme, options);
		}

		public IFileSystem CreateView(string scheme)
		{
			return CreateView(scheme, FileSystemOptions.NewDefault());
		}

		public IFileSystem CreateView(FileSystemOptions options)
		{
			return CreateView(this.Address.Scheme, options);
		}

		public virtual INode AddJumpPoint(INode node)
		{
			return this.Wrappee.AddJumpPoint(node);
		}

		public virtual INode AddJumpPoint(string name, INode node)
		{
			return this.Wrappee.AddJumpPoint(name, node);
		}
	}
}
