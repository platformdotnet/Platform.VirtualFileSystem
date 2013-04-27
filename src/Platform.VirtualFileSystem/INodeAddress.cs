using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Represents the name of a node in the file system.
	/// </summary>
	public interface INodeAddress
		: INamed
	{
		string InnerUri
		{
			get;
		}

		new string Name
		{
			get;
		}

		string NameAndQuery
		{
			get;
		}

		string NameWithoutQuery
		{
			get;
		}

		bool IsRoot
		{
			get;
		}

		/// <summary>
		/// Gets the short name of the file.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The <c>ShortName</c> is the name without the parent's path.
		/// </p>
		/// <code>
		/// e.g. <c>/usr/local/myfile.txt</c>
		/// </code>
		/// </remarks>
		string ShortName
		{
			get;			
		}

		/// <summary>
		/// Gets the full path to the file.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The <c>AbsolutePath</c> is the fully absolute qualified path and short name.
		/// </p>
		/// <code>
		/// e.g. <c>/usr/local/myfile.txt</c>
		/// </code>
		/// </remarks>
		string AbsolutePath
		{
			get;
		}

		/// <summary>
		/// Gets the file name extension.
		/// </summary>
		/// <remarks>
		/// The extension is the part after the last '.'.  Examples include <c>txt</c>,
		/// <c>exe</c>, <c>ini</c>.
		/// </remarks>
		string Extension { get; }


		/// <summary>
		/// Gets the file name without the file extension.
		/// </summary>
		/// <remarks>
		/// The extension is the part after the last '.'.  Examples include <c>txt</c>,
		/// <c>exe</c>, <c>ini</c>.
		/// </remarks>
		string NameWithoutExtension { get; }

		/// <summary>
		/// Gets the depth of the file name.
		/// </summary>
		/// <remarks>
		/// The depth of the root (/) is 0.  The depth of a child of the root is 1 and so on...
		/// </remarks>
		int Depth { get; }

		/// <summary>
		/// Gets the scheme of this file name.
		/// </summary>
		/// <remarks>
		/// Common schemes are <c>file</c>, <c>http</c>, <f>ftp</f>.
		/// </remarks>
		string Scheme { get; }

		/// <summary>
		/// Gets the URI to this file.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The URI includes the <see cref="FullName"/>, <see cref="Schema"/>and 
		/// file system specific elements.
		/// </p>
		/// <code>
		/// e.g. <c>ftp://localhost:21/usr/local/myfile.txt</c>
		/// </code>
		string Uri { get; }
		string RootUri { get; }
		string DisplayUri { get; }
		string Query { get; }
		NameValueCollection QueryValues { get;  }

		string PathAndQuery { get; }
		INodeAddress Parent { get; }
		INodeAddress ResolveAddress(string name);
		INodeAddress ResolveAddress(string name, AddressScope scope);

		string GetRelativePathTo(INodeAddress address);
		string GetRelativePathTo(string absolutePath);

		bool IsAncestorOf(INodeAddress address);
		bool IsDescendentOf(INodeAddress address);
		bool IsDescendentOf(INodeAddress address, AddressScope scope);
		bool IsDescendentOf(INodeAddress address, StringComparison comparisonType, AddressScope scope);

		bool RootsAreEqual(INodeAddress nodeAddress);
		bool ParentsEqual(INodeAddress nodeAddress);
		bool ParentsEqual(INodeAddress nodeAddress, StringComparison comparisonType);

		string PathToDepth(int depth);
	}
}
