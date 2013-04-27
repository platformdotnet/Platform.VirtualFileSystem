using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for <i>nodes</i> within a file system.
	/// </summary>
	public interface INode
		: INamed, INodeResolver, ISyncLocked, IComparable<INode>
	{		
		event NodeActivityEventHandler Renamed;
		event NodeActivityEventHandler Created;
		event NodeActivityEventHandler Deleted;
		event NodeActivityEventHandler Changed;
		event NodeActivityEventHandler Activity;

		void CheckAccess(FileSystemSecuredOperation operation);

		bool SupportsActivityEvents
		{
			get;
		}

		void CreateGlobalLock(string name);

		/// <summary>
		/// Gets the type of this node.
		/// </summary>
		NodeType NodeType
		{
			get;
		}

		/// <summary>
		/// Gets the name of this node.
		/// </summary>		
		new string Name
		{
			get;
		}
		
		/// <summary>
		/// Gets the <c>INodeAddress</c> for this node.
		/// </summary>
		INodeAddress Address
		{
			get;
		}

		/// <summary>
		/// Gets the owner FileSystem.
		/// </summary>
		/// <remarks>
		/// If this object is a FileSystem node then this property is null.
		/// </remarks>		
		IFileSystem FileSystem
		{
			get;
		}

		/// <summary>
		/// Gets the parent directory of this node or null if the current node is the root directory.
		/// </summary>
		/// <remarks>
		/// If this node isn't contained in a directory (e.g. it is a filesystem or the root directory)
		/// then null is returned.
		/// </remarks>
		IDirectory ParentDirectory
		{
			get;
		}

		/// <summary>
		/// Gets an array of the alternate files for this object.
		/// </summary>
		/// <remarks>
		/// Some file systems support alternate files.  On NTFS, a file may have multiple streams
		/// with the same name and <c>overlayed</c> virtual file systems may have more than one 
		/// file for a given name.  This method will return the current node as one of the alternates.
		/// </remarks>
		/// <returns>The array of alternates</returns>
		IEnumerable<INode> GetAlternates();

		/// <summary>
		///  Moves this node to a target node.
		/// </summary>
		/// <param name="target">The target node to move to which must be the same type as the current node</param>
		/// <param name="overwrite">True if the target node should be overwriten if it exists</param>
		/// <returns>The current object</returns>
		INode MoveTo(INode target, bool overwrite);

		/// <summary>
		/// Copies this node to a target node.
		/// </summary>
		/// <param name="target">The target node to copy to which must be the same type as the current node</param>
		/// <returns>The current object</returns>
		INode CopyTo(INode target, bool overwrite);

		/// <summary>
		/// Moves this node to the directory.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="overwrite"></param>
		/// <returns>The current object</returns>
		INode MoveToDirectory(IDirectory target, bool overwrite);

		/// <summary>
		/// Copies this node to the directory.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="overwrite"></param>
		/// <returns>The current object</returns>
		INode CopyToDirectory(IDirectory target, bool overwrite);

		/// <summary>
		/// Renames this node to the given name.
		/// </summary>
		/// <param name="name">
		/// The new name for the node.
		/// </param>
		/// <remarks>
		/// The name can not contain any paths relative or otherwise.  Use <see cref="RenamedTo(INode, bool)"/>
		/// if you wish to readdress the node.
		/// </remarks>
		/// <param name="overwrite">
		/// Overwrite any existing file with the same name as the new name.
		/// </param>
		/// <returns>The current object</returns>
		INode RenameTo(string name, bool overwrite);

		/// <summary>
		/// Create the node.
		/// </summary>
		/// <remarks>
		/// The exact semantics of node creation depends on the type of node and filesystem.
		/// </remarks>
		/// <returns>The current object</returns>
		INode Create();

		/// <summary>
		/// Create the node on the target file system.
		/// </summary>
		/// <remarks>
		/// The effect this method has depends on the current node type.
		/// For example, if the node is a file, an empty file will be created.
		/// </remarks>
		/// <param name="createParent">
		/// If true, the parent directories of this node will be created if they don't exist.
		/// </param>
		/// <exception cref="System.IO.DirectoryNotFoundException">
		/// The parent directory does not exist.
		/// </exception>
		/// <returns>The current object</returns>
		INode Create(bool createParent);

		/// <summary>
		/// Delete the node on the target file system.
		/// </summary>
		/// <remarks>
		/// The effect this method has depends on the current node type.
		/// For example, if the node is a file, the file will be deleted.
		/// </remarks>
		/// <returns>The current object</returns>
		INode Delete();

		/// <summary>
		/// Gets an appropriate target directory when this node is used as the target of 
		/// a move, copy or other operation.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Implementations of operations such as move or copy require a target 
		/// directory node.
		/// The VirtualFileSystem allows non directory nodes to be the target of such
		/// operations.  For example, if you copy file1 into symboliclink1 then file1 will be 
		/// copied into symboliclink1's target.
		/// When required, the VirtualFileSystem will call this method on target node so 
		/// that the node can provide an appropriate directory node.  
		/// </p>
		/// <p>
		/// By default, directory nodes will always return a reference to themselves; 
		/// File nodes will return a reference to their parent and SymbolicLink
		/// nodes will return a reference to their target's OperationTargetDirectory.
		/// </p>
		/// <p>
		/// Some implementations may return directories within foreign file systems.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The node can not provide an operation target directory.
		/// </exception>
		IDirectory OperationTargetDirectory
		{
			get;
		}

		IService GetService(ServiceType serviceType);

		T GetService<T>()
			where T : IService;

		/// <summary>
		/// Autocasting version of GetService.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		T GetService<T>(ServiceType serviceType)
			where T : IService;
		
		/// <summary>
		/// Get the details (timestamps and other state mutable information). 
		/// </summary>
		INodeAttributes Attributes
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>The current object</returns>
		INode Refresh();

		bool Exists
		{
			get;
		}

		/// <summary>
		/// The name of the default content.
		/// </summary>
		string DefaultContentName
		{
			get;
		}

		/// <summary>
		/// Get an enumeration of the names of the available contents for this node.
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> GetContentNames();

		/// <summary>
		/// Gets the default file content of node.
		/// </summary>
		/// <remarks>
		/// Usually only files contain default content.
		/// </remarks>
		/// <returns></returns>
		INodeContent GetContent();

		/// <summary>
		/// Gets the content of this file given an alternate content name.
		/// </summary>
		/// <param name="contentName">
		/// The name of the content or null for the default content.
		/// </param>
		/// <remarks>		
		/// <para>
		/// Some file systems (such as the local file system backed by NTFS)
		/// support alternate content or streams.  A single file or directory 
		/// may have more than one logical stream.  These streams can be used to 
		/// write alternate data such as thumbnails and previews of the main 
		/// file or custom extended attributes.
		/// </para>
		/// </remarks>
		/// <exception cref="NotSupportedException">
		/// Alternate streams are not supported by the file system.
		/// </exception>
		/// <returns>
		/// An <see cref="INodeContent"/> for the file content.
		/// </returns>
		INodeContent GetContent(string contentName);


		INode GetDirectoryOperationTargetNode(IDirectory directory);
	}
}
