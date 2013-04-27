using System;
using System.IO;
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
			lock (this)
			{
				lock (this.FileSystem)
				{
					this.Refresh();

					encoding = null;

					if (this.zipEntry == null)
					{
						throw new FileNotFoundException(this.Address.Uri);
					}

					return ((ZipFileSystem)this.FileSystem).GetInputStream(this.zipEntry);
				}
			}
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
