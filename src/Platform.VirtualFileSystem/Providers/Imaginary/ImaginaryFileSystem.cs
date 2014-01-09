using System;

namespace Platform.VirtualFileSystem.Providers.Imaginary
{
	public class ImaginaryFileSystem
		: AbstractFileSystem
	{
		public override event FileSystemActivityEventHandler Activity
		{
			add
			{
			}
			remove
			{
			}
		}


		private IDirectory root;

		public ImaginaryFileSystem(string scheme)
			: this(scheme, FileSystemOptions.Default)
		{
			this.root = this.ResolveDirectory("/").Create();
		}

		public ImaginaryFileSystem(string scheme, FileSystemOptions options)
			: base(new View.ViewNodeAddress(scheme, "/", ""), null, options)
		{
		}

		protected override INode CreateNode(INodeAddress address, NodeType nodeType)
		{
			if (!address.IsRoot)
			{
				var parent = (IDirectory)this.Resolve(address.Parent, NodeType.Directory);

				if (parent is ImaginaryDirectory)
				{
					var retval = ((ImaginaryDirectory)parent).GetImaginaryChild(address.Name, nodeType);

					if (retval != null)
					{
						return retval;
					}
				}
			}

			if (nodeType == NodeType.Directory)
			{								
				return new ImaginaryDirectory(this, address);
			}
			else if (nodeType == NodeType.File)
			{
				return new ImaginaryFile(this, address);
			}

			throw new NotSupportedException(String.Format("{0}:{1}", address, nodeType));
		}
	}
}
