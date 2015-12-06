using System;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// Wraps an <i>INode</i> and emulated delegation to redirect operations
	/// to the parent (wrapped) node (also known as the wrappee or target).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Implementation redirection is performed using <c>delegation</c>.
	/// Full delegation is not possible but is emulated in the following ways:
	/// </para>
	/// <para>
	/// All <c>Resolve</c> calls to the <see cref="NodeDelegationWrapper.Resolver"/>.
	/// The default <see cref="NodeDelegationWrapper.Resolver"/> delegates all
	/// <c>Resolve</c> calls to 
	/// <see cref="NodeDelegationWrapper.Resolve(string, NodeType, AddressScope)"/>.
	/// This allows subclasses of this class to simply override 
	/// <see cref="NodeDelegationWrapper.Resolve(string, NodeType, AddressScope)"/>
	/// in order to delegate the behaviour of all the <c>Resolve</c> methods.
	/// </para>	
	/// <para>
	/// The <see cref="NodeDelegationWrapper.ToString()"/> method is override to
	/// return <c>NodeDelegationWrapper.Address.Tostring()</c>.  The default
	/// implementaion of <see cref="NodeDelegationWrapper.Address"/> returns the
	/// wrappee's <c>Address</c> so the default delegated behaviour for 
	/// <see cref="NodeDelegationWrapper.ToString()"/> is the same as 
	/// undelegated behaviour.  Subclasses that override 
	/// <see cref="NodeDelegationWrapper.Address"/> will automatically get the correct
	/// <see cref="NodeDelegationWrapper.ToString()"/> implementation based on
	/// the override <see cref="NodeDelegationWrapper.Address"/>.
	/// </para>
	/// <para>
	/// All methods intended to return the current node will not return
	/// the wrappee but will return the <see cref="NodeDelegationWrapper"/>.
	/// </para>
	/// <para>
	/// All methods (i.e. children or parent) returning new nodes (known as
	/// adapter-condidate nodes) will return a node, optionally converted 
	/// (adapted) by the supplied 
	/// <see cref="NodeDelegationWrapper.NodeAdapter"/>.  The default 
	/// <c>NodeAdapter</c> returns the node unadapted.
	/// </para>
	/// <para>
	/// References:
	/// http://javalab.cs.uni-bonn.de/research/darwin/delegation.html	
	/// </para>
	/// </remarks>
	public abstract class NodeDelegationWrapper
		: NodeConsultationWrapper
	{
		internal class DefaultResolver
			: AbstractResolver
		{
			private readonly INode node;
			private INodeAddress parentAddress;
						
			public DefaultResolver(NodeDelegationWrapper node)
			{
				this.node = node;
			}

			public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
			{
				if (this.parentAddress == null)
				{
					if (this.node.NodeType == NodeType.File)
					{
						this.parentAddress = this.node.Address.Parent;
					}
					else
					{
						this.parentAddress = this.node.Address;
					}
				}

				return this.node.FileSystem.Resolve(this.parentAddress.ResolveAddress(name).AbsolutePath, nodeType, scope);
			}
		}

		protected virtual Converter<INode, INode> NodeAdapter { get; set; }

		protected virtual INodeResolver NodeResolver { get; set; }

		protected NodeDelegationWrapper(INode innerNode)
			: this(innerNode, null, ConverterUtils<INode, INode>.NoConvert)
		{
		}

		/// <summary>
		/// Construct a new <see cref="NodeDelegationWrapper"/>.
		/// </summary>
		/// <param name="innerNode">
		/// The <see cref="INode"/> to delegate to.
		/// </param>
		/// <param name="resolver">
		/// The resolver used to delegate <c>Resolve</c> calls.
		/// </param>
		/// <param name="nodeAdapter">
		/// The adapter that will adapt adapter candidate nodes returned be the
		/// <c>Resolve</c> and <c>ParentDirectory</c> methods.
		/// </param>
		protected NodeDelegationWrapper(INode innerNode, INodeResolver resolver, Converter<INode, INode> nodeAdapter)
			: base(innerNode)
		{
			if (resolver == null)
			{
				if (this.NodeType == NodeType.Directory)
				{
					resolver = new DefaultResolver(this);
				}
				else
				{
					resolver = new DefaultResolver(this);
				}
			}

			this.NodeResolver = resolver;
			this.NodeAdapter = nodeAdapter;
		}

		/// <summary>
		/// Overrides and returns the current node delegater's name.
		/// </summary>
		/// <remarks>
		/// This property delegates to <c>this.Address.Name</c>
		/// </remarks>
		public override string Name
		{
			get
			{
				return this.Address.Name;
			}
		}

		/// <summary>
		/// Overrides and returns the current node's parent directory
		/// optionally adapting the return result using the current
		/// object's <see cref="NodeAdapter"/>.
		/// </summary>
		public override IDirectory ParentDirectory
		{
			get
			{
				return (IDirectory)NodeAdapter(this.Wrappee.ParentDirectory);
			}
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override IFile ResolveFile(string name)
		{
			return this.NodeResolver.ResolveFile(name);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override IDirectory ResolveDirectory(string name)
		{
			return this.NodeResolver.ResolveDirectory(name);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override IFile ResolveFile(string name, AddressScope scope)
		{
			return this.NodeResolver.ResolveFile(name, scope);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override IDirectory ResolveDirectory(string name, AddressScope scope)
		{
			return this.NodeResolver.ResolveDirectory(name, scope);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override INode Resolve(string name)
		{
			return this.NodeResolver.Resolve(name);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override INode Resolve(string name, AddressScope scope)
		{
			return this.NodeResolver.Resolve(name, scope);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override INode Resolve(string name, NodeType nodeType)
		{
			return this.NodeResolver.Resolve(name, nodeType);
		}

		/// <summary>
		/// Resolves a file using the current object's <see cref="NodeResolver"/>.
		/// </summary>
		/// <remarks>
		/// The default implementation delegates to
		/// <see cref="Resolve(string, NodeType, AddressScope)"/>.
		/// </remarks>
		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			return this.NodeResolver.Resolve(name, nodeType, scope);
		}

		/// <summary>
		/// Overrides and returns the current node's
		/// <see cref="INode.OperationTargetDirectory"/>
		/// and adaptes the return result using the current
		/// object's <see cref="NodeAdapter"/>.
		/// </summary>
		public override IDirectory OperationTargetDirectory
		{
			get
			{
				return (IDirectory)NodeAdapter(this.Wrappee.OperationTargetDirectory);
			}
		}

		/// <summary>
		/// Simulates delegation by comparing the delegator's (this) <c>INodeAddress</c>
		/// and <c>NodeType</c>.
		/// </summary>
		/// <remarks>
		/// Uses <c>this.Address</c> and <c>this.NodeType</c>.
		/// </remarks>
		public override bool Equals(object obj)
		{
			var node = obj as INode;

			if (obj == null)
			{
				return false;
			}

			if (obj == this)
			{
				return true;
			}

			return this.NodeType.Equals(node.NodeType) && this.Address.Equals(node.Address);
		}

		/// <summary>
		/// Gets the hashcode based on the delegator's (this) <c>INodeAddress</c>.
		/// </summary>
		public override int GetHashCode()
		{
			return this.Address.GetHashCode();
		}

		/// <summary>
		/// Returns a string representation of the object using the delegator's (this) <c>INodeAddress</c>.
		/// </summary>
		public override string ToString()
		{
			return this.Address.ToString();
		}

		public override int CompareTo(INode other)
		{
			return System.String.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override void CheckAccess(FileSystemSecuredOperation operation)
		{
			if (!this.FileSystem.SecurityManager.CurrentContext.HasAccess
			(
				new AccessVerificationContext(this, operation)
			))
			{
				throw new FileSystemSecurityException(this.Address);
			}
		}

		public override INode GetDirectoryOperationTargetNode(IDirectory directory)
		{
			return NodeAdapter(this.Wrappee.GetDirectoryOperationTargetNode(directory));
		}
	}
}