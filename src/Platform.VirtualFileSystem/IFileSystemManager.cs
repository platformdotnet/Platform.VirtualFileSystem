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
		/// <param name="name">The name of the node to look for.</param>
		/// <param name="nodeType">The <see cref="NodeType"/> of node to look for.</param>
		/// <param name="scope">The <see cref="AddressScope"/> of the node.</param>
		/// <returns></returns>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the node.
		/// </exception>
		INode Resolve(string uri, NodeType nodeType, AddressScope scope, FileSystemOptions options);

		/// <summary>
		/// Closes all file systems managed by the current object
		/// </summary>
		void CloseAllFileSystems();
	}
}