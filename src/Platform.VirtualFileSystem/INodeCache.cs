using System.Collections;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for VFS node caches.
	/// </summary>
	/// <remarks>
	/// This is a generic interface for <see cref="INode"/> caches.  Implementers must guarantee
	/// that the class is thread safe.
	/// </remarks>
	public interface INodeCache
		: IEnumerable, ISyncLocked
	{
		/// <summary>
		/// Adds a node to the cache
		/// </summary>
		/// <param name="node"></param>
		void Add(INodeAddress address, INode node);

		/// <summary>
		/// Gets a node from its address
		/// </summary>		
		/// <param name="address">The address of the node</param>
		/// <param name="nodeType">The type of node</param>
		INode Get(INodeAddress address, NodeType nodeType);

		/// <summary>
		/// Purge the cache of all nodes
		/// </summary>
		void Purge();

		/// <summary>
		/// Purge a single node from the cache
		/// </summary>
		/// <param name="node">The node to purge</param>
		void Purge(INodeAddress address, NodeType nodeType);

		void PurgeWithDescendents(INodeAddress address, NodeType nodeType);
	}
}
