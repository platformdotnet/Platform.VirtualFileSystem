using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for the filesystem.
	/// </summary>
	/// <remarks>
	/// This class is analogous to the XmlDocument class.
	/// </remarks>
	public interface IFileSystem	
		: INodeResolver, IDisposable
	{
        bool IsDisposed
        {
            get;
        }

		/// <summary>
		/// Raised when the file system is closed.
		/// </summary>
		event EventHandler Closed;

		/// <summary>
		/// Get the lock for this <c>FileSystem</c>.
		/// </summary>
		object SyncLock
		{
			get;
		}

		/// <summary>
		/// Aquire and return the <see cref="AutoLock"/> for this <c>FileSystem</c>.
		/// </summary>
		AutoLock AquireAutoLock();
		
		/// <summary>
		/// Raised when there is activity in the file system.
		/// </summary>
		event FileSystemActivityEventHandler Activity;

		int MaximumPathLength
		{
			get;
		}

		/// <summary>
		/// True if the file system is capable of raising activity events.
		/// </summary>
		bool SupportsActivityEvents
		{
			get;
		}

		/// <summary>
		/// True if the file system provides seekable streams.
		/// </summary>
		/// <remarks>
		/// This value is only a generic hint.  Individual files within the file system
		/// may have differing seekablility.  The <c>CanSeek</c> property of a stream
		/// can be used to properly verify that an individual stream is seekable.
		/// </remarks>
		bool SupportsSeeking
		{
			get;
		}

		/// <summary>
		/// Gets the root directory of the file system.
		/// </summary>
		IDirectory RootDirectory
		{
			get;
		}

		bool PathsEqual(string path1, string path2, int length);

		/// <summary>
		/// Gets a service for the specified node.
		/// </summary>
		/// <param name="node">The node to get the service for</param>
		/// <param name="serviceType">The <see cref="ServiceType"/></param>
		/// <returns>The service if supported</returns>
		/// <exception cref="NotSupportedException">
		/// The node does not support the specified <see cref="ServiceType"/>.
		/// </exception>
		IService GetService(INode node, ServiceType serviceType);

		/// <summary>
		/// Gets the <c>NodeType</c> of the node at the given path if it exists.
		/// </summary>
		/// <param name="path"></param>
		/// <returns><c>NodeType.None if the node doesn't exist</c></returns>
		NodeType GetNodeType(string path);
		
		/// <summary>
		/// Returns true if a node with the given path and <see cref="NodeType"/> exists.
		/// </summary>
		/// <param name="path">
		/// The path to the node to check.
		/// </param>
		/// <param name="nodeType">
		/// The <see cref="NodeType"/> to check.
		/// </param>
		/// <returns>
		/// True if the node of the specified type at the given path exists.
		/// </returns>		
		bool NodeExists(string path, NodeType nodeType);

		/// <summary>
		/// Gets the options for this <see cref="IFileSystem"/> instance.
		/// </summary>
		FileSystemOptions Options
		{
			get;
		}

		/// <summary>
		/// Get the extenders created and use by this file system.
		/// </summary>
		/// <remarks>
		/// Extenders to be used are defined in the file system <see cref="Options"/>.
		/// </remarks>
		FileSystemExtenders Extenders
		{
			get;
		}

		/// <summary>
		/// Resolve a node in the file system.
		/// </summary>
		/// <param name="address">The address of the node to resolve.</param>
		/// <param name="nodeType">The type of the node to resolve.</param>
		/// <returns></returns>
		INode Resolve(INodeAddress address, NodeType nodeType);

		/// <summary>
		/// Releases all resources used by the file system.
		/// </summary>
		void Close();

		/// <summary>
		/// Gets the default security manage for this file system.
		/// </summary>
		FileSystemSecurityManager SecurityManager
		{
			get;
		}

		bool HasAccess(INode node, FileSystemSecuredOperation operation);
		void CheckAccess(INode node, FileSystemSecuredOperation operation);
	}
}
