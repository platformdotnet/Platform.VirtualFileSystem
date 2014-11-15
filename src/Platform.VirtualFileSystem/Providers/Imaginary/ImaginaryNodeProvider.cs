using System;
using Platform.Text;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers.Imaginary
{
	public class ImaginaryNodeProvider
		: AbstractNodeProvider
	{
		[XmlElement("Options")]
		public class ConstructionOptions
		{
			[XmlElement, XmlVariableSubstitution]
			public virtual string Scheme { get; protected set; }

			public ConstructionOptions()
			{
				Scheme = "";
			}

			public ConstructionOptions(string scheme)
			{
				this.Scheme = scheme;
			}
		}

		public override string[] SupportedUriSchemas
		{
			get
			{
				return (string[])this.schemas.Clone();
			}
		}
		private readonly string[] schemas;

		private readonly IFileSystem imaginaryFileSystem;

		protected virtual IFileSystem ImaginaryFileSystem
		{
			get
			{
				return this.imaginaryFileSystem;
			}
		}

		private IDirectory root;

		public ImaginaryNodeProvider(IFileSystemManager manager, string scheme)
			: base(manager)
		{
			this.schemas = new string[]
			{
				scheme
			};

			this.imaginaryFileSystem = new ImaginaryFileSystem(scheme);
			this.root = this.imaginaryFileSystem.RootDirectory;
		}

		public override INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options)
		{
			Pair<string, string> result;

			result = uri.SplitOnFirst("://");

			if (result.Left != this.imaginaryFileSystem.RootDirectory.Address.Scheme)
			{
				throw new NotSupportedException(result.Left);
			}

			return this.imaginaryFileSystem.Resolve(TextConversion.FromEscapedHexString(result.Right), nodeType);
		}
	}
}
