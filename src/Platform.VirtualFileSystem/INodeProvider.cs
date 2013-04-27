using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for providers of nodes.
	/// </summary>
	public interface INodeProvider
	{
		/// <summary>
		/// Gets an array containing the schemas supported by this
		/// <c>NodeProvider</c>.
		/// </summary>
		string[] SupportedUriSchemas
		{
			get;
		}

		/// <summary>
		/// Checks if the node provider supports the supplied URI
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		bool SupportsUri(string uri);

		/// <summary>
		/// Resolve a node from the given URI.
		/// </summary>
		INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options);
		
		/// <summary>
		/// Create a new layered file system.
		/// </summary>
		/// <param name="scheme">
		/// The Uri schema for the target file system.  The scheme must be a scheme
		/// supported by the provider (<see cref="SupportedUriSchemas"/>).
		/// </param>
		/// <param name="destination"></param>
		/// <exception cref="NotSupportedException">
		/// Creating a layered file system of the specified scheme isn't supported.
		/// </exception>
		IFileSystem CreateNewFileSystem(string scheme, IFile destination, FileSystemOptions options);
	}	
}
