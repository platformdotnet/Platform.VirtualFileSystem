using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Represents mutable attributes of a node.
	/// </summary>
	public interface INodeAttributes
		: IRefreshable, ISyncLocked, IEnumerable<KeyValuePair<string, object>>
	{
		/// <summary>
		/// Aquires a context to use for updating a the current attributes object.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method locks the attributes object and prevents other threads
		/// from accessing the attributes object.  When the update context
		/// is released, the attribute object is also unlocked.
		/// </para>
		/// <para>
		/// Updates to individual attribute values in the object are not guaranteed
		/// to be reflected on the underlying file system until the context is
		/// released.  This loose contractual specification allows update contexts
		/// to be used to group updates to multiple attributes on a remote file into
		/// a single operation.
		/// </para>
		/// <para>
		/// To release (unaquire) the update context, call dispose on the returned
		/// object.  This can be easily accomplished by utilising the <c>using</c>
		/// keyword in C#.
		/// </para>
		/// <example>
		/// <code>
		/// IFile file = FileSystemManaget.GetManager().ResolveFile("c:/test.txt");
		/// 
		/// using (file.Attributes.AquireUpdateContext())
		/// {
		///		file.Attributes.CreationTime = DateTime.Now;
		/// 	file.Attributes.LastWriteTime = DateTime.Now;
		/// }
		/// </code>
		/// </example>
		/// </remarks>
		/// <returns></returns>
		INodeAttributesUpdateContext AquireUpdateContext();

		/// <summary>
		/// Gets/Sets whether a file is readonly.
		/// </summary>
		bool? ReadOnly
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/Sets whether a file is hidden.
		/// </summary>
		bool? IsHidden
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the creation time.
		/// </summary>
		DateTime? CreationTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/sets the last access time.
		/// </summary>
		DateTime? LastAccessTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets/Sets the last write time.
		/// </summary>
		DateTime? LastWriteTime
		{
			get;
			set;
		}

		/// <summary>
		/// Gets whether this node exists or not.
		/// </summary>
		bool Exists
		{
			get;
		}

		/// <summary>
		/// Gets or Sets an attribute by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		object this[string name]
		{
			get;
			set;
		}
		
		/// <summary>
		/// Refresh the attributes of this node.
		/// </summary>
		new INodeAttributes Refresh();

		/// <summary>
		/// Get an enumeration of the names of the atttributes of this node.
		/// </summary>
		IEnumerable<string> Names
		{
			get;
		}

		/// <summary>
		/// Get an enumeration of the values of the atttributes of this node.
		/// </summary>
		IEnumerable<object> Values
		{
			get;
		}
		
		/// <summary>
		/// Checks if this node supports the given attribute by name.
		/// </summary>
		/// <param name="name">
		/// The name of the attribute
		/// </param>
		/// <returns>
		/// True if the attribute is supported; otherwise false.
		/// </returns>
		bool Supports(string name);
	}
}
