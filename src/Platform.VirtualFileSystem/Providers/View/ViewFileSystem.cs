using System;

namespace Platform.VirtualFileSystem.Providers.View
{
	public class ViewFileSystem
		: AbstractFileSystem
	{
		protected internal virtual IDirectory ForiegnRoot
		{
			get
			{
				return this.foriegnRoot;
			}
		}
		private readonly IDirectory foriegnRoot;

		/// <summary>
		/// Adapts a node belonging to the underlying file system
		/// to a node belonging to current <c>ViewFileSystem</c>.
		/// </summary>
		/// <param name="node">
		/// The node of the underlying file system for this <c>ViewFileSystem</c>.
		/// </param>
		/// <returns>
		/// The equivalent viewed node from the current file system.
		/// </returns>
		protected internal virtual INode ViewNodeAdapter(INode node)
		{
			var root = this.ForiegnRoot;
			var relativePath = root.Address.GetRelativePathTo(node.Address.AbsolutePath);

			var wrapped = this.Resolve(relativePath, node.NodeType);

			return wrapped;
		}

		public ViewFileSystem(string scheme, IDirectory root)
			: this(scheme, root, FileSystemOptions.Default)
		{
		}

		public ViewFileSystem(string scheme, IDirectory root, FileSystemOptions options)
			: base(ViewNodeAddress.Parse(scheme + ":///"), null, options)
		{
			this.foriegnRoot = root;
		}

		public ViewFileSystem(IDirectory root, INodeAddress rootAddress, IFile parentLayer, FileSystemOptions options)
			: base(rootAddress, parentLayer, options)
		{
			this.foriegnRoot = root;
		}

		protected override INode CreateNode(INodeAddress address, NodeType nodeType)
		{
			var retval = this.foriegnRoot.Resolve("." + address.PathAndQuery, nodeType, AddressScope.DescendentOrSelf);

			if (retval.NodeType.Is(NodeType.File))
			{
				retval = new ViewFile(this, (ViewNodeAddress)address, (IFile)retval);
			}
			else if (retval.NodeType.Is(NodeType.Directory))
			{
				retval = new ViewDirectory(this, (ViewNodeAddress)address, (IDirectory)retval);
			}
			else
			{
				throw new NotSupportedException(retval.NodeType.ToString());
			}

			return retval;
		}
	}
}
