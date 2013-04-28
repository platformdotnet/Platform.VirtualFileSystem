using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Class to help implementers of <c>IFileSystemManager</c>.
	/// </summary>
	/// <remarks>
	/// Because the nodes are resolved in the context or the root of the file system, the <c>AddressScope</c>
	/// argument of the resolve methods will always have the same effect as <c>AddressScope.FileSystem</c>.
	/// </remarks>
	public abstract class AbstractFileSystemManager
		: AbstractResolver, IFileSystemManager
	{
		private INodeCache nodeCache;

		protected AbstractFileSystemManager()
		{
			nodeCache = new DefaultNodeCache();
		}

		public virtual void CloseAllFileSystems()
		{
		}

		public virtual void AddFileSystem(IFileSystem fileSystem)
		{
			throw new NotSupportedException(string.Format("{0} does not support dynamically adding file systems", this.GetType()));
		}

		public IFileSystem CreateLayeredFileSystem(string provider, IFile file)
		{			
			throw new NotSupportedException("LayeredFileSystems of the type [" + provider + "] are not supported.");
		}

		public override INode Resolve(string uri, NodeType nodeType, AddressScope scope)
		{
			return Resolve(uri, nodeType, scope, null);
		}

		protected virtual void QueryResolutionFilters()
		{			
		}

		public abstract INode Resolve(string uri, NodeType nodeType, AddressScope scope, FileSystemOptions options);
	}
}