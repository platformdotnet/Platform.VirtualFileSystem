using System;

namespace Platform.VirtualFileSystem
{
	public interface IFileSystemManager
		: INodeResolver
	{
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