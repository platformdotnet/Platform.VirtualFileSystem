using System;
using System.IO;
using Platform.IO;
using Platform.VirtualFileSystem.Providers;
using Platform.VirtualFileSystem.Providers.Zip;
using ZLib = ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ZipFile
		: AbstractFile, IZipNode
	{
		private ZLib.ZipEntry zipEntry;

		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}

		public ZipFile(ZipFileSystem fileSystem, LayeredNodeAddress address, ZLib.ZipEntry zipEntry)
			: base(address, fileSystem)
		{
			((IZipNode)this).SetZipEntry(zipEntry);
		}

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode fileMode, FileShare fileShare)
		{
			encoding = null;

			return ZipFileStream.CreateInputStream(this);
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode fileMode, FileShare fileShare)
		{
			return ZipFileStream.CreateOutputStream(this);
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new ZipFileAttributes(this);
		}

		ZLib.ZipEntry IZipNode.ZipEntry
		{
			get
			{
				return this.zipEntry;
			}
		}

		void IZipNode.SetZipEntry(ZLib.ZipEntry value)
		{
			this.zipEntry = value;
			
			if (this.zipEntry != null)
			{
				this.zipPath = this.zipEntry.Name;
			}
			else
			{
				this.zipPath = this.Address.AbsolutePath.Substring(1);
			}
		}

		protected override INode DoDelete()
		{
			var zipFileInfo = ((ZipFileSystem)this.FileSystem).GetZipFileInfo(this.Address.AbsolutePath);

			zipFileInfo.Delete();

			return this;
		}

		string IZipNode.ZipPath
		{
			get
			{
				return this.zipPath;;
			}
		}
		private string zipPath;
	}
}
