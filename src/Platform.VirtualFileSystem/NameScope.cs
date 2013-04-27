namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// An enumeration of scoping policies used when resolving a file system node.
	/// </summary>
	public enum NameScope
	{
		/// <summary>
		/// Same as FileSystem except that an exception is thrown if the node is not
		/// a direct child of the current node.
		/// </summary>
		Child,

		/// <summary>
		/// Same as FileSystem except that an exception is thrown if the node is not
		/// a descendent of the current node.
		/// </summary>
		Descendent,

		/// <summary>
		/// Same as FileSystem except that an exception is thrown if the node is not
		/// a descendent of the current node or the current node.
		/// </summary>
		DescendentOrSelf,

		/// <summary>
		/// Resolve against file nodes in the same file system as the base node.
		/// </summary>
		FileSystem
	}
}
