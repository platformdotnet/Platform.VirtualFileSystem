using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Summary description for NodeDeletingServiceType.
	/// </summary>
	public class NodeDeletingServiceType
		: ServiceType
	{
		/// <summary>
		///  Gets/Sets whether the node should be recursively deleted.
		/// </summary>
		/// <remarks>
		/// This property only has effect if the node is a container node (i.e. a directory).
		/// </remarks>
		public virtual bool Recursive { get; set; }

		/// <summary>
		/// Create a node <see cref="NodeDeletingServiceType"/>.
		/// </summary>
		/// <remarks>
		/// The <c>Recursive</c> property is by default to false.
		/// </remarks>
		public NodeDeletingServiceType()
			: this(false)
		{
		}

		/// <summary>
		/// Create a node <see cref="NodeDeletingServiceType"/>.
		/// </summary>
		/// <param name="recursive">
		/// Sets whether the node should be deleted recursively.
		/// </param>
		public NodeDeletingServiceType(bool recursive)
			: base(typeof(INodeDeletingService))
		{
			this.Recursive = recursive;
		}
	}
}
