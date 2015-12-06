using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of <c>IFileSystem</c> to minimize the effort 
	/// required to implement the interface.	
	/// </summary>
	/// <remarks>
	/// <seealso cref="IFileSystem"/>
	/// </remarks>
	public abstract class AbstractFileSystem
		: AbstractResolver, IFileSystem
	{
		#region Fields

		private readonly INodeAddress rootAddress;
		private readonly INodeCache cache;
		private volatile IDirectory rootDirectory;
		private readonly AutoLock autoLock;

		#endregion

        public virtual bool IsDisposed
        {
            get
            {
                return false;
            }
        }

		public virtual FileSystemSecurityManager SecurityManager { get; private set; }

		public virtual bool HasAccess(INode node, FileSystemSecuredOperation operation)
		{
			if (!this.SecurityManager.IsActive)
			{
				return true;
			}

			return this.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(node, operation));
		}

		public virtual void CheckAccess(INode node, FileSystemSecuredOperation operation)
		{
			if (!HasAccess(node, operation))
			{
				throw new FileSystemSecurityException(node.Address);
			}
		}

		#region Events

		/// <summary>
		/// Raised when this <see cref="IFileSystem"/> is closed.
		/// </summary>
		public virtual event EventHandler Closed;
		
		/// <summary>
		/// Raised when activity occurs in this <see cref="IFileSystem"/>.
		/// </summary>
		public virtual event FileSystemActivityEventHandler Activity
		{
			add
			{
				throw new NotSupportedException();
			}
			remove
			{
				throw new NotSupportedException();
			}
		}

		#endregion

		#region Properties
		
		public virtual bool SupportsSeeking
		{
			get
			{
				return false;
			}
		}

		public virtual AutoLock AquireAutoLock()
		{
			return this.autoLock;
		}

		public virtual object SyncLock
		{
			get
			{
				return this;
			}
		}

		public virtual int MaximumPathLength
		{
			get
			{
				return Int32.MaxValue;
			}
		}

		protected IFile ParentLayer { get; private set; }

		protected virtual INodeAddress RootAddress
		{
			get
			{
				return this.rootAddress;
			}
		}

		public virtual bool SupportsActivityEvents
		{
			get
			{
				return false;
			}
		}

		public FileSystemOptions Options { get; private set; }

		public virtual IDirectory RootDirectory
		{
			get
			{
				if (this.rootDirectory == null)
				{
					lock (this)
					{
						if (this.rootDirectory == null)
						{
							this.rootDirectory = ResolveDirectory("/");
						}
					}
				}

				return this.rootDirectory;
			}
		}

		public virtual void Close()
		{
			this.SecurityManager.Dispose();

			OnClosed();
		}

		public virtual bool NodeExists(string name, NodeType nodeType)
		{
			var node = Resolve(name, nodeType);

			node.Attributes.Refresh();

			return node.Exists;
		}

		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			if (!nodeType.IsLikeDirectory && name.EndsWith(FileSystemManager.SeperatorString))
			{
				name = name.TrimRight(FileSystemManager.SeperatorChar);
			}

			var nodeAddress = this.rootAddress.ResolveAddress(name);

			if (nodeType == NodeType.Any && name.EndsWith(FileSystemManager.SeperatorString))
			{
				nodeType = NodeType.Directory;
			}
			
			return Resolve(nodeAddress, nodeType);
		}

		public virtual NodeType GetNodeType(string path)
		{
			if (NodeExists(path, NodeType.File))
			{
				return NodeType.File;
			}
			else if (NodeExists(path, NodeType.Directory))
			{
				return NodeType.Directory;
			}
			else
			{
				return NodeType.None;
			}
		}

		/// <summary>
		/// Initializes the file system with the supplied <c>rootAddress</c>, <c>parentLayer</c> and <c>options</c>.
		/// </summary>
		/// <param name="rootAddress">
		/// The rootAddress for the file system.  All nodes in the file system are relative to the root name.</param>
		/// <param name="parentLayer">
		/// The parent layer for this file system or <c>null</c> if this <see cref="IFileSystem"/> is not layered.
		/// </param>
		/// <param name="options">
		/// The options for creating this <c>FileSystem.</c>
		/// </param>
		protected AbstractFileSystem(INodeAddress rootAddress, IFile parentLayer, FileSystemOptions options)
		{
			this.cache = (INodeCache)Activator.CreateInstance(options.NodeCacheType);
			this.ParentLayer = parentLayer;
			this.rootAddress = rootAddress;
			this.Options = options;
			this.autoLock = new AutoLock(this);

			InitializeConstruction(rootAddress, parentLayer, options);

			this.Extenders = CreateExtenders();			
			CreateSecurityManager();
		}

		/// <summary>
		/// Called by the constructor before the file system extenders and security manager is created.
		/// </summary>
		/// <remarks>
		/// Override this method to perform FileSystem initialization routines that need to be performed
		/// before the extenders and security manager are constructed.  Initialization routines that rely
		/// on extenders or the security manager should be created within the dervied class's constructor.
		/// </remarks>
		/// <param name="rootAddress"></param>
		/// <param name="parentLayer"></param>
		/// <param name="options"></param>
		protected virtual void InitializeConstruction(INodeAddress rootAddress, IFile parentLayer, FileSystemOptions options)
		{
		}

		protected virtual void CreateSecurityManager()
		{
			this.SecurityManager = new FileSystemSecurityManager();

			foreach (var type in this.Options.AccessPermissionVerifierTypes)
			{
				var verifier = (AccessPermissionVerifier)this.Extenders.ConstructExtender(type);

				this.SecurityManager.GlobalContext.AddPermissionVerifier(verifier);
			}
		}
		
		protected virtual FileSystemExtenders CreateExtenders()
		{
			return new FileSystemExtenders(this);
		}

		public virtual FileSystemExtenders Extenders { get; private set; }

		protected virtual void SetParentLayer(IFile file)
		{
			this.ParentLayer = file;
		}

		public virtual INodeCache NodeCache
		{
			get
			{
				return this.cache;
			}
		}

		protected virtual void AddToCache(INode node)
		{
			lock (this.cache)
			{
				this.cache.Add(node.Address, node);
			}
		}

		protected virtual void AddToCache(INodeAddress address, INode node)
		{
			lock (this.cache)
			{
				this.cache.Add(address, node);
			}
		}

		/// <summary>
		/// Gets a node from this <see cref="IFileSystem"/>'s <see cref="INodeCache"/>.
		/// </summary>
		/// <remarks>
		/// Nodes are with the same address are equivalent.  <c>INodeAddress.Equals()</c> is used to compare nodes.
		/// </remarks>
		/// <param name="address">The address of the node.</param>
		/// <param name="nodeType">
		/// The type of node to look for.  If the this argument is <see cref="NodeType.Any"/> then the first node
		/// of any type that exists in the cache is returned.  If more than one node type with the same address
		/// exists in the cache then the node that is returned will be one of the nodes that match but the exact
		/// <see cref="NodeType"/> returned is non-deterministic.
		/// </param>
		/// <returns></returns>
		protected INode GetFromCache(INodeAddress address, NodeType nodeType)
		{			
			return this.cache.Get(address, nodeType);
		}
	
		protected virtual void OnClosed()
		{
			if (Closed != null)
			{
				Closed(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Verifies that an address is valid within the scope of this <see cref="IFileSystem"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation of this method returns true if the address scheme is the
		/// same as the <see cref="IFileSystem"/>'s <c>rootAddress</c>'s scheme.
		/// </remarks>
		/// <param name="address">The address to verify.</param>
		/// <returns>True if the address is valid within the scope of this <see cref="IFileSystem"/>.</returns>
		protected virtual bool AddressOk(INodeAddress address)
		{
			return this.rootAddress.RootsAreEqual(address);
		}

		#endregion

		public virtual IService GetService(INode node, ServiceType serviceType)
		{
			if (this.Extenders.NodeServiceProviders.Count > 0)
			{
				foreach (INodeServiceProvider provider in this.Extenders.NodeServiceProviders)
				{
					var retval = provider.GetService(node, serviceType);

					if (retval != null)
					{
						return retval;
					}
				}
			}

			throw new NotSupportedException("The specified service is not supported");
		}

		/// <summary>
		/// Decorates the given node.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Implementers should override this method and add necessary customisations.  This method will usually
		/// only be called once for each unique node (because nodes are cached) but implementers should not rely on 
		/// this always being true since nodes could be evicted from the node cache during a file system's lifetime
		/// (this depends on the actual node cache implementation).
		/// </p>
		/// <p>
		/// This default implementation of this method uses the FileSystem's default <c>NodeDecorator</c> stored in
		/// the <see cref="IFileSystem.Options"/> property.  
		/// </p>
		/// <seealso cref="FileSystemOptions.NodeDecorator"/>
		/// </remarks>
		/// <param name="node">The node to check</param>
		/// <returns>The decorated node).
		protected virtual INode Decorate(INode node)
		{
			var retval = node;

			if (this.Extenders != null)
			{
				foreach (INodeTransformationFilter filter in this.Extenders.NodeTransformationFilters)
				{
					retval = filter.Filter(retval);
				}
			}

			return retval;
		}

		protected class NodeAddressLengthComparer
			: IComparer<INodeAddress>
		{
			public static readonly NodeAddressLengthComparer Default = new NodeAddressLengthComparer();

			public virtual int Compare(INodeAddress address1, INodeAddress address2)
			{
				var x = address1.Depth - address2.Depth;

				if (x == 0)
				{
					return StringComparer.CurrentCulture.Compare(address1.AbsolutePath, address2.AbsolutePath);
				}
				else
				{
					return x;
				}
			}
		}

		private readonly IDictionary<INodeAddress, INodeAddress> shortcutAddresses = new SortedDictionary<INodeAddress, INodeAddress>(NodeAddressLengthComparer.Default);
				
		protected virtual bool IsContainedInShortcut(INodeAddress address)
		{
			INodeAddress tempAddress;

			if (address.IsRoot)
			{
				return false;
			}

			if (this.shortcutAddresses.Count == 0)
			{
				return false;
			}

			var addressKey = address;

			if (this.shortcutAddresses.TryGetValue(addressKey, out tempAddress))
			{
				return true;
			}

			foreach (var jumpPointAddress in this.shortcutAddresses.Values)
			{
				if (address.Depth < jumpPointAddress.Depth)
				{
					break;
				}

				if (jumpPointAddress.IsRoot)
				{
					continue;
				}

				var jumpPointParentAddress = jumpPointAddress.Parent;

				if (jumpPointParentAddress.AbsolutePath != address.PathToDepth(jumpPointParentAddress.Depth))
				{
					continue;
				}

				if (address.IsDescendentOf(jumpPointAddress, StringComparison.CurrentCultureIgnoreCase, AddressScope.DescendentOrSelf))
				{
					return true;
				}
			}

			return false;
		}

		private void NodeJumpPointAdded(object sender, JumpPointEventArgs eventArgs)
		{
			lock (this)
			{
				var dir = (IDirectory)sender;
				var address = dir.Address.ResolveAddress("./" + eventArgs.Name);

				this.shortcutAddresses[address] = address;
			}
		}

		private void NodeJumpPointRemoved(object sender, JumpPointEventArgs eventArgs)
		{
			lock (this)
			{
				var dir = (IDirectory)sender;
				var address = dir.Address.ResolveAddress("./" + eventArgs.Name);

				this.shortcutAddresses[address] = address;
				this.shortcutAddresses.Remove(address);

				this.cache.PurgeWithDescendents(address, eventArgs.Target.NodeType);
			}
		}

		private void AttachShortcut(IDirectory dirNode)
		{
			dirNode.JumpPointAdded += NodeJumpPointAdded;
			dirNode.JumpPointRemoved += NodeJumpPointRemoved;
		}

		public virtual INode Resolve(INodeAddress address, NodeType nodeType)
		{
			INode node;

			var fromCache = GetFromCache(address, nodeType);

			if (fromCache != null)
			{
				return fromCache;
			}

			lock (this.SyncLock)
			{
				if (IsContainedInShortcut(address))
				{
					var parent = (IDirectory)this.Resolve(address.Parent, NodeType.Directory);
					
					node = parent.Resolve(address.Name, nodeType);

					AddToCache(address, node);

					return node;
				}
			
				INodeResolver resolver = this;

				if (this.Extenders != null)
				{
					foreach (INodeResolutionFilter filter in this.Extenders.NodeResolutionFilters)
					{
						bool canCache;

						if ((node = filter.Filter(ref resolver, ref address, ref nodeType, out canCache)) != null)
						{
							node = Decorate(node);

							if (canCache)
							{
								if (node is IDirectory)
								{
									AttachShortcut((IDirectory)node);
								}

								AddToCache(node);
							}

							return node;
						}
					}
				}
			}

			if (!AddressOk(address))
			{
				throw new MalformedUriException(address.Uri, "Malformed or unsupported URI");
			}

			lock (this.SyncLock)
			{
				if (fromCache == null)
				{
					node = CreateNode(address, nodeType);

					node = Decorate(node);

					if (node is IDirectory)
					{
						AttachShortcut((IDirectory)node);
					}

					AddToCache(node);
				}
				else
				{
					node = fromCache;
				}

				return node;
			}
		}

		protected abstract INode CreateNode(INodeAddress address, NodeType nodeType);

		public virtual bool PathsEqual(string path1, string path2, int length)
		{
			if (length > path1.Length || length > path2.Length)
			{
				return false;
			}

			for (var i = 0; i < length; i++)
			{
				if (!(Char.ToLower(path1[i]).Equals(Char.ToLower(path2[i]))))
				{
					return false;
				}
			}

			return true;
		}

        public void Dispose()
        {
	        this.Dispose(true);
	        GC.SuppressFinalize(this);
        }

		protected virtual void Dispose(bool disposed)
		{
			Close();
		}
    }
}
