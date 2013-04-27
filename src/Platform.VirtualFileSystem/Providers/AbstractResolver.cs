using System;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// Summary description for AbstractResolver.
	/// </summary>
	public abstract class AbstractResolver
		: MarshalByRefObject, IResolver
	{
		public virtual IDirectory ResolveDirectory(string name)
		{
			return ResolveDirectory(name, NameScope.FileSystem);
		}

		public virtual IFile ResolveFile(string name)
		{
			return ResolveFile(name, NameScope.FileSystem);
		}

		public virtual IDirectory ResolveDirectory(string name, NameScope scope)
		{
			return (IDirectory)Resolve(name, NodeType.Directory, scope);
		}
		
		public virtual IFile ResolveFile(string name, NameScope scope)
		{
			return (IFile)Resolve(name, NodeType.File, scope);
		}

		/// <summary>
		/// <see cref="INode.Resolve(string)"/>
		/// </summary>
		public virtual INode Resolve(string name)
		{
			return Resolve(name, NameScope.FileSystem);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="scope"></param>
		/// <returns></returns>		
		public virtual INode Resolve(string name, NameScope scope)
		{
			return Resolve(name, NodeType.Any, scope);
		}

		/// <summary>
		/// <see cref="INode.Resolve(string, NodeType)"/>
		/// </summary>		
		public virtual INode Resolve(string name, NodeType nodeType)
		{
			return Resolve(name, nodeType, NameScope.FileSystem);
		}

		/// <summary>
		/// <see cref="INode.Resolve(string, NodeType, NameScope)"/>
		/// </summary>
		/// <remarks>
		/// The default implementation validates the NameScope by calling IsScopeValid(Uri, NameScope)
		/// and then calls the current object's FileSystem's <c>Resolve(string, NodeType)</c>
		/// method.
		/// </remarks>		
		public abstract INode Resolve(string name, NodeType nodeType, NameScope scope);
	}
}
