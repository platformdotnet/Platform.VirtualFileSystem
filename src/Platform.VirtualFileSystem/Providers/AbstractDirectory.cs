using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>IDirectory </c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="IDirectory"/>
	/// </summary>
	public abstract class AbstractDirectory
		: AbstractNode, IDirectory
	{
		private readonly IDictionary<string, INode> jumpPoints = new SortedDictionary<string, INode>(StringComparer.InvariantCultureIgnoreCase);

		public virtual event NodeActivityEventHandler RecursiveActivity
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					RecursiveActivityEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					RecursiveActivityEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		private NodeActivityEventHandler RecursiveActivityEvent;

		protected virtual void OnRecursiveActivity(NodeActivityEventArgs eventArgs)
		{
			if (RecursiveActivityEvent != null)
			{
				RecursiveActivityEvent(this, eventArgs);
			}
		}

		public virtual event NodeActivityEventHandler DirectoryActivity
		{
			add
			{
				lock (this)
				{
					PreAddActivityEvent();

					DirectoryActivityEvent += value;

					PostAddActivityEvent();
				}
			}

			remove
			{
				lock (this)
				{
					PreRemoveActivityEvent();

					DirectoryActivityEvent -= value;

					PostRemoveActivityEvent();
				}
			}
		}
		private NodeActivityEventHandler DirectoryActivityEvent;

		protected virtual void OnDirectoryActivity(NodeActivityEventArgs eventArgs)
		{
			if (DirectoryActivityEvent != null)
			{
				DirectoryActivityEvent(this, eventArgs);
			}
		}


		protected virtual bool InvokeRecursiveActivityRequired
		{
			get
			{
				return RecursiveActivityEvent != null;
			}
		}

		protected AbstractDirectory(IFileSystem fileSystem, INodeAddress address)
			: base(fileSystem, address)
		{
		}

		IDirectory IDirectory.Create()
		{
			return (IDirectory)Create();
		}

		IDirectory IDirectory.Create(bool createParent)
		{
			return (IDirectory)Create(createParent);
		}

		IDirectory IDirectory.Refresh()
		{
			return (IDirectory)Refresh();
		}

		public override INode Refresh()
		{
			Refresh(DirectoryRefreshMask.All);
			
			return this;
		}

		public virtual IDirectory Refresh(DirectoryRefreshMask mask)
		{
			if ((mask & DirectoryRefreshMask.Attributes) != 0)
			{
				base.Refresh();
			}
			
			return this;
		}

		public override NodeType NodeType
		{
			get
			{
				return NodeType.Directory;
			}
		}
		

		#region JumpPoint
		
		public virtual event JumpPointEventHandler JumpPointAdded;

		public virtual void OnJumpPointAdded(JumpPointEventArgs eventArgs)
		{
			if (JumpPointAdded != null)
			{
				JumpPointAdded(this, eventArgs);
			}
		}

		public virtual event JumpPointEventHandler JumpPointRemoved;

		public virtual void OnJumpPointRemoved(JumpPointEventArgs eventArgs)
		{
			if (JumpPointRemoved != null)
			{
				JumpPointRemoved(this, eventArgs);
			}
		}

		public virtual INode AddJumpPoint(INode targetNode)
		{
			return AddJumpPoint(targetNode.Name, targetNode);
		}

		public virtual INode AddJumpPoint(string name, INode targetNode)
		{
			INode jumpPointNode;
			
			if (targetNode.NodeType == NodeType.File)
			{
				jumpPointNode = new JumpPointFile((IFile)targetNode, this.Address.ResolveAddress(name));
			}
			else if (targetNode.NodeType == NodeType.Directory)
			{
				jumpPointNode = new JumpPointDirectory((IDirectory)targetNode, this.Address.ResolveAddress(name));
			}
			else
			{
				throw new NotSupportedException(targetNode.NodeType.ToString());
			}

			if (jumpPoints.ContainsKey(name))
			{
				OnJumpPointRemoved(new JumpPointEventArgs(name, targetNode, jumpPointNode));

				jumpPoints.Remove(name);
			}

			jumpPoints.Add(name, jumpPointNode);

			OnJumpPointAdded(new JumpPointEventArgs(name, targetNode, jumpPointNode));
			
			return jumpPointNode;
		}

		#endregion

		protected virtual bool ContainsShortcut(string name, NodeType nodeType)
		{
			if (nodeType == NodeType.Any)
			{
				return jumpPoints.ContainsKey(name);
			}
			else
			{
				INode node;
				
				if (jumpPoints.TryGetValue(name, out node))
				{
					return node.NodeType.Is(nodeType);
				}

				return false;
			}
		}

		public virtual IEnumerable<INode> GetJumpPoints(NodeType nodeType)
		{
			return GetJumpPoints(nodeType, PredicateUtils<INode>.AlwaysTrue);
		}

		public virtual IEnumerable<INode> GetJumpPoints(NodeType nodeType, Predicate<INode> acceptJumpPoint)
		{
			foreach (var node in jumpPoints.Values)
			{
				if (node.NodeType.Is(nodeType) || nodeType == NodeType.Any)
				{
					if (acceptJumpPoint(node))
					{
						yield return node;
					}
				}
			}
		}

		public virtual void DeleteAllJumpPoints()
		{
			lock (this.SyncLock)
			{
				var names = new List<string>();

				foreach (var s in GetJumpPointNames(NodeType.Any))
				{
					names.Add(s);
				}
			}
		}

		public virtual void DeleteJumpPoint(string name, NodeType nodeType)
		{
			lock (this.SyncLock)
			{
				INode node;

				if (!jumpPoints.TryGetValue(name, out node))
				{
					return;
				}

				jumpPoints.Remove(name);

				OnJumpPointRemoved(new JumpPointEventArgs(name, ((IWrapperObject<INode>)node).Wrappee, node));
			}
		}

		public virtual IEnumerable<string> GetJumpPointNames(NodeType nodeType)
		{
			return GetJumpPointNames(nodeType, PredicateUtils<string>.AlwaysTrue);
		}

		public virtual IEnumerable<string> GetJumpPointNames(NodeType nodeType, Predicate<string> acceptJumpPoint)
		{
			if (nodeType == NodeType.Any)
			{
				foreach (var name in jumpPoints.Keys)
				{
					if (acceptJumpPoint(name))
					{
						yield return name;
					}
				}
			}
			else
			{
				foreach (var keyValuePair in jumpPoints)
				{
					if (keyValuePair.Value.NodeType.Is(nodeType))
					{
						if (acceptJumpPoint(keyValuePair.Key))
						{
							yield return keyValuePair.Key;
						}
					}
				}
			}
		}

		public virtual IEnumerable<INode> Walk()
		{
			return Walk(NodeType.Any);
		}

		public virtual IEnumerable<INode> Walk(NodeType nodeType)
		{
			return Walk(this, nodeType);
		}

		public static IEnumerable<INode> Walk(IDirectory directory, NodeType nodeType)
		{
			foreach (var childNode in directory.GetChildren())
			{
				if (childNode.NodeType.Equals(nodeType))
				{
					yield return childNode;
				}

				if (childNode is IDirectory)
				{
					foreach (var childChildNode in Walk(((IDirectory)childNode), nodeType))
					{
						yield return childChildNode;
					}
				}
			}
		}

		protected virtual bool AcceptsRecursiveActivity(FileSystemActivityEventArgs eventArgs)
		{
			if (eventArgs.Path.Length == this.Address.AbsolutePath.Length)
			{
				return false;
			}

			return this.FileSystem.PathsEqual(eventArgs.Path, this.Address.AbsolutePath, this.Address.AbsolutePath.Length);
		}

		protected virtual bool AcceptsDirectoryActivity(FileSystemActivityEventArgs eventArgs)
		{
			int x;

			if (eventArgs.Path.Length == this.Address.AbsolutePath.Length)
			{
				return false;
			}

			x = eventArgs.Path.CountChars(c => c == '/');
			
			if (x == 1)
			{
				return true;
			}
			else if (x == 0)
			{
				return false;
			}
			else
			{
				x = eventArgs.Path.LastIndexOf('/');

				return this.FileSystem.PathsEqual(eventArgs.Path, this.Address.AbsolutePath, x);
			}
		}

		protected override void FileSystem_Activity(object sender, FileSystemActivityEventArgs eventArgs)
		{
			INode node = null;

			if (AcceptsDirectoryActivity(eventArgs))
			{
				node = this.FileSystem.Resolve(eventArgs.Path, eventArgs.NodeType);

				if (eventArgs is FileSystemRenamedActivityEventArgs)
				{
					OnDirectoryActivity(new NodeActivityEventArgs(eventArgs.Activity, node, ((FileSystemRenamedActivityEventArgs) eventArgs).NewName));
				}
				else
				{
					OnDirectoryActivity(new NodeActivityEventArgs(eventArgs.Activity, node));
				}
			}

			if (AcceptsRecursiveActivity(eventArgs) && InvokeRecursiveActivityRequired)
			{
				if (node == null)
				{
					node = this.FileSystem.Resolve(eventArgs.Path, eventArgs.NodeType);
				}

				if (eventArgs is FileSystemRenamedActivityEventArgs)
				{
					OnRecursiveActivity(new NodeActivityEventArgs(eventArgs.Activity, node, ((FileSystemRenamedActivityEventArgs)eventArgs).NewName));
				}
				else
				{
					OnRecursiveActivity(new NodeActivityEventArgs(eventArgs.Activity, node));
				}
			}

			base.FileSystem_Activity(sender, eventArgs);
		}

		public virtual bool ChildExists(string name)
		{
			return Resolve(name).Exists;
		}

		public virtual IEnumerable<IFile> GetFiles()
		{
			foreach (var node in GetChildren(NodeType.File))
			{
				yield return (IFile)node;
			}
		}

		public virtual IEnumerable<IFile> GetFiles(Predicate<IFile> acceptFile)
		{
			foreach (var node in GetChildren(NodeType.File))
			{
				if (acceptFile((IFile)node))
				{
					yield return (IFile)node;
				}
			}
		}

		public virtual IEnumerable<IDirectory> GetDirectories()
		{
			foreach (var node in GetChildren(NodeType.Directory))
			{
				yield return (IDirectory)node;
			}
		}

		public virtual IEnumerable<IDirectory> GetDirectories(Predicate<IDirectory> acceptDirectory)
		{
			foreach (var node in GetChildren(NodeType.Directory))
			{
				if (acceptDirectory((IDirectory)node))
				{
					yield return (IDirectory)node;
				}
			}
		}

		public virtual IEnumerable<INode> GetChildren()
		{
			return GetChildren(NodeType.Any, NodeFilters.Any);
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType)
		{
			return GetChildren(nodeType, NodeFilters.Any);
		}

		public virtual IEnumerable<INode> GetChildren(Predicate<INode> acceptNode)
		{
			return GetChildren(NodeType.Any, acceptNode);
		}

		public virtual IEnumerable<INode> GetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			if (!this.FileSystem.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(this, FileSystemSecuredOperation.List)))
			{
				yield break;
			}

			foreach (INode node in DoGetChildren(nodeType, acceptNode))
			{
				if (this.FileSystem.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(node, FileSystemSecuredOperation.View)))
				{
					yield return node;
				}
			}
		}

		public abstract IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode);

		private IEnumerable<string> GetChildNamesUsingNodes(NodeType nodeType, Predicate<INode> acceptNode)
		{
			foreach (var node in GetChildren(nodeType, acceptNode))
			{
				yield return node.Name;
			}
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
			return GetChildNamesUsingNodes(nodeType, NodeFilters.ByNodeName(acceptName));
		}

		public virtual IDirectory Delete(bool recursive)
		{
			return DoDelete(recursive);
		}

		protected virtual IDirectory DoDelete(bool recursive)
		{
			throw new NotSupportedException();
		}

		public override IDirectory OperationTargetDirectory
		{
			get
			{
				return this;
			}
		}

		public virtual IFileSystem CreateView()
		{
			return CreateView(this.Address.Scheme);
		}

		public virtual IFileSystem CreateView(FileSystemOptions options)
		{
			return DoCreateView(this.Address.Scheme, options);
		}

		public virtual IFileSystem CreateView(string scheme)
		{
			return CreateView(scheme, (FileSystemOptions)this.FileSystem.Options.Clone());
		}

		public virtual IFileSystem CreateView(string scheme, FileSystemOptions options)
		{
			return DoCreateView(scheme, options);
		}

		protected virtual IFileSystem DoCreateView(string scheme, FileSystemOptions options)
		{
			return new View.ViewFileSystem(scheme, this, options);
		}

		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			var nodeAddress = base.ResolveAddress(name, scope);

			if (nodeAddress.IsDescendentOf(this.Address, AddressScope.Descendent))
			{
				INode jumpPointNode;
				
				if (jumpPoints.TryGetValue(name, out jumpPointNode))
				{
					var relativePath = this.Address.GetRelativePathTo(nodeAddress);

					if (relativePath == name)
					{
						return jumpPointNode;
					}
					else if (jumpPointNode.NodeType == NodeType.Directory)
					{
						return jumpPointNode.Resolve(relativePath);
					}
				}
			}
			
			return base.Resolve(nodeAddress, nodeType, scope);
		}

		public override INode GetDirectoryOperationTargetNode(IDirectory directory)
		{
			return directory;
		}
	}	
}