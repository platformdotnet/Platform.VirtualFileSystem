using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// Wraps an <i>INode</i> and uses consultation redirect operations
	/// to the parent (wrapped) node (also known as the wrappee or target).
	/// </summary>
	/// <remarks>
	/// Implementation redirection is performed using <c>consulation</c>.
	/// Consultation means all operations are delegated to the wrappee and
	/// evaluated in the context of the wrappee.
	/// <para>
	/// References:
	/// http://javalab.cs.uni-bonn.de/research/darwin/delegation.html	
	/// </para>
	/// </remarks>
	public abstract class NodeConsultationWrapper
		: INode, IWrapperObject<INode>
	{
		public virtual event NodeActivityEventHandler Renamed
		{
			add
			{
				lock (this)
				{
					if (RenamedEvent == null)
					{
						this.Wrappee.Renamed += new NodeActivityEventHandler(DelegateRenamedEvent);
					}

					RenamedEvent = (NodeActivityEventHandler)Delegate.Combine(RenamedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					RenamedEvent = (NodeActivityEventHandler)Delegate.Remove(RenamedEvent, value);

					if (RenamedEvent == null)
					{
						this.Wrappee.Renamed -= new NodeActivityEventHandler(DelegateRenamedEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler RenamedEvent;

		private void DelegateRenamedEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnRenamedEvent(eventArgs);
		}

		protected virtual void OnRenamedEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (RenamedEvent != null)
				{
					RenamedEvent(this, eventArgs);
				}
			}
		}

		public virtual event NodeActivityEventHandler Created
		{
			add
			{
				lock (this)
				{
					if (CreatedEvent == null)
					{
						this.Wrappee.Created += new NodeActivityEventHandler(DelegateCreatedEvent);
					}

					CreatedEvent = (NodeActivityEventHandler)Delegate.Combine(CreatedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					CreatedEvent = (NodeActivityEventHandler)Delegate.Remove(CreatedEvent, value);

					if (CreatedEvent == null)
					{
						this.Wrappee.Created -= new NodeActivityEventHandler(DelegateCreatedEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler CreatedEvent;

		private void DelegateCreatedEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnCreatedEvent(eventArgs);
		}

		protected void OnCreatedEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (CreatedEvent != null)
				{
					CreatedEvent(this, eventArgs);
				}
			}
		}

		public virtual event NodeActivityEventHandler Deleted
		{
			add
			{
				lock (this)
				{
					if (DeletedEvent == null)
					{
						this.Wrappee.Deleted += new NodeActivityEventHandler(DelegateDeletedEvent);
					}

					DeletedEvent = (NodeActivityEventHandler)Delegate.Combine(DeletedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					DeletedEvent = (NodeActivityEventHandler)Delegate.Remove(DeletedEvent, value);

					if (DeletedEvent == null)
					{
						this.Wrappee.Deleted -= new NodeActivityEventHandler(DelegateDeletedEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler DeletedEvent;

		private void DelegateDeletedEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnDeletedEvent(eventArgs);
		}

		protected void OnDeletedEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (DeletedEvent != null)
				{
					DeletedEvent(this, eventArgs);
				}
			}
		}

		public virtual event NodeActivityEventHandler Changed
		{
			add
			{
				lock (this)
				{
					if (ChangedEvent == null)
					{
						this.Wrappee.Changed += new NodeActivityEventHandler(DelegateChangedEvent);
					}

					ChangedEvent = (NodeActivityEventHandler)Delegate.Combine(ChangedEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					ChangedEvent = (NodeActivityEventHandler)Delegate.Remove(ChangedEvent, value);

					if (ChangedEvent == null)
					{
						this.Wrappee.Changed -= new NodeActivityEventHandler(DelegateChangedEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler ChangedEvent;

		private void DelegateChangedEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnChangedEvent(eventArgs);
		}

		protected void OnChangedEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (ChangedEvent != null)
				{
					ChangedEvent(this, eventArgs);
				}
			}
		}

		public virtual event NodeActivityEventHandler Activity
		{
			add
			{
				lock (this)
				{
					if (ActivityEvent == null)
					{
						this.Wrappee.Activity += new NodeActivityEventHandler(DelegateActivityEvent);
					}

					ActivityEvent = (NodeActivityEventHandler)Delegate.Combine(ActivityEvent, value);
				}
			}

			remove
			{
				lock (this)
				{
					ActivityEvent = (NodeActivityEventHandler)Delegate.Remove(ActivityEvent, value);

					if (ActivityEvent == null)
					{
						this.Wrappee.Activity -= new NodeActivityEventHandler(DelegateActivityEvent);
					}
				}
			}
		}
		private NodeActivityEventHandler ActivityEvent;

		private void DelegateActivityEvent(object sender, NodeActivityEventArgs eventArgs)
		{
			OnActivityEvent(eventArgs);
		}

		protected void OnActivityEvent(NodeActivityEventArgs eventArgs)
		{
			lock (this)
			{
				if (ActivityEvent != null)
				{
					ActivityEvent(this, eventArgs);
				}
			}
		}

		public virtual string Name
		{
			get
			{
				return this.Address.Name;
			}
		}

		public virtual IFileSystem FileSystem
		{
			get
			{
				return this.Wrappee.FileSystem;
			}
		}

		public virtual IDirectory ParentDirectory
		{
			get
			{
				return this.Wrappee.ParentDirectory;
			}
		}

		public virtual NodeType NodeType
		{
			get
			{
				return this.Wrappee.NodeType;
			}
		}

		public virtual bool Exists
		{
			get
			{
				return this.Wrappee.Exists;
			}
		}

		protected virtual INode Wrappee { get; private set; }

		INode IWrapperObject<INode>.Wrappee
		{
			get
			{
				return this.Wrappee;
			}
		}

		public virtual string DefaultContentName
		{
			get
			{
				return this.Wrappee.DefaultContentName;
			}
		}

		public virtual IEnumerable<string> GetContentNames()
		{
			return this.Wrappee.GetContentNames();
		}

		public virtual INodeContent GetContent()
		{
			return this.Wrappee.GetContent();
		}

		public virtual INodeContent GetContent(string contentName)
		{
			return this.Wrappee.GetContent(contentName);
		}

		public virtual IAutoLock GetAutoLock()
		{
			return this.Wrappee.GetAutoLock();
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return this.Wrappee.AquireAutoLock();
		}

		protected virtual void SetWrappee(INode node)
		{
			this.Wrappee = node;
		}

		public virtual bool SupportsActivityEvents
		{
			get
			{
				return this.Wrappee.SupportsActivityEvents;
			}
		}

		protected NodeConsultationWrapper(INode innerNode)
		{
			this.Wrappee = innerNode;
		}
				
		public virtual INode Create()
		{
			var operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.Create(this, false, ref operationPerformed, this.Wrappee.Create);

			if (!operationPerformed)
			{
				this.Wrappee.Create();
			}

			return this;
		}

		public virtual INode Create(bool createParent)
		{
			bool operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.Create(this, createParent, ref operationPerformed, this.Wrappee.Create);

			if (!operationPerformed)
			{
				this.Wrappee.Create(createParent);
			}
			
			return this;
		}

		public virtual INode Delete()
		{
			var operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.Delete(this, ref operationPerformed, this.Wrappee.Delete);

			if (!operationPerformed)
			{
				this.Wrappee.Delete();
			}

			return this;
		}

		public virtual IFile ResolveFile(string name)
		{
			return this.Wrappee.ResolveFile(name);
		}

		public virtual IDirectory ResolveDirectory(string name)
		{
			return this.Wrappee.ResolveDirectory(name);
		}

		public virtual IFile ResolveFile(string name, AddressScope scope)
		{
			return this.Wrappee.ResolveFile(name, scope);
		}

		public virtual IDirectory ResolveDirectory(string name, AddressScope scope)
		{
			return this.Wrappee.ResolveDirectory(name, scope);
		}

		public virtual INode Resolve(string name)
		{
			return this.Wrappee.Resolve(name);
		}

		public virtual INode Resolve(string name, AddressScope scope)
		{
			return this.Wrappee.Resolve(name, scope);
		}

		public virtual INode Resolve(string name, NodeType nodeType)
		{
			return this.Wrappee.Resolve(name, nodeType);
		}

		public virtual INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			return this.Wrappee.Resolve(name, nodeType, scope);
		}

		public virtual INode MoveTo(INode target, bool overwrite)
		{
			var operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.MoveTo(this, target, overwrite, ref operationPerformed, this.Wrappee.MoveTo);

			if (!operationPerformed)
			{
				this.Wrappee.MoveTo(target, overwrite);
			}

			return this;
		}

		public virtual INode CopyTo(INode target, bool overwrite)
		{
			var operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.CopyTo(this, target, overwrite, ref operationPerformed, this.Wrappee.CopyTo);

			if (!operationPerformed)
			{
				this.Wrappee.CopyTo(target, overwrite);
			}

			return this;
		}

		public virtual IDirectory OperationTargetDirectory
		{
			get
			{
				return this.Wrappee.OperationTargetDirectory;
			}
		}

		public virtual IService GetService(ServiceType serviceType)
		{
			return this.Wrappee.GetService(serviceType);
		}

		public virtual T GetService<T>(ServiceType serviceType)
			where T : IService
		{
			return this.Wrappee.GetService<T>(serviceType);
		}

		public virtual T GetService<T>()
			where T : IService
		{
			return this.Wrappee.GetService<T>();
		}

		public virtual INodeAddress Address
		{
			get
			{
				return this.Wrappee.Address;
			}
		}

		public virtual INode Refresh()
		{
			this.Wrappee.Refresh();

			return this;
		}

		public virtual IEnumerable<INode> GetAlternates()
		{
			return this.Wrappee.GetAlternates();
		}

		public virtual INodeAttributes Attributes
		{
			get
			{
				return this.Wrappee.Attributes;
			}
		}

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode();
		}

		public override string ToString()
		{
			return this.Address.ToString();
		}

		public virtual object SyncLock
		{
			get
			{
				return this.Wrappee.SyncLock;
			}
		}

		public virtual INode RenameTo(string name, bool overwrite)
		{
			var operationPerformed = false;

			this.FileSystem.Extenders.CompositeNodeOperationFilter.RenameTo(this, name, overwrite, ref operationPerformed, this.Wrappee.RenameTo);

			if (!operationPerformed)
			{
				this.Wrappee.RenameTo(name, overwrite);
			}

			return this;
		}

		public virtual INode MoveToDirectory(IDirectory directory, bool overwrite)
		{
			var operationPerformed = false;
			var target = this.GetDirectoryOperationTargetNode(directory);

			this.FileSystem.Extenders.CompositeNodeOperationFilter.MoveTo(this, target, overwrite, ref operationPerformed, this.Wrappee.MoveTo);

			if (!operationPerformed)
			{
				this.Wrappee.MoveTo(target, overwrite);
			}

			return this;
		}

		public virtual INode CopyToDirectory(IDirectory directory, bool overwrite)
		{
			var operationPerformed = false;
			var target = this.GetDirectoryOperationTargetNode(directory);

			this.FileSystem.Extenders.CompositeNodeOperationFilter.CopyTo(this, target, overwrite, ref operationPerformed, this.Wrappee.CopyTo);

			if (!operationPerformed)
			{
				this.Wrappee.CopyTo(target, overwrite);
			}

			return this;
		}

		public virtual int CompareTo(INode other)
		{
			return this.Wrappee.CompareTo(other);
		}

		public virtual void CheckAccess(FileSystemSecuredOperation operation)
		{
			this.Wrappee.CheckAccess(operation);
		}

		public virtual INode GetDirectoryOperationTargetNode(IDirectory directory)
		{
			return this.Wrappee.GetDirectoryOperationTargetNode(directory);
		}

		public virtual void CreateGlobalLock(string name)
		{
			this.Wrappee.CreateGlobalLock(name);
		}
	}
}