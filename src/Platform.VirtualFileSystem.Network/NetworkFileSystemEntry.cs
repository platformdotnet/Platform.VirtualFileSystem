using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Network
{
	public struct NetworkFileSystemEntry
	{
		public string Name { get; private set; }

		public NodeType NodeType { get; private set; }

		/// <summary>
		/// Reads the attributes of the current directory entry.
		/// </summary>		
		/// <returns>
		/// An <see cref="IEnumerable"/> capable of iterating over the
		/// attributes of the current directory entry.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Implementations are free to return attributes as they are
		/// read 'on the fly'.  Because of this, the returned
		/// <see cref="IEnumerable"/> can only be iterated once and
		/// and only during the iteration of the parent's directory.
		/// This is called the <i>iteration context</i>.
		/// </para>
		/// <para>
		/// Calling this method more than once will <see cref="IEnumerable"/>
		/// objects with invalid iteration contexts.
		/// The returned <see cref="IEnumerable"/> will throw an
		/// <see cref="InvalidOperationException"/> if it is used outside
		/// of its iteration context.
		/// </para>
		/// <para>
		/// The order of the return attributes is undefined.
		/// </para>
		/// </remarks>
		public IEnumerable<Pair<string, object>> ReadAttributes()
		{
			return this.attributes;
		}
		private readonly IEnumerable<Pair<string, object>> attributes;

		public NetworkFileSystemEntry(string name, NodeType nodeType, IEnumerable<Pair<string, object>> attributes) : this()
		{
			this.Name = name;
			this.NodeType = nodeType;
			this.attributes = attributes;
		}
	}
}
