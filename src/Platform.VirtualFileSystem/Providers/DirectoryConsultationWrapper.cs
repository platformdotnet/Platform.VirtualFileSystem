using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class DirectoryConsulatationWrapper
		: NodeConsultationWrapper, IDirectory
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

		/// <summary>
		/// Delegate DirectoryActivity event
		/// </summary>
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

		protected DirectoryConsulatationWrapper(IDirectory innerDirectory)
			: base(innerDirectory)
		{
		}

		public virtual IEnumerable<INode> Walk()
		{
			return this.Wrappee.Walk();
		}

		public virtual IEnumerable<INode> Walk(NodeType nodeType)
		{
			return this.Wrappee.Walk(nodeType);
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
			return this.Wrappee.GetFiles();
		}

		public virtual IEnumerable<IFile> GetFiles(Predicate<IFile> acceptFile)
		{
			return this.Wrappee.GetFiles(acceptFile);
		}

		public virtual IEnumerable<IDirectory> GetDirectories()
		{
			return this.Wrappee.GetDirectories();
		}

		public virtual IEnumerable<IDirectory> GetDirectories(Predicate<IDirectory> acceptDirectory)
		{
			return this.Wrappee.GetDirectories(acceptDirectory);
		}

		public virtual bool ChildExists(string name)
		{
			return this.Wrappee.ChildExists(name);
		}

		public virtual IEnumerable<string> GetChildNames()
		{
			return this.Wrappee.GetChildNames();
		}

		public virtual IEnumerable<string> GetChildNames(NodeType nodeType)
		{
			return this.Wrappee.GetChildNames(nodeType);
		}

		public virtual IEnumerable<string> GetChildNames(Predicate<string> acceptName)
		{
			return this.Wrappee.GetChildNames(acceptName);
		}

		public virtual IEnumerable<string> GetChildNames(NodeType nodeType, Predicate<string> acceptName)
		{
			return this.Wrappee.GetChildNames(nodeType, acceptName);
		}

		public virtual IEnumerable<INode> GetChildren()
		{
			return this.Wrappee.GetChildren();
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType)
		{
			return this.Wrappee.GetChildren(nodeType);
		}

		public virtual IEnumerable<INode> GetChildren(Predicate<INode> acceptNode)
		{
			return this.Wrappee.GetChildren(acceptNode);
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			return this.GetChildren(nodeType, acceptNode);
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
			return this.Wrappee.CreateView();
		}

		public virtual IFileSystem CreateView(string scheme, FileSystemOptions options)
		{
			return this.Wrappee.CreateView(scheme, options);
		}

		public IFileSystem CreateView(string scheme)
		{
			return this.Wrappee.CreateView(scheme);
		}

		public IFileSystem CreateView(FileSystemOptions options)
		{
			return this.Wrappee.CreateView(options);
		}

		public virtual INode AddJumpPoint(INode node)
		{
			return this.Wrappee.AddJumpPoint(node);
		}

		public virtual INode AddJumpPoint(string name, INode node)
		{
			return this.Wrappee.AddJumpPoint(name, node);
		}

		IDirectory IDirectory.Refresh()
		{
			return (IDirectory)this.Refresh();
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
	}
}
