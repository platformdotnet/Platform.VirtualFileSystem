using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platform.IO;
using ZLib = ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ZipFileSystem
		: AbstractFileSystem
	{
		internal ZLib.ZipFile zipFile;		

		/// <summary>
		/// <see cref="IFileSystem.SupportsActivityEvents"/>
		/// </summary>
		/// <remarks>
		/// Zip file systems support activity events though not on a file by file basis (because
		/// zip file systems are read only).  Events are fired when the host zip file is changed,
		/// deleted or renamed.
		/// </remarks>
		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}
		private readonly AttributeChangeDeterminer changeDeterminer;

		public ZipFileSystem(IFile file)
			: this(file, FileSystemOptions.NewDefault())
		{
		}

		public ZipFileSystem(IFile file, FileSystemOptions options)
			: this(LayeredNodeAddress.Parse("zip://[" + file.Address.Uri + "]"), file, options)
		{
		}

		public static ZipFileSystem CreateZipFile(IFile zipFile, IDirectory zipFilecontents)
		{
			return CreateZipFile(zipFile, zipFilecontents, FileSystemOptions.NewDefault());
		}
		
		public static ZipFileSystem CreateZipFile(IFile zipFile, IDirectory zipFileContents, FileSystemOptions options)
		{
			return CreateZipFile(zipFile, zipFileContents.Walk(NodeType.File).Select(c => (IFile)c), file => zipFileContents.Address.GetRelativePathTo(file.Address), options);
		}

		public static ZipFileSystem CreateZipFile(IFile zipFile, IEnumerable<IFile> files, Func<IFile, string> fileToFullPath, FileSystemOptions options)
		{
			var compressionLevel = 9;

			var zipCompressionLevel = options.Variables["ZipCompressionLevel"];

			if (zipCompressionLevel != null)
			{
				compressionLevel = Convert.ToInt32(zipCompressionLevel);

				if (compressionLevel < 0)
				{
					compressionLevel = 0;
				}
				else if (compressionLevel > 9)
				{
					compressionLevel = 9;
				}
			}

			using (var zipOutputStream = new ZLib.ZipOutputStream(zipFile.GetContent().GetOutputStream()))
			{
				zipOutputStream.SetLevel(compressionLevel);
				zipOutputStream.IsStreamOwner = true;
				zipOutputStream.UseZip64 = ZLib.UseZip64.Dynamic;

				foreach (IFile file in files)
				{
					var entryName = fileToFullPath(file);
					entryName = ZLib.ZipEntry.CleanName(entryName);
					
					var entry = new ZLib.ZipEntry(entryName);

					using (var stream = file.GetContent().GetInputStream(FileMode.Open, FileShare.Read))
					{
						if (stream.Length > 0)
						{
							entry.Size = stream.Length;
						}

						zipOutputStream.PutNextEntry(entry);

						stream.CopyTo(zipOutputStream);
					}

					zipOutputStream.CloseEntry();
				}
			}

			return new ZipFileSystem(zipFile, options);
		}

		private static IFile GetZipFile(IFile zipFile)
		{
			if (zipFile.Address.QueryValues["shadow"] as string == "true")
			{
				return zipFile;	
			}

			if (ConfigurationSection.Instance.AutoShadowThreshold == -1
				|| zipFile.Length <= ConfigurationSection.Instance.AutoShadowThreshold)
			{
				zipFile = zipFile.ResolveFile(StringUriUtils.AppendQueryPart(zipFile.Address.NameAndQuery, "shadow", "true"));
			}

			return zipFile;
		}

		/// <summary>
		/// Constructs a new <c>ZipFileSystem</c>
		/// </summary>
		/// <param name="rootAddress">The root address of the zip file system.</param>
		/// <param name="zipFile">The zip file that hosts the file system.</param>
		/// <param name="options">Options for the file system.</param>
		public ZipFileSystem(INodeAddress rootAddress, IFile zipFile, FileSystemOptions options)
			: base(rootAddress, GetZipFile(zipFile), options)
		{
			this.changeDeterminer = new AttributeChangeDeterminer(ParentLayer, "LastWriteTime", "Length");

			this.zipFile = new ZLib.ZipFile(this.ParentLayer.GetContent().GetInputStream());

			if (zipFile.SupportsActivityEvents)
			{
				zipFile.Activity += new NodeActivityEventHandler(ZipFile_Activity);
			}
		}

		protected virtual void ZipFile_Activity(object sender, NodeActivityEventArgs eventArgs)
		{
			if (eventArgs.Activity == FileSystemActivity.Changed)
			{
				Reload();
			}
		}

		internal virtual void CheckAndReload()
		{
			if (!this.changeDeterminer.IsUnchanged())
			{
				Reload();	
			}
		}

		protected virtual void Reload()
		{
			lock (this)
			{
				this.zipFile.Close();
				this.changeDeterminer.MakeUnchanged();
				this.zipFile = new ZLib.ZipFile(this.ParentLayer.GetContent().GetInputStream());
			}
		}

		public override event FileSystemActivityEventHandler Activity;

		protected virtual void OnActivity(FileSystemActivityEventArgs eventArgs)
		{
			if (Activity != null)
			{
				Activity(this, eventArgs);
			}
		}

		public override void Close()
		{
			lock (this)
			{
				base.Close();

				this.zipFile.Close();
			}
		}

		public override INode Resolve(INodeAddress address, NodeType nodeType)
		{		
			lock (this)
			{
				CheckAndReload();

				if (nodeType == NodeType.Any)
				{
					var node = this.Resolve(address, NodeType.File);

					if (node.Exists)
					{
						return node;
					}
				
					node = Resolve(address, NodeType.Directory);

					if (node.Exists)
					{
						return node;
					}
				
					return base.Resolve(address, NodeType.Directory);				
				}

				return base.Resolve(address, nodeType);
			}
		}

		internal virtual ZLib.ZipEntry GetEntry(string path)
		{
			lock (this)
			{
				CheckAndReload();

				if (path.Length > 1 && path[0] == FileSystemManager.SeperatorChar)
				{
					path = path.Substring(1);
				}

				return this.zipFile.GetEntry(path);
			}
		}

		private class PrivateStreamWrapper
			: StreamWrapper
		{
			public PrivateStreamWrapper(Stream s)
				: base(s)
			{
			}

			public override void Close()
			{
			}
		}

		internal virtual Stream GetInputStream(string path)
		{
			lock (this)
			{
				if (path.Length > 1 && path[0] == FileSystemManager.SeperatorChar)
				{
					path = path.Substring(1);
				}

				var zipEntry = this.zipFile.GetEntry(path);

				if (zipEntry == null)
				{
					throw new FileNodeNotFoundException();
				}

				return GetInputStream(zipEntry);
			}
		}

		internal virtual Stream GetInputStream(ZLib.ZipEntry zipEntry)
		{
			lock (this)
			{
				return new PrivateStreamWrapper(this.zipFile.GetInputStream(zipEntry));
			}
		}

		/// <summary>
		/// <see cref="AbstractFileSystem.CreateNode(INodeAddress, NodeType)"/>
		/// </summary>
		protected override INode CreateNode(INodeAddress address, NodeType nodeType)
		{
			lock (this)
			{
				string path;

				if (nodeType.Equals(NodeType.File))
				{
					path = ((AbstractNodeAddressWithRootPart)address).AbsolutePathIncludingRootPart;

					return new ZipFile(this, (LayeredNodeAddress)address, GetEntry(path));
				}
				else if (nodeType.Equals(NodeType.Directory))
				{
					path = ((AbstractNodeAddressWithRootPart)address).AbsolutePathIncludingRootPart;

					if (path != FileSystemManager.SeperatorString)
					{
						path += "/";
					}

					return new ZipDirectory(this, (LayeredNodeAddress)address, GetEntry(path));
				}
				else
				{
					throw new NotSupportedException(nodeType.ToString());
				}
			}
		}
	}
}
