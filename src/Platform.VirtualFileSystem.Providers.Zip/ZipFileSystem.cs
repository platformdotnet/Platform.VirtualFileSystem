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
		public override bool SupportsActivityEvents => true;

		private readonly AttributeChangeDeterminer changeDeterminer; 
		private readonly Dictionary<string, ZipFileInfo> zipFileInfos = new Dictionary<string, ZipFileInfo>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, ZipDirectoryInfo> zipDirectoryInfos = new Dictionary<string, ZipDirectoryInfo>(StringComparer.InvariantCultureIgnoreCase);

		internal virtual ZipDirectoryInfo GetZipDirectoryInfo(string path)
		{
			lock (this)
			{
				ZipDirectoryInfo retval;

				if (!zipDirectoryInfos.TryGetValue(path, out retval))
				{
					retval = new ZipDirectoryInfo(false) { AbsolutePath = path };


					zipDirectoryInfos[path] = retval;
				}

				return retval;
			}
		}

		internal virtual ZipFileInfo GetZipFileInfo(string path)
		{
			lock (this)
			{
				ZipFileInfo retval;

				if (!zipFileInfos.TryGetValue(path, out retval))
				{
					retval = new ZipFileInfo(null) { AbsolutePath = path };


					zipFileInfos[path] = retval;
				}

				return retval;
			}
		}

		private void RefreshNodeInfos()
		{	
			lock (this)
			{
				if (zipFileInfos != null)
				{
					foreach (var zipFileInfo in this.zipFileInfos.Values.Where(c => c.ShadowFile != null))
					{
						try
						{
							zipFileInfo.ShadowFile.Delete();
						}
						catch
						{
						}
					}
				}

				zipFileInfos.Clear();
				zipDirectoryInfos.Clear();

				var directories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "/" };


				var zipEntriesByDirectory = new Dictionary<string, ZLib.ZipEntry>();

				foreach (ZLib.ZipEntry zipEntry in zipFile)
				{
					if (zipEntry.IsDirectory)
					{
						var s = "/" + zipEntry.Name.TrimRight('/');

						directories.Add(s);

						zipEntriesByDirectory[s] = zipEntry;
					}
					else
					{
						var zipFileInfo = new ZipFileInfo(zipEntry);

						var x = zipEntry.Name.LastIndexOf('/');

						if (x > 0)
						{
							var path = zipEntry.Name.Substring(0, x);

							directories.Add("/" + path);
						}

						zipFileInfo.AbsolutePath = "/" + zipEntry.Name;

						zipFileInfos[zipFileInfo.AbsolutePath] = zipFileInfo;
					}
				}

				foreach (var directoryPath in directories)
				{
					ZLib.ZipEntry zipEntry; 
					var zipDirectoryInfo = new ZipDirectoryInfo(true);

					if (zipEntriesByDirectory.TryGetValue(directoryPath, out zipEntry))
					{
						zipDirectoryInfo.ZipEntry = zipEntry;
					}

					zipDirectoryInfo.AbsolutePath = directoryPath;
					zipDirectoryInfos[zipDirectoryInfo.AbsolutePath] = zipDirectoryInfo;
				}
			}
		}

		public virtual void Flush()
		{
			this.Flush(true);
		}

		private void Flush(bool refresh)
		{
			lock (this)
			{
				var filesChanged = zipFileInfos.Values.Where(c => c.ShadowFile != null).ToList();
				var filesDeleted = zipFileInfos.Values.Where(c => !c.Exists && c.ZipEntry != null).ToList();

				var setOfPreviousDirectories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

				foreach (ZLib.ZipEntry zipEntry in zipFile)
				{
					if (zipEntry.IsDirectory)
					{
						setOfPreviousDirectories.Add("/" + zipEntry.Name.Substring(0, zipEntry.Name.Length - 1));
					}
					else
					{
						var x = zipEntry.Name.LastIndexOf('/');

						if (x > 0)
						{
							var path = zipEntry.Name.Substring(0, x);

							setOfPreviousDirectories.Add("/" + path);
						}
					}
				}

				var setOfCurrentImplicitDirectories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase); 
				var setOfCurrentDirectories = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase); 

				foreach (var zipFileInfo in zipFileInfos.Values)
				{
					if (zipFileInfo.Exists)
					{
						var x = zipFileInfo.AbsolutePath.LastIndexOf('/');

						if (x > 0)
						{
							var path = zipFileInfo.AbsolutePath.Substring(0, x);

							setOfCurrentDirectories.Add(path);
							setOfCurrentImplicitDirectories.Add(path);
						}
					}
				}

				foreach (var zipDirectoryInfo in zipDirectoryInfos.Values.Where(c => c.Exists))
				{
					setOfCurrentDirectories.Add(zipDirectoryInfo.AbsolutePath);
				}

				var setOfNewDirectories = new HashSet<string>(setOfCurrentDirectories.Where(c => !setOfPreviousDirectories.Contains(c)), StringComparer.InvariantCultureIgnoreCase);
				var setOfDeletedDirectories = new HashSet<string>(setOfPreviousDirectories.Where(c => !setOfCurrentDirectories.Contains(c)), StringComparer.InvariantCultureIgnoreCase);
				var setOfDirectoriesToCreate = new HashSet<string>(setOfNewDirectories.Where(c => !setOfCurrentImplicitDirectories.Contains(c)), StringComparer.InvariantCultureIgnoreCase);

				setOfDirectoriesToCreate.Remove("/");

				if (filesChanged.Count > 0 || filesDeleted.Count > 0)
				{
					zipFile.BeginUpdate();

					try
					{
						foreach (var zipFileInfo in filesChanged)
						{
							var shadowFile = zipFileInfo.ShadowFile;

							var name = zipFileInfo.AbsolutePath;

							try
							{
								zipFile.Add(new StreamDataSource(shadowFile.GetContent().GetInputStream()), name);
							}
							catch (FileNodeNotFoundException)
							{
							}
						}

						foreach (var zipFileInfo in filesDeleted)
						{
							zipFile.Delete(zipFileInfo.ZipEntry);
						}

						foreach (var directoryToCreate in setOfDirectoriesToCreate)
						{
							zipFile.AddDirectory(directoryToCreate);
						}

						foreach (var directory in setOfDeletedDirectories)
						{
							// SharpZipLib currently doesn't support removing explicit directories
						}
					}
					finally
					{
						zipFile.CommitUpdate();
					}
				}

				if (refresh)
				{
					this.RefreshNodeInfos();
				}
			}
		}

		protected override void Dispose(bool disposing)
		{	
			lock (this)
			{
				this.Flush(false);

				this.Close();

				foreach (var zipFileInfo in this.zipFileInfos.Values.Where(c => c.ShadowFile != null))
				{
					try
					{
						zipFileInfo.ShadowFile.Delete();
					}
					catch
					{
					}
				}

				((IDisposable)this.zipFile).Dispose();
			}

			base.Dispose(disposing);
		}

		public ZipFileSystem(IFile file)
			: this(file, FileSystemOptions.Default)
		{
		}

		public ZipFileSystem(IFile file, FileSystemOptions options)
			: this(LayeredNodeAddress.Parse("zip://[" + file.Address.Uri + "]"), file, options)
		{
		}

		public static ZipFileSystem CreateZipFile(IFile zipFile)
		{
			return CreateZipFile(zipFile, FileSystemOptions.Default);
		}

		public static ZipFileSystem CreateZipFile(IFile zipFile, FileSystemOptions options)
		{
			return CreateZipFile(zipFile, null, null, options);
		}

		public static ZipFileSystem CreateZipFile(IFile zipFile, IDirectory zipFilecontents)
		{
			return CreateZipFile(zipFile, zipFilecontents, FileSystemOptions.Default);
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

			var password = options.Variables["ZipPassword"];

			using (var zipOutputStream = new ZLib.ZipOutputStream(zipFile.GetContent().GetOutputStream()))
			{
				zipOutputStream.SetLevel(compressionLevel);
				zipOutputStream.IsStreamOwner = true;
				zipOutputStream.UseZip64 = ZLib.UseZip64.Dynamic;
				zipOutputStream.Password = password;

				if (files != null)
				{
					foreach (var file in files)
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

			this.OpenZlib();

			this.RefreshNodeInfos();

			if (zipFile.SupportsActivityEvents && options.ReadOnly)
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

		private void OpenZlib()
		{
			if (this.Options.ReadOnly)
			{
				this.zipFile = new ZLib.ZipFile(this.ParentLayer.GetContent().GetInputStream(FileShare.ReadWrite))
				{
					Password = this.Options.Variables["ZipPassword"] 
				};
			}
			else
			{
				this.zipFile = new ZLib.ZipFile(this.ParentLayer.GetContent().OpenStream(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
				{
					Password = this.Options.Variables["ZipPassword"]
				};
			}
		}

		protected virtual void Reload()
		{
			lock (this)
			{
				if (this.isClosed || this.isClosing)
				{
					return;
				}

				this.zipFile.Close();
				
				this.changeDeterminer.MakeUnchanged();

				this.OpenZlib();

				this.RefreshNodeInfos();
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

		private bool isClosed;
		private bool isClosing;

		public override void Close()
		{
			isClosing = true;

			base.Close();

			lock (this)
			{
				if (isClosed)
				{
					return;
				}

				this.zipFile.Close();

				isClosed = true;

				foreach (var zipFileInfo in this.zipFileInfos.Values.Where(c => c.ShadowFile != null))
				{
					try
					{
						zipFileInfo.ShadowFile.Delete();
					}
					catch
					{
					}
				}
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
