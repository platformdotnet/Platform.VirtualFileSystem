using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>IResolver</c> interface to minimize the
	/// effort required to implement the <see cref="IResolver"/> interface.
	/// </summary>
	public abstract class AbstractResolver
		: INodeResolver
	{
		public virtual IDirectory ResolveDirectory(string name)
		{
			return ResolveDirectory(name, AddressScope.FileSystem);
		}

		public virtual IFile ResolveFile(string name)
		{
			return ResolveFile(name, AddressScope.FileSystem);
		}

		public virtual IDirectory ResolveDirectory(string name, AddressScope scope)
		{
			return (IDirectory)Resolve(name, NodeType.Directory, scope);
		}
		
		public virtual IFile ResolveFile(string name, AddressScope scope)
		{
			return (IFile)Resolve(name, NodeType.File, scope);
		}

		public virtual INode Resolve(string name)
		{
			return Resolve(name, AddressScope.FileSystem);
		}

		public virtual INode Resolve(string name, AddressScope scope)
		{
			return Resolve(name, NodeType.Any, scope);
		}

		public virtual INode Resolve(string name, NodeType nodeType)
		{
			return Resolve(name, nodeType, AddressScope.FileSystem);
		}

		public abstract INode Resolve(string name, NodeType nodeType, AddressScope scope);
	}
}
