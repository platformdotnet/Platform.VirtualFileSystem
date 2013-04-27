using System;
using System.Collections.Generic;
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
