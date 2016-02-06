using System;

namespace Platform.VirtualFileSystem
{
	public interface IFileSystemManager
		: INodeResolver
	{
		/// <summary>
		/// Adds a file system to the manager. May throw an exception if this is not supported by the manager.
		/// </summary>
		/// <exception cref="NotSupportedException">The FileSystemManager does not support dynamically adding file systems</exception>
		/// <param name="fileSystem"></param>
		void AddFileSystem(IFileSystem fileSystem);

		/// <summary>
		/// Resolves a node using the supplied <c>FileSystemOptions</c>.
		/// </summary>
		/// An error occured while trying to resolve the node.
		INode Resolve(string uri, NodeType nodeType, AddressScope scope, FileSystemOptions options = null);

		/// <summary>
		/// Resolves a file system given the URL to the file system's root directory
		/// </summary>
		IFileSystem ResolveFileSystem(string uri, FileSystemOptions options = null);

		/// <summary>
		/// Closes all file systems managed by the current object
		/// </summary>
		void CloseAllFileSystems();
	}
}