using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for objects that can resolve VFS nodes.
	/// </summary>
	public interface INodeResolver
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the file.
		/// </exception>
		IFile ResolveFile(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the directory.
		/// </exception>
		IDirectory ResolveDirectory(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the directory.
		/// </exception>
		IFile ResolveFile(string name, AddressScope scope);

		/// <summary>
		/// Resolves a directory within the file system.
		/// </summary>
		/// <param name="name">
		/// </param>
		/// <param name="scope">
		/// </param>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the directory.
		/// </exception>
		IDirectory ResolveDirectory(string name, AddressScope scope);

		/// <summary>
		/// Gets the node with the specified name.
		/// </summary>
		/// <remarks>
		/// <p>If the node doesn't exist, an exception is thrown (see exceptions documentation).</p>
		/// <p>Relative paths can only be resolved on directories.</p>
		/// <p>This method should behave the same as a call to <c>Resolve(string, NodeType.Any)</c></p>
		/// </remarks>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the node.
		/// </exception>
		INode Resolve(string name);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="scope"></param>
		/// <returns></returns>
		/// <remarks>
		/// <seealso cref="Resolve(string, NodeType)"/>
		/// <seealso cref="Resolve(string, NodeType, AddressScope)"/>
		/// </remarks>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the node.
		/// </exception>
		INode Resolve(string name, AddressScope scope);

		/// <summary>
		/// Resolves a node within the file system.
		/// </summary>
		/// <remarks>
		/// <p>If the node doesn't exist, a new node of the type <c>nodeType</c> is constructed and returned.</p>
		/// <p>Note: You will have to call <c>INode.Create</c> on the returned object to actually 
		/// create the file/directory.</p>
		/// <p>Relative paths can only be resolved on directories.</p>
		/// <p>If the <c>nodeType</c> is <see cref="NodeType.Any"/> then the method should 
		/// behave the same as the method <see cref="Resolve(string)"/></p>
		/// </remarks>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the node.
		/// </exception>
		INode Resolve(string name, NodeType nodeType);

		/// <summary>
		/// Resolves a node within the file system.
		/// </summary>
		/// <param name="name">The name of the node to look for.</param>
		/// <param name="nodeType">The <see cref="NodeType"/> of node to look for.</param>
		/// <param name="scope">The <see cref="AddressScope"/> of the node.</param>
		/// <returns></returns>
		/// <exception cref="VirtualFileSystemException">
		/// An error occured while trying to resolve the node.
		/// </exception>
		INode Resolve(string name, NodeType nodeType, AddressScope scope);
	}
}
