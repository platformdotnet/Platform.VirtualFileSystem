using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZLib = ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ZipDirectory
		: AbstractDirectory, IZipNode
	{
		private ZLib.ZipEntry zipEntry;

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

			if (value != null)
			{
				this.zipPath = value.Name;
			}
			else
			{
				this.zipPath = this.Address.AbsolutePath.Substring(1);
			}
		}

		private string zipPath;

		public ZipDirectory(ZipFileSystem fileSystem, LayeredNodeAddress address, ZLib.ZipEntry zipEntry)
			: base(fileSystem, address)
		{
			((IZipNode)(this)).SetZipEntry(zipEntry);
		}

		string IZipNode.ZipPath
		{
			get
			{
				return this.zipPath;
			}
		}

		protected override INode DoDelete()
		{
			return this.DoDelete(false);
		}

		protected override IDirectory DoDelete(bool recursive)
		{
			if (((ZipFileSystem)this.FileSystem).Options.ReadOnly)
			{
				return base.DoDelete(recursive);
			}

			if (!recursive)
			{
				if (this.GetChildren().Any())
				{
					throw new IOException("The directory is not empty");
				}

				((ZipFileSystem)this.FileSystem).GetZipDirectoryInfo(this.Address.AbsolutePath).Delete();
			}
			else
			{
				foreach (var item in this.GetChildren())
				{
					if (item is IDirectory)
					{
						((IDirectory)item).Delete(true);
					}
					else
					{
						item.Delete();
					}
				}

				((ZipFileSystem)this.FileSystem).GetZipDirectoryInfo(this.Address.AbsolutePath).Delete();
			}

			return this;
		}

		public override INode DoCreate(bool createParent)
		{
			if (((ZipFileSystem)this.FileSystem).Options.ReadOnly)
			{
				return base.DoCreate(createParent);
			}

			if (createParent)
			{
				if (!this.ParentDirectory.Exists)
				{
					this.ParentDirectory.Create(true);
				}
			}
			else
			{
				if (!this.ParentDirectory.Exists)
				{
					throw new DirectoryNodeNotFoundException(this.ParentDirectory.Address);
				}
			}

			((ZipFileSystem)this.FileSystem).GetZipDirectoryInfo(this.Address.AbsolutePath).Create();

			return this;
		}
		
		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			INode node;
			var files = new List<IFile>();
			var directories = new HashSet<string>();

			Refresh();

			var fileSystem = (ZipFileSystem)this.FileSystem;

			foreach (ZLib.ZipEntry zipEntry in fileSystem.zipFile)
			{
				if (zipEntry.Name.StartsWith(this.zipPath)  /* Is descendent */
					&& zipEntry.Name.Length != this.zipPath.Length /* Not self */)
				{
					var x = zipEntry.Name.IndexOf('/', this.zipPath.Length);

					if (x == -1 /* Is direct descendent File */)
					{
						node = this.FileSystem.ResolveFile(FileSystemManager.SeperatorChar + zipEntry.Name);

						if (acceptNode(node))
						{
							files.Add((IFile)node);
						}
					}
					else if (x <= zipEntry.Name.Length - 1 /* Is direct descendent dir */)
					{
						var s = zipEntry.Name.Substring(0, x);

						directories.Add(s);
					}
				}
			}

			if (nodeType.Equals(NodeType.Directory) || nodeType.Equals(NodeType.Any))
			{
				var sortedDirectories = directories.Sorted(StringComparer.InvariantCultureIgnoreCase);

				foreach (var path in sortedDirectories)
				{
					node = this.FileSystem.ResolveDirectory(FileSystemManager.SeperatorChar + path);

					if (acceptNode(node))
					{
						yield return node;
					}
				}
			}
			
			if (nodeType.Equals(NodeType.File) || nodeType.Equals(NodeType.Any))
			{
				foreach (var file in files)
				{
					yield return file;
				}
			}
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new ZipNodeAttributes(this);
		}
	}
}
