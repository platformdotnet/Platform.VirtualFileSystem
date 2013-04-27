using System;
using System.Collections;
using System.Reflection;
using Platform;
using Platform.Text;
using Platform.Collections;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public class StandardFileSystemManager
		: AbstractFileSystemManager
	{
		private readonly ILList<INodeProvider> providers;

		public override void CloseAllFileSystems()
		{
			foreach (var provider in providers)
			{
				if (provider is Providers.View.ViewNodeProvider)
				{
					((Providers.View.ViewNodeProvider) provider).ViewFileSystem.Close();
				}
			}
		}

		public StandardFileSystemManager()
		{
			providers = new ArrayList<INodeProvider>();			
		}

		protected virtual INodeProvider CreateProvider(ConfigurationSection.NodeProviderConfigEntry entry)
		{
			try
			{
				var options = entry.LoadConfiguration();

				if (options == null)
				{
					return (INodeProvider)Activator.CreateInstance(entry.Type, new object[] { this });
				}
				else
				{
					return (INodeProvider)Activator.CreateInstance(entry.Type, new object[] { this, options });
				}
			}
			catch (MissingMethodException)
			{
			}

			try
			{
				return (INodeProvider)Activator.CreateInstance(entry.Type);
			}
			catch (MissingMethodException)
			{
				throw new NotSupportedException(entry.Type.ToString());
			}
		}

		/// <summary>
		/// Add a file system provider to this manager.
		/// </summary>
		/// <remarks>
		/// When a provider is added, its <c>SupportedUriSchemas</c> property is used to determine
		/// which resolution requests will be forwarded to the provider.  If more than one provider
		/// support the same schema, the most recently added provider will be used.
		/// </remarks>
		/// <param name="provider"></param>
		public virtual void AddProvider(INodeProvider provider)
		{
			providers.Add(provider);
		}

		public virtual void AddFileSystem(IFileSystem fileSystem)
		{
			var provider = new Providers.View.ViewNodeProvider(this, fileSystem);

			providers.Add(provider);
		}

		public virtual void AddFileSystem(IFileSystem fileSystem, string scheme)
		{
			var provider = new Providers.View.ViewNodeProvider(this, fileSystem);

			providers.Add(provider);
		}

		public override INode Resolve(string uri, NodeType nodeType, AddressScope scope, FileSystemOptions options)
		{						
			if (uri.IsNullOrEmpty())
			{
				if (uri == null)
				{
					throw new ArgumentNullException("uri");
				}
				else
				{
					throw new ArgumentException("uri");
				}
			}

			if (uri[0] == ' ')
			{
				uri = uri.Trim();
			}

			if (uri.Length == 0)
			{
				throw new ArgumentException(uri, "uri");
			}
						
			foreach (var provider in providers)
			{
				if (provider.SupportsUri(uri))
				{
					return provider.Find(this, uri, nodeType, options);
				}
			}

			throw new NotSupportedException(uri);
		}
	}
}