using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Platform.Text;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers.View
{
	public class ViewNodeProvider
		: AbstractNodeProvider
	{
		[XmlElement("Options")]
		public class Configuration
			: NodeProviderConfiguration
		{
			[XmlElement, XmlVariableSubstitution]
			public virtual string Scheme { get; protected set; }

			[XmlElement, XmlVariableSubstitution]
			public virtual string Uri { get; protected set; }

			public virtual bool Create { get; protected set; }

			public Configuration()
			{
				Create = false;
				Uri = Path.GetTempPath();
				Scheme = "";
			}

			public Configuration(string scheme, string uri, bool create)
			{
				this.Uri = uri;
				this.Scheme = scheme;
				this.Create = create;
			}
		}

		private readonly string[] schemes;
		private readonly IFileSystem viewFileSystem;

		public virtual IFileSystem ViewFileSystem
		{
			get
			{
				return this.viewFileSystem;
			}
		}

		private static IDirectory ResolveRootDirectory(INodeResolver resolver, string uri, bool create)
		{
			try
			{
				return create ? resolver.ResolveDirectory(uri).Create(true) : resolver.ResolveDirectory(uri);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);

				throw;
			}
		}

		public ViewNodeProvider(IFileSystemManager fileSystemManager, Configuration config)
			: this
			(
				fileSystemManager,
				config.Scheme,
				ResolveRootDirectory(fileSystemManager, config.Uri, config.Create),
				FileSystemOptions.Default,
				config
			)
		{
		}

		public ViewNodeProvider(IFileSystemManager fileSystemManager, string scheme, IDirectory root)
			: this(fileSystemManager, scheme, root, FileSystemOptions.Default)
		{
		}

		public ViewNodeProvider(IFileSystemManager fileSystemManager, string scheme, IDirectory root, FileSystemOptions options)
			: this(fileSystemManager, scheme, root, options, null)
		{
		}

		public ViewNodeProvider(IFileSystemManager fileSystemManager, string scheme, IDirectory root, FileSystemOptions options, Configuration config)
			: this(fileSystemManager, CreateViewFileSystem(scheme, root, options, config))
		{
		}

		private static IFileSystem CreateViewFileSystem(string scheme, IDirectory root, FileSystemOptions options, Configuration config)
		{
			if (scheme.IsNullOrEmpty())
			{
				throw new ArgumentException(String.Empty, "scheme");
			}

			if (config != null)
			{
				options = options.CreateWithAdditionalConfig(config);
			}

			var fileSystem = root.CreateView(scheme, options);
			
			return fileSystem;
		}


		public ViewNodeProvider(IFileSystemManager fileSystemManager, IFileSystem fileSystem)
			: base(fileSystemManager)
		{			
			this.schemes = new string[]
			{
				fileSystem.RootDirectory.Address.Scheme
			};

			this.viewFileSystem = fileSystem;			
		}

		public override string[] SupportedUriSchemas
		{
			get
			{
				return (string[])this.schemes.Clone();
			}
		}

		public override INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options)
		{
			var result = uri.SplitAroundFirstStringFromLeft("://");

			if (result.Left != this.viewFileSystem.RootDirectory.Address.Scheme)
			{
				throw new NotSupportedException(result.Left);
			}

			return this.viewFileSystem.Resolve(TextConversion.FromEscapedHexString(result.Right), nodeType);
		}
	}
}
