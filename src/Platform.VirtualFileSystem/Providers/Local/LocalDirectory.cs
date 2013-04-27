using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Platform.VirtualFileSystem.Providers.Local
{
	public class LocalDirectory
		: AbstractDirectory, INativePathService
	{
		internal DirectoryInfo directoryInfo;
		
		protected override INodeAttributes CreateAttributes()
		{
			LocalNodeAttributes attributes = null;
						
			attributes = new LocalNodeAttributes(this, this.directoryInfo);
									
			return new AutoRefreshingNodeAttributes(attributes, -1);
		}

		public override IService GetService(ServiceType serviceType)
		{
			if (serviceType.Is(typeof(INativePathService)))
			{
				return this;
			}

			return base.GetService(serviceType);
		}

		public override string DefaultContentName
		{
			get
			{
				return Native.GetInstance().DefaultContentName;
			}
		}

		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}

        protected override INode DoRenameTo(string name, bool overwrite)
        {
            string destPath;

            name = StringUriUtils.RemoveQuery(name);

            destPath = Path.Combine(this.directoryInfo.Parent.FullName, name);

            if (this.FileSystem.PathsEqual(destPath, this.directoryInfo.Parent.FullName, destPath.Length))
            {
                return this;
            }

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    if (overwrite)
                    {
                        Directory.Delete(destPath);
                    }

                    //
                    // Don't use FileInfo.MoveTo as it changes the existing FileInfo
                    // use the new path.
                    //

                    Directory.Move(this.directoryInfo.FullName, destPath);
                    this.directoryInfo.Refresh();

                    break;

                }
                catch (IOException)
                {
                    if (i == 4)
                    {
                        throw;
                    }

                    Thread.Sleep(500);
                }
            }

            return this;
        }

		public override INode DoCreate(bool createParent)
		{
			if (!createParent)
			{
				this.ParentDirectory.Refresh();

				if (!this.ParentDirectory.Exists)
				{
					throw new DirectoryNodeNotFoundException(this.ParentDirectory.Address);
				}
			}

			try
			{
				this.directoryInfo.Create();
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(this.Address);	
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(this.Address);	
			}

			return this;
		}
		
		protected override INode DoDelete()
		{
			try
			{
				this.directoryInfo.Delete();

			    LocalFileSystem.OnDirectoryDeleted(new LocalFileSystemEventArgs(this));
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(this.Address);	
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(this.Address);	
			}

			return this;
		}

		protected override IDirectory DoDelete(bool recursive)
		{
			try
			{
				this.directoryInfo.Delete(recursive);

                LocalFileSystem.OnDirectoryDeleted(new LocalFileSystemEventArgs(this));
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(this.Address);
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(this.Address);
			}

			return this;
		}

		public override IDirectory OperationTargetDirectory
		{
			get
			{
				return this;
			}
		}

		public override IEnumerable<string> GetChildNames()
		{
			return GetChildNames(NodeType.Any);
		}

		public override IEnumerable<string> GetChildNames(NodeType nodeType)
		{
			if (this.FileSystem.SecurityManager.IsActive)
			{
				return base.GetChildNames(nodeType);
			}
			else
			{
				return PrivateGetChildNames(nodeType);
			}
		}

		private IEnumerable<string> PrivateGetChildNames(NodeType nodeType)
		{
			if (nodeType.Equals(NodeType.File) || nodeType.Equals(NodeType.Any))
			{
				string[] names;

				foreach (string s in this.GetJumpPointNames(NodeType.File))
				{
					yield return s;
				}

				try
				{
					names = Directory.GetFiles(this.directoryInfo.FullName);
				}
				catch (DirectoryNotFoundException)
				{
					throw new DirectoryNodeNotFoundException(this.Address);
				}

				foreach (string s in names)
				{
					if (!this.ContainsShortcut(s, NodeType.File))
					{
						yield return Path.GetFileName(s);
					}
				}
			}

			if (nodeType.Equals(NodeType.Directory) || nodeType.Equals(NodeType.Any))
			{
				string[] names;

				foreach (var s in this.GetJumpPointNames(NodeType.Directory))
				{
					yield return s;
				}

				try
				{
					names = Directory.GetDirectories(this.directoryInfo.FullName);
				}
				catch (System.IO.DirectoryNotFoundException)
				{
					throw new DirectoryNodeNotFoundException(this.Address);
				}

				foreach (var s in names)
				{
					if (!this.ContainsShortcut(s, NodeType.Directory))
					{
						yield return Path.GetFileName(s);
					}
				}
			}
		}

		private bool refreshNodes = true;

		public override IDirectory Refresh(DirectoryRefreshMask mask)
		{
			base.Refresh(mask);

			if ((mask & DirectoryRefreshMask.AllChildren) != 0)
			{
				lock (this.SyncLock)
				{
					this.refreshNodes = true;
				}
			}

			return this;
		}

		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			bool localRefreshNodes;

			lock (this.SyncLock)
			{
				localRefreshNodes = this.refreshNodes;

				this.refreshNodes = false;
			}

			localRefreshNodes = true;

			/**
			 * TODO: If refreshNodes is true then we have to manually refresh 
			 * all children that aren't listed in the current listing but are
			 * still in the node cache.
			 * 
			 * Do this by attaching to cache events (thru a weak event handler) and 
			 * keeping a list of all cached (and thus resolved) nodes that are children
			 * of this node.
			 */

			//lock (this.SyncLock)
			{
				string[] items;

				foreach (var jumpPoint in GetJumpPoints(NodeType.Directory))
				{
					yield return jumpPoint;
				}

				if ((nodeType.Equals(NodeType.Directory)) || nodeType.Equals(NodeType.Any))
				{
					try
					{
						if (Environment.OSVersion.Platform == PlatformID.Unix)
						{
							// Hack for mono which does not return symbolic links as directories anymore

							items = Directory.GetFileSystemEntries(this.directoryInfo.FullName);
						}
						else
						{
							items = Directory.GetDirectories(this.directoryInfo.FullName);
						}
					}
					catch (System.IO.IOException)
					{
						throw new DirectoryNodeNotFoundException(this.Address);
					}

					foreach (var name in items)
					{
						if (Environment.OSVersion.Platform == PlatformID.Unix)
						{
							if (Native.GetInstance().GetSymbolicLinkTarget(name) != null)
							{
								if (Native.GetInstance().GetSymbolicLinkTargetType(name) != NodeType.Directory)
								{
									continue;
								}
							}
							else
							{
								if (!Directory.Exists(name))
								{
									continue;
								}
							}
						}

						var dir = (IDirectory)this.FileSystem.Resolve(this.Address.ResolveAddress(Path.GetFileName(name)), NodeType.Directory);

						if (localRefreshNodes && !String.Equals((string)dir.Attributes["DriveType"], "Removable", StringComparison.CurrentCultureIgnoreCase))
						{							
							dir.Refresh();
						}

						if (!ContainsShortcut(dir.Name, NodeType.Directory))
						{
							if (acceptNode(dir))
							{
								yield return dir;
							}
						}
					}
				}

				foreach (INode jumpPoint in GetJumpPoints(NodeType.File))
				{
					yield return jumpPoint;
				}

				if ((nodeType.Equals(NodeType.File)) || nodeType.Equals(NodeType.Any))
				{
					try
					{
						if (Environment.OSVersion.Platform == PlatformID.Unix)
						{
							items = Directory.GetFileSystemEntries(this.directoryInfo.FullName);
						}
						else
						{
							items = Directory.GetFiles(this.directoryInfo.FullName);
						}
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						throw new DirectoryNodeNotFoundException(this.Address);
					}

					foreach (string name in items)
					{
						if (Environment.OSVersion.Platform == PlatformID.Unix)
						{
							if (Native.GetInstance().GetSymbolicLinkTarget(name) != null)
							{
								if (Native.GetInstance().GetSymbolicLinkTargetType(name) != NodeType.File)
								{
									continue;
								}
							}
							else
							{
								if (!File.Exists(name))
								{
									continue;
								}
							}
						}

                        var file = (IFile)this.FileSystem.Resolve(this.Address.ResolveAddress(Path.GetFileName(name)), NodeType.File);
						
						if (localRefreshNodes)
						{
							file.Refresh();
						}

						if (!ContainsShortcut(file.Name, NodeType.File))
						{
							if (acceptNode(file))
							{
								yield return file;
							}
						}
					}
				}

				localRefreshNodes = false;
			}
		}

		protected override IFileSystem DoCreateView(string scheme, FileSystemOptions options)
		{
			if (scheme == this.Address.Scheme 
				&& this == this.FileSystem.RootDirectory
				&& options == this.FileSystem.Options)
			{
				return this.FileSystem;
			}

			return new LocalFileSystem(((AbstractNodeAddressWithRootPart)this.Address).CreateAsRoot(scheme), options);
		}

		public LocalDirectory(LocalFileSystem fileSystem, LocalNodeAddress address)
			: base(fileSystem, address)
		{
			this.directoryInfo = new DirectoryInfo(address.AbsoluteNativePath);
		}

		protected override Stream DoOpenStream(string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (contentName == null)
			{
				throw new NotSupportedException();
			}

			return Native.GetInstance().OpenAlternateContentStream
			(
				this.directoryInfo.FullName,
				contentName,
				fileMode, fileAccess, fileShare
			);
		}

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode mode, FileShare sharing)
		{
			if (contentName == null)
			{
				throw new NotSupportedException();
			}

			encoding = null;

			return Native.GetInstance().OpenAlternateContentStream
				(
					this.directoryInfo.FullName,
					contentName,
					mode, FileAccess.Write, sharing
				);
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			if (contentName == null)
			{
				throw new NotSupportedException();
			}

			return Native.GetInstance().OpenAlternateContentStream
				(
					this.directoryInfo.FullName,
					contentName,
					mode, FileAccess.Read, sharing
				);
		}

		string INativePathService.GetNativePath()
		{
			var retval = ((LocalNodeAddress)this.Address).AbsoluteNativePath;

			if (!retval.EndsWith(Path.DirectorySeparatorChar))
			{
				retval += Path.DirectorySeparatorChar;
			}

			return retval;
		}

		string INativePathService.GetNativeShortPath()
		{
			var retval = Native.GetInstance().GetShortPath(((LocalNodeAddress)this.Address).AbsoluteNativePath);

			if (!retval.EndsWith(Path.DirectorySeparatorChar))
			{
				retval += Path.DirectorySeparatorChar;
			}

			return retval;
		}
	}
}
