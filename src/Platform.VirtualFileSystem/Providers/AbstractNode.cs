using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>INode</c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="INode"/>
	/// </summary>
	public abstract class AbstractNode
		: AbstractResolver, INode
	{
		protected volatile int eventCount;

		public virtual event NodeActivityEventHandler Renamed
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					RenamedEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					RenamedEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		protected NodeActivityEventHandler RenamedEvent;

		public virtual event NodeActivityEventHandler Created
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					CreatedEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					CreatedEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		protected NodeActivityEventHandler CreatedEvent;

		public virtual event NodeActivityEventHandler Deleted
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					DeletedEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					DeletedEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		protected NodeActivityEventHandler DeletedEvent;

		public virtual event NodeActivityEventHandler Changed
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					ChangedEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					ChangedEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		protected NodeActivityEventHandler ChangedEvent;

		public virtual event NodeActivityEventHandler Activity
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					ActivityEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					ActivityEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		protected NodeActivityEventHandler ActivityEvent;

		private int activityListenerCount;

		protected virtual void PreAddActivityEvent()
		{
			if (!this.SupportsActivityEvents)
			{
				throw new NotSupportedException();
			}
		}

		protected virtual bool AcceptsActivity(FileSystemActivityEventArgs eventArgs)
		{
			return ((eventArgs.NodeType == this.NodeType) || (eventArgs.NodeType == NodeType.Any))
					&& ((this.FileSystem.PathsEqual(eventArgs.Path, this.Address.AbsolutePath, Math.Max(this.Address.AbsolutePath.Length, eventArgs.Path.Length))) || (eventArgs.Path == "*"));
		}

		protected virtual void PostAddActivityEvent()
		{
			if (this.activityListenerCount == 0)
			{
				this.FileSystem.Activity += new FileSystemActivityEventHandler(FileSystem_Activity);
			}

			this.activityListenerCount++;
		}

		protected virtual void FileSystem_Activity(object sender, FileSystemActivityEventArgs eventArgs)
		{
			if (!this.FileSystem.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(this, FileSystemSecuredOperation.View)))
			{
				return;
			}

			if (!AcceptsActivity(eventArgs))
			{
				return;
			}

			Refresh();

			switch (eventArgs.Activity)
			{
				case FileSystemActivity.Changed:
					OnActivity(new NodeActivityEventArgs(eventArgs.Activity, this));
					OnChanged(new NodeActivityEventArgs(eventArgs.Activity, this));
					break;
				case FileSystemActivity.Created:
					OnActivity(new NodeActivityEventArgs(eventArgs.Activity, this));
					OnCreated(new NodeActivityEventArgs(eventArgs.Activity, this));
					break;
				case FileSystemActivity.Deleted:
					OnActivity(new NodeActivityEventArgs(eventArgs.Activity, this));
					OnDeleted(new NodeActivityEventArgs(eventArgs.Activity, this));
					break;
				case FileSystemActivity.Renamed:
					var renamedEventArgs = (FileSystemRenamedActivityEventArgs)eventArgs;
					OnActivity(new NodeActivityEventArgs(eventArgs.Activity, this, renamedEventArgs.Name));
					OnRenamed(new NodeActivityEventArgs(eventArgs.Activity, this, renamedEventArgs.Name));
					break;
			}

		}

		protected virtual void PreRemoveActivityEvent()
		{
			if (!this.SupportsActivityEvents)
			{
				throw new NotSupportedException();
			}
		}

		protected virtual void PostRemoveActivityEvent()
		{
			this.activityListenerCount--;

			if (this.activityListenerCount == 0)
			{
				this.FileSystem.Activity -= new FileSystemActivityEventHandler(FileSystem_Activity);
			}
		}

		protected  void PreCheckEvents()
		{
			throw new NotSupportedException();
		}

		protected  void PostCheckEvents()
		{
		}

		private INodeContent CheckAndCreateContent(string contentName)
		{
			if (this.nodeContents == null)
			{
				lock (this)
				{
					if (this.nodeContents == null)
					{
						var alternateContents = new Dictionary<string, INodeContent>();

						System.Threading.Thread.MemoryBarrier();

						this.nodeContents = alternateContents;
					}
				}
			}

			lock (this.nodeContents)
			{
				INodeContent retval;

				if (this.nodeContents.TryGetValue(contentName, out retval))
				{
					return retval;
				}
				else
				{
					retval = CreateContent(contentName);
					this.nodeContents[contentName] = retval;

					return retval;
				}
			}
		}

		private volatile IDictionary<string, INodeContent> nodeContents;

		protected virtual INodeContent CreateContent(string contentName)
		{
			if (!contentName.IsNullOrEmpty() && !SupportsAlternateContent())
			{
				throw new NotSupportedException(String.Format("CONTENT: {0}", contentName));
			}

			return new StandardNodeContent(this, contentName);
		}

		public virtual string DefaultContentName
		{
			get
			{
				return "";
			}
		}

		public virtual IEnumerable<string> GetContentNames()
		{
			yield return DefaultContentName;
		}

		public virtual INodeContent GetContent()
		{
			return CheckAndCreateContent("");
		}

		public virtual INodeContent GetContent(string contentName)
		{
			if ((contentName != null || contentName != this.DefaultContentName)
				&& !SupportsAlternateContent())
			{
				throw new NotSupportedException();
			}

			return CheckAndCreateContent(contentName);
		}

		protected virtual bool SupportsAlternateContent()
		{
			return false;
		}

		protected class StandardNodeContent
			: AbstractNodeContent
		{
			private readonly AbstractNode node;
			private readonly string contentName;

			public StandardNodeContent(AbstractNode node, string contentName)
			{
				this.node = node;
				this.contentName = contentName;
			}

			public override void Delete()
			{
				this.node.CheckAccess(FileSystemSecuredOperation.Write);

				this.node.DeleteContent(this.contentName);
			}

			protected override Stream DoOpenStream(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
			{
				var operation = FileSystemSecuredOperation.None;

				if ((fileAccess & FileAccess.Read) == FileAccess.Read)
				{
					operation = FileSystemSecuredOperation.Read;
				}

				if ((fileAccess & FileAccess.Write) == FileAccess.ReadWrite)
				{
					operation |= FileSystemSecuredOperation.Read;
				}

				this.node.CheckAccess(operation);

				return this.node.OpenStream(this.contentName, fileMode, fileAccess, fileShare);
			}

			protected override Stream DoGetInputStream(out string encoding, FileMode mode, FileShare sharing)
			{
				this.node.CheckAccess(FileSystemSecuredOperation.Read);

				return this.node.DoGetInputStream(this.contentName, out encoding, mode, sharing);
			}

			protected override Stream DoGetOutputStream(string encoding, FileMode mode, FileShare sharing)
			{
				this.node.CheckAccess(FileSystemSecuredOperation.Write);

				return this.node.DoGetOutputStream(this.contentName, encoding, mode, sharing);
			}
		}

		protected virtual void DeleteContent(string contentName)
		{
			if (contentName == null || contentName == this.DefaultContentName)
			{
				Delete();

				return;
			}

			throw new NotSupportedException();
		}

		protected virtual Stream OpenStream(string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (fileAccess == FileAccess.Read)
			{
				string encoding;

				return DoGetInputStream(contentName, out encoding, fileMode, fileShare);
			}
			else if (fileAccess == FileAccess.Write)
			{
				return DoGetOutputStream(contentName, null, fileMode, fileShare);
			}

			return DoOpenStream(contentName, fileMode, fileAccess, fileShare);
		}

		protected virtual Stream DoOpenStream(string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			throw new NotSupportedException();
		}

		protected virtual Stream DoGetInputStream(string contentName, out string encoding, FileMode fileMode, FileShare fileShare)
		{
			throw new NotSupportedException();
		}

		protected virtual Stream DoGetOutputStream(string contentName, string encoding, FileMode fileMode, FileShare fileShare)
		{
			throw new NotSupportedException();
		}

		public virtual bool SupportsActivityEvents
		{
			get
			{
				return this.FileSystem.SupportsActivityEvents;
			}
		}

		protected virtual void OnActivity(NodeActivityEventArgs eventArgs)
		{
			if (ActivityEvent != null)
			{
				ActivityEvent(this, eventArgs);
			}
		}

		protected virtual void OnRenamed(NodeActivityEventArgs eventArgs)
		{
			if (RenamedEvent != null)
			{
				RenamedEvent(this, eventArgs);
			}
		}

		protected virtual void OnCreated(NodeActivityEventArgs eventArgs)
		{
			if (CreatedEvent != null)
			{
				CreatedEvent(this, eventArgs);
			}
		}

		protected virtual void OnDeleted(NodeActivityEventArgs eventArgs)
		{
			if (DeletedEvent != null)
			{
				DeletedEvent(this, eventArgs);
			}
		}

		protected virtual void OnChanged(NodeActivityEventArgs eventArgs)
		{
			if (ChangedEvent != null)
			{
				ChangedEvent(this, eventArgs);
			}
		}
		
		protected AbstractNode(IFileSystem fileSystem, INodeAddress nodeAddress)
		{
			this.Address = nodeAddress;
			this.FileSystem = fileSystem;

			this.autoLock = new AutoLock(this.SyncLock);
		}

		public virtual INode Refresh()
		{
			this.Attributes.Refresh();

			return this;
		}
		
		/// <summary>
		/// <see cref="INode.Exists"/>
		/// </summary>
		public virtual bool Exists
		{
			get
			{
				return this.Attributes.Exists;
			}
		}

		/// <summary>
		/// <see cref="INode.NodeType"/>
		/// </summary>
		public abstract NodeType NodeType
		{
			get;
		}

		public virtual string Name
		{
			get
			{
				return this.Address.Name;
			}
		}

		public virtual INodeAddress Address { get; protected set; }

		public virtual IFileSystem FileSystem { get; protected set; }

		public virtual INode Create()
		{
			return Create(false);
		}

		public virtual void CheckAccess(FileSystemSecuredOperation operation)
		{
			if (!this.FileSystem.SecurityManager.CurrentContext.HasAccess
			(
				new AccessVerificationContext(this, operation)
			))
			{
				throw new FileSystemSecurityException(this.Address);
			}
		}

		public virtual INode Create(bool createParent)
		{
			var operationPerformed = false;

			CheckAccess(FileSystemSecuredOperation.Write);
						
			return this.FileSystem.Extenders.CompositeNodeOperationFilter.Create(this, createParent, ref operationPerformed, DoCreate);
		}

		public virtual INode DoCreate(bool createParent)
		{
			throw new NotSupportedException();
		}

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}
		
		public virtual System.Collections.Generic.IEnumerable<INode> GetAlternates()
		{
			yield return this;
		}

		public virtual IDirectory ParentDirectory
		{
			get
			{
				lock (this)
				{
					if (this.NodeType.IsLikeDirectory)
					{
						return ResolveDirectory("..");
					}
					else
					{
						return ResolveDirectory(".");
					}
				}
			}
		}

		private string Unescape(string path)
		{
			var builder = new StringBuilder(path.Length + 10);

			for (var x = 0; x < path.Length;)
			{
				builder.Append(System.Uri.HexUnescape(path, ref x));
			}

			return builder.ToString();
		}

		protected virtual INodeAddress ResolveAddress(string name, AddressScope scope)
		{
			if (this.NodeType.IsLikeDirectory)
			{
				return this.Address.ResolveAddress(name, scope);
			}
			else
			{
				return this.Address.Parent.ResolveAddress(name, scope);
			}
		}

		protected virtual INode Resolve(INodeAddress address, NodeType nodeType, AddressScope scope)
		{
			return this.FileSystem.Resolve(address, nodeType);
		}

		/// <summary>
		/// <see cref="INode.Resolve(string, NodeType, AddressScope)"/>
		/// </summary>
		/// <remarks>
		/// The default implementation validates the AddressScope by calling IsScopeValid(Uri, AddressScope)
		/// and then calls the current object's FileSystem's <c>Resolve(string, NodeType)</c>
		/// method.
		/// </remarks>
		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			var nodeAddress = this.ResolveAddress(name, scope);

			return Resolve(nodeAddress, nodeType, scope);
		}

		public virtual INode RenameTo(string name, bool overwrite)
		{
			var operationPerformed = false;

			return this.FileSystem.Extenders.CompositeNodeOperationFilter.RenameTo(this, name, overwrite, ref operationPerformed, DoRenameTo);			
		}

		protected virtual INode DoRenameTo(string name, bool overwrite)
		{
			CheckAccess(FileSystemSecuredOperation.Write);

			if (name.IndexOf(FileSystemManager.SeperatorChar) >= 0)
			{
				throw new ArgumentException("Name can not contain paths", name);
			}

			MoveTo(Resolve(name), overwrite);

			return this;
		}

		public virtual INode MoveTo(INode target, bool overwrite)
		{
			var operationPerformed = false;

			CheckAccess(FileSystemSecuredOperation.Write);

			return this.FileSystem.Extenders.CompositeNodeOperationFilter.MoveTo(this, target, overwrite, ref operationPerformed, DoMoveTo);
		}

		protected virtual INode DoMoveTo(INode target, bool overwrite)
		{
			((INodeMovingService)GetService(new NodeMovingServiceType(target, overwrite))).Run();

			return this;
		}

		public virtual INode CopyTo(INode target, bool overwrite)
		{
			bool operationPerformed = false;

			CheckAccess(FileSystemSecuredOperation.Write);

			return this.FileSystem.Extenders.CompositeNodeOperationFilter.CopyTo(this, target, overwrite, ref operationPerformed, DoCopyTo);
		}

		protected virtual INode DoCopyTo(INode target, bool overwrite)
		{
			((INodeCopyingService)GetService(new NodeCopyingServiceType(target, overwrite))).Run();

			return this;
		}

		public virtual INode MoveToDirectory(IDirectory directory, bool overwrite)
		{
			return DoMoveTo(this.GetDirectoryOperationTargetNode(directory), overwrite);
		}

		public virtual INode CopyToDirectory(IDirectory directory, bool overwrite)
		{
			return DoCopyTo(this.GetDirectoryOperationTargetNode(directory), overwrite);
		}

		public virtual INode Delete()
		{
			var operationPerformed = false;

			return this.FileSystem.Extenders.CompositeNodeOperationFilter.Delete(this, ref operationPerformed, DoDelete);
		}

		protected virtual INode DoDelete()
		{
			CheckAccess(FileSystemSecuredOperation.Write);

			throw new NotSupportedException(GetType().Name + "_Delete_NotSupported");
		}
		
		public override int GetHashCode()
		{
			return this.Address.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var node = obj as INode;

			if (node == null)
			{
				return false;
			}

			if (obj == this)
			{
				return true;
			}

			return this.NodeType.Equals(node.NodeType) && this.Address.Equals(node.Address);
		}

		public virtual IService GetService(ServiceType serviceType)
		{
			return this.FileSystem.GetService(this, serviceType);
		}

		public virtual T GetService<T>()
			where T : IService
		{
			return (T)GetService(ServiceType.FromRuntimeType(typeof(T)));
		}

		public virtual T GetService<T>(ServiceType serviceType)
			where T : IService
		{
			return (T)GetService(serviceType);
		}

		public abstract IDirectory OperationTargetDirectory
		{
			get;
		}
		
		public virtual INodeAttributes Attributes
		{
			get
			{
				if (this.attributes == null)
				{
					lock (this)
					{
						if (this.attributes == null)
						{
							this.attributes = this.CreateAttributes();
						}
					}
				}

				return this.attributes;
			}
		}

		private readonly AutoLock autoLock;

		public virtual IAutoLock GetAutoLock()
		{
			return this.autoLock;
		}

		public virtual IAutoLock AquireAutoLock()
		{
			return GetAutoLock().Lock();
		}

		protected abstract INodeAttributes CreateAttributes();

		private volatile INodeAttributes attributes;
		
		public override string ToString()
		{
			return String.Format("{0}: {1}", this.NodeType, this.Address);
		}

		public virtual int CompareTo(INode other)
		{
			return String.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public virtual void CreateGlobalLock(string name)
		{
		}

		public abstract INode GetDirectoryOperationTargetNode(IDirectory directory);
	}
}