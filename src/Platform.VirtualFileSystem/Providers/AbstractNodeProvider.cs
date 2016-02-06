using System;
using System.Collections.Specialized;
using System.Linq;

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

		protected virtual FileSystemOptions AmmendOptionsFromAddress(INodeAddress address, FileSystemOptions options)
		{
			NameValueCollection variables = null;

			foreach (var key in address.QueryValues.Keys.Cast<string>().Where(key => this.SupportedUriSchemas.Any(c => key.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))))
			{
				if (variables == null)
				{
					variables = new NameValueCollection();
				}

				variables[key] = address.QueryValues[key];
			}

			return options.AddVariables(variables);
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
			var schema = uri.SplitOnFirst("://").Left;

			return Array.Exists<string>
			(
				this.SupportedUriSchemas,
				s => schema.Equals(s, StringComparison.CurrentCultureIgnoreCase)
			);
		}
	}	
}