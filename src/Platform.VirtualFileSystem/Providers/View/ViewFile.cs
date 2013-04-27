using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Providers.View
{
	public class ViewFile
		: FileWrapper
	{
		public override INodeAddress Address
		{
			get
			{
				return this.address;
			}
		}
		private readonly ViewNodeAddress address;

		public override IFileSystem FileSystem
		{
			get
			{
				return this.fileSystem;
			}
		}
		private readonly IFileSystem fileSystem;

		public ViewFile(ViewFileSystem fileSystem, ViewNodeAddress address, IFile wrappee)
			: base(wrappee)
		{
			this.address = address;
			this.fileSystem = fileSystem;

			this.NodeResolver = new ViewResolver(fileSystem, this.Address);
			this.NodeAdapter = fileSystem.ViewNodeAdapter;
		}
	}
}
