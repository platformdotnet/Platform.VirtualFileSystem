using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Platform.VirtualFileSystem
{
	public class StandardFileSystemManager
		: AbstractFileSystemManager
	{
		private static readonly Regex fileSystemProviderRegex = new Regex(@"Platform\.VirtualFileSystem\.Providers\.([^\.]+)\.dll");

		private readonly IList<INodeProvider> providers;

		public override void CloseAllFileSystems()
		{
			foreach (var provider in providers)
			{
				(provider as Providers.View.ViewNodeProvider)?.ViewFileSystem.Close();
			}
		}

		public StandardFileSystemManager()
			: this(false)
		{
		}

		public StandardFileSystemManager(bool scanAssembliesForProviders)
		{
			providers = new List<INodeProvider>();	
		
			if (scanAssembliesForProviders)
			{
				ScanAssembliesAndAddProviders();
			}
		}

		public void ScanAssembliesAndAddProviders()
		{
			var location = new Uri(this.GetType().Assembly.GetName().CodeBase);

			try
			{
				if (string.IsNullOrEmpty(location.LocalPath))
				{
					return;
				}
			}
			catch (InvalidOperationException)
			{
				return;
			}

			var parent = Path.GetDirectoryName(location.LocalPath);

			foreach (var path in Directory.GetFiles(parent))
			{
				var fileName = Path.GetFileName(path);
				var match = fileSystemProviderRegex.Match(fileName);

				if (match.Success)
				{
					var assembly = Assembly.LoadFrom(Path.Combine(parent, fileName));

					foreach (var type in assembly.GetTypes())
					{
						if (typeof(INodeProvider).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
						{
							this.AddProvider((INodeProvider)Activator.CreateInstance(type, this));
						}
					}
				}
			}
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

		public override void AddFileSystem(IFileSystem fileSystem)
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
					throw new ArgumentNullException(nameof(uri));
				}
				else
				{
					throw new ArgumentException(nameof(uri));
				}
			}

			if (uri[0] == ' ')
			{
				uri = uri.Trim();
			}

			if (uri.Length == 0)
			{
				throw new ArgumentException(uri, nameof(uri));
			}
						
			foreach (var provider in this.providers.Where(provider => provider.SupportsUri(uri)))
			{
				return provider.Find(this, uri, nodeType, options);
			}

			throw new NotSupportedException(uri);
		}
	}
}