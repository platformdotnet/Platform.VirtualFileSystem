using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem
{
	public interface IDirectory
		: INode
	{
		IEnumerable<INode> Walk();
		IEnumerable<INode> Walk(NodeType nodeType);
		
		new IDirectory Create();
		new IDirectory Create(bool createParent);

		event NodeActivityEventHandler RecursiveActivity;
		event NodeActivityEventHandler DirectoryActivity;
		event JumpPointEventHandler JumpPointAdded;
		event JumpPointEventHandler JumpPointRemoved;

		INode AddJumpPoint(INode node);
		INode AddJumpPoint(string name, INode node);

		new IDirectory Refresh();
		IDirectory Refresh(DirectoryRefreshMask mask);

		/// <summary>
		/// Checks if a child exists within this directory.
		/// </summary>
		/// <remarks>
		/// This method only checks the existance of direct descendents.
		/// </remarks>
		/// <param name="name">The name of the child</param>
		/// <returns></returns>		
		bool ChildExists(string name);

		/// <summary>
		/// Gets the names of all child nodes within this directory.
		/// </summary>
		/// <remarks>
		/// In some file systems, it is possible that two nodes with the same name but different types exist.
		/// In such a case the nodes returned are selected in the order (most favoured -> least favoured):
		/// Files -> Directories -> Other.
		/// </remarks>
		IEnumerable<string> GetChildNames();

		/// <summary>
		/// Gets the names of all child nodes within this directory.
		/// </summary>
		/// <remarks>
		/// In some file systems, it is possible that two nodes with the same name but different types exist.
		/// In such a case the nodes returned are selected in the order (most favoured -> least favoured):
		/// Files -> Directories -> Other.
		/// </remarks>
		/// <param name="nodeType">The type of node to get.</param>
		/// <returns></returns>		
		IEnumerable<string> GetChildNames(NodeType nodeType);

		/// <summary>
		/// Gets the names of all child nodes within this directory.
		/// </summary>
		/// <remarks>
		/// If there are no children, an empty list is returned.
		/// </remarks>
		/// <param name="filter">
		/// The <see cref="IObjectFilter"/> to use when selecting the children (the filter is passed the node name).
		/// </param>
		IEnumerable<string> GetChildNames(Predicate<string> acceptNode);

		/// <summary>
		/// Gets the names of all child nodes within this directory.
		/// </summary>
		/// <remarks>
		/// If there are no children, an empty list is returned.
		/// </remarks>
		/// <param name="filter">
		/// The <see cref="IObjectFilter"/> to use when selecting the children (the filter is passed the node name).
		/// </param>
		/// <param name="nodeType">The type of node to get.</param>
		IEnumerable<string> GetChildNames(NodeType nodeType, Predicate<string> acceptName);

		/// <summary>
		/// Get all the files in this directory.
		/// </summary>
		IEnumerable<IFile> GetFiles();

		/// <summary>
		/// Get all the files in this directory.
		/// </summary>
		IEnumerable<IFile> GetFiles(Predicate<IFile> acceptFile);

		/// <summary>
		/// Get all the directories in this directory.
		/// </summary>
		IEnumerable<IDirectory> GetDirectories();

		/// <summary>
		/// Get all the directories in this directory.
		/// </summary>
		IEnumerable<IDirectory> GetDirectories(Predicate<IDirectory> acceptDirectory);
		
		/// <summary>
		/// Gets all the children of this directory.
		/// </summary>
		IEnumerable<INode> GetChildren();

		/// <summary>
		/// Gets all the children of this directory.
		/// </summary>
		IEnumerable<INode> GetChildren(NodeType nodeType);

		/// <summary>
		/// Gets all the children of this directory.
		/// </summary>
		/// <param name="filter">The filter to use.</param>
		/// <remarks>
		/// <p>Nodes can be either files or directories.</p>
		/// <p>If there are no children, an empty list is returned.</p>		
		/// </remarks>
		IEnumerable<INode> GetChildren(Predicate<INode> acceptNode);

		/// <summary>
		/// Gets all the children of this directory.
		/// </summary>
		/// <param name="filter">The filter to use.</param>
		/// <remarks>
		/// <p>Nodes can be either files or directories.</p>
		/// <p>If there are no children, an empty list is returned.</p>		
		/// </remarks>
		IEnumerable<INode> GetChildren(NodeType nodeType, Predicate<INode> acceptNode);

		/// <summary>
		/// Delete the directory and optionally recursively delete all sub directories.
		/// </summary>
		/// <param name="recursive">true if sub directories should be deleted</param>
		IDirectory Delete(bool recursive);		

		/// <summary>
		/// Creates a virtual file system with this directory as the root.
		/// </summary>
		/// <returns></returns>
		IFileSystem CreateView();

		/// <summary>
		/// Creates a virtual file system with this directory as the root.
		/// </summary>
		/// <returns></returns>
		IFileSystem CreateView(string scheme);

		/// <summary>
		/// Creates a virtual file system with this directory as the root.
		/// </summary>
		/// <returns></returns>
		IFileSystem CreateView(string scheme, FileSystemOptions options);

		/// <summary>
		/// Creates a virtual file system with this directory as the root.
		/// </summary>
		/// <returns></returns>
		IFileSystem CreateView(FileSystemOptions options);
	}
}
