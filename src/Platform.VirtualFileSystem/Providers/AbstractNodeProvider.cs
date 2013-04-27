using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>INodeProvider </c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="INodeProvider"/>
	/// </summary>
	public abstract class AbstractNodeProvider
		: MarshalByRefObject, INodeProvider
	{
		public virtual IFileSystemManager Manager { get; private set; }

		/// <summary>
		/// <see cref=" INodeProvider.SupportedUriSchemas"/>
		/// </summary>
		public abstract string[] SupportedUriSchemas
		{
			get;
		}

		protected AbstractNodeProvider(IFileSystemManager manager)
		{
			this.Manager = manager;
		}

		public abstract INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options);

		public virtual IFileSystem CreateNewFileSystem(string scheme, IFile destination, FileSystemOptions options)
		{
			throw new NotSupportedException
			(
				new System.Diagnostics.StackFrame().GetMethod().Name + "|NotSupported"
			);
		}

		public virtual bool SupportsUri(string uri)
		{
			var schema = uri.SplitAroundFirstStringFromLeft("://").Left;

			return Array.Exists<string>
			(
				this.SupportedUriSchemas,
				s => schema.Equals(s, StringComparison.CurrentCultureIgnoreCase)
			);
		}
	}	
}