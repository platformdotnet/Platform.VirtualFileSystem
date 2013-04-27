using System;
using System.Diagnostics;
using System.IO;

namespace Platform.VirtualFileSystem.Providers.Local
{
	public class LocalFileSystem
		: AbstractFileSystem
	{
		private FileSystemWatcher fileSystemWatcherFile = null;
		private FileSystemWatcher fileSystemWatcherDirectory = null;

	    internal static event EventHandler<LocalFileSystemEventArgs> DirectoryDeleted;

        internal static void OnDirectoryDeleted(LocalFileSystemEventArgs eventArgs)
        {
            if (DirectoryDeleted != null)
            {
                DirectoryDeleted(null, eventArgs);
            }
        }

	    public override bool SupportsSeeking
		{
			get
			{
				return true;
			}
		}

		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}

		private volatile int activityListenerCount;

		public override event FileSystemActivityEventHandler Activity
		{
			add
			{
				lock (this)
				{
					if (!this.fileSystemWatcherInitialised)
					{
						InitializeFsw();
					}

					this.ActivityEvent += value;

					if (this.activityListenerCount == 0)
					{
                        try
                        {
                            this.fileSystemWatcherFile.EnableRaisingEvents = true;
                            this.fileSystemWatcherDirectory.EnableRaisingEvents = true;
                        }
                        catch (Exception)
                        {
                        }
					}

					this.activityListenerCount++;
				}
			}
			remove
			{
				lock (this)
				{
					if (!this.fileSystemWatcherInitialised)
					{
						InitializeFsw();
					}

					this.ActivityEvent -= value;

					this.activityListenerCount--;

					if (this.activityListenerCount == 0)
					{
						this.fileSystemWatcherFile.EnableRaisingEvents = false;	
						this.fileSystemWatcherDirectory.EnableRaisingEvents = false;
					}
				}
			}
		}
		private FileSystemActivityEventHandler ActivityEvent;

		protected virtual void OnActivityEvent(FileSystemActivityEventArgs eventArgs)
		{
			if (ActivityEvent != null)
			{
				if (this.SecurityManager.CurrentContext.IsEmpty)
				{
					ActivityEvent(this, eventArgs);
				}
				else
				{
					INode node;

					node = this.Resolve(eventArgs.Path, eventArgs.NodeType);

					if (this.SecurityManager.CurrentContext.HasAccess(new AccessVerificationContext(node, FileSystemSecuredOperation.View)))
					{
						ActivityEvent(this, eventArgs);
					}
				}
			}
		}

		public LocalFileSystem(INodeAddress rootAddress, FileSystemOptions options)
			: base(rootAddress, null, options)
		{			
		}

		private volatile bool fileSystemWatcherInitialised = false;

		private void InitializeFsw()
		{
			lock (this)
			{
				if (this.fileSystemWatcherInitialised)
				{
					return;
				}

			    WeakReference weakref;
                EventHandler<LocalFileSystemEventArgs> handler = null;

			    weakref = new WeakReference(this);

			    handler = delegate(object sender, LocalFileSystemEventArgs eventArgs)
			              {
                              LocalFileSystem _this = (LocalFileSystem)weakref.Target;

                              if (_this == null)
                              {
                                  LocalFileSystem.DirectoryDeleted -= handler;

                                  return;
                              }

			                  string s;

			                  s = (((LocalDirectory) eventArgs.Directory).directoryInfo.FullName);

                              if (this.PathsEqual(((LocalNodeAddress)this.RootAddress).AbsoluteNativePath, s, s.Length))
                              {
                                  // This allows the directory to be deleted

                                  this.fileSystemWatcherDirectory.EnableRaisingEvents = false;
                                  this.fileSystemWatcherFile.EnableRaisingEvents = false;

                                  // TODO: Use file system watcher from level above to allow
                                  // events to still be fired if the directory gets recreated
                              }
			              };

                LocalFileSystem.DirectoryDeleted += handler;

				// We need two different watchers since we need to determine if the renamed/created/deleted events
				// occured on a file or a directory.
				// Changed events occur on both dirs and files regardless of the NotifyFilters setting so the
				// File watcher will be responsible for handling that.

				DriveInfo driveInfo;

                try
                {
                    driveInfo = new DriveInfo(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath);
                }
                catch (ArgumentException)
                {
                    driveInfo = null;
                }

			    if (driveInfo == null || (driveInfo != null && driveInfo.DriveType != DriveType.Removable))
				{
                    if (!Directory.Exists(((LocalNodeAddress)this.RootAddress).AbsoluteNativePath))
                    {
                        // TODO: Use directory above

                        return;
                    }

				    string path;

				    path = ((LocalNodeAddress) this.RootAddress).AbsoluteNativePath;

				    path = path.TrimEnd('/', '\\');

					if (Environment.OSVersion.Platform != PlatformID.Unix)
					{
						if (Path.GetDirectoryName(path) != null)
						{
							path = Path.GetDirectoryName(path);
						}
					}

					if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
					{
						path += Path.DirectorySeparatorChar;
					}

					this.fileSystemWatcherFile = new FileSystemWatcher(path);
					this.fileSystemWatcherFile.InternalBufferSize = 64 * 1024 /* 128K buffer */;
					this.fileSystemWatcherFile.IncludeSubdirectories = true;
					this.fileSystemWatcherFile.NotifyFilter =
						NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName /* Files only */
						| NotifyFilters.LastAccess | NotifyFilters.LastWrite /*| NotifyFilters.Security | NotifyFilters.Size*/;

					this.fileSystemWatcherFile.Renamed += new RenamedEventHandler(FileSystemWatcher_Renamed);
					this.fileSystemWatcherFile.Changed += new FileSystemEventHandler(FileSystemWatcher_Changed);
					this.fileSystemWatcherFile.Created += new FileSystemEventHandler(FileSystemWatcher_Created);
					this.fileSystemWatcherFile.Deleted += new FileSystemEventHandler(FileSystemWatcher_Deleted);
					this.fileSystemWatcherFile.Error += new ErrorEventHandler(FileSystemWatcher_Error);

					this.fileSystemWatcherDirectory = new FileSystemWatcher(path);
					this.fileSystemWatcherDirectory.InternalBufferSize = 64 * 1024;
					this.fileSystemWatcherDirectory.IncludeSubdirectories = true;
					this.fileSystemWatcherDirectory.NotifyFilter =
						NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName /* Dirs only */
						| NotifyFilters.LastAccess | NotifyFilters.LastWrite /*| NotifyFilters.Security | NotifyFilters.Size*/;

					this.fileSystemWatcherDirectory.Filter = "*";
					this.fileSystemWatcherDirectory.Renamed += new RenamedEventHandler(FileSystemWatcher_Renamed);
					this.fileSystemWatcherDirectory.Created += new FileSystemEventHandler(FileSystemWatcher_Created);
					this.fileSystemWatcherDirectory.Deleted += new FileSystemEventHandler(FileSystemWatcher_Deleted);
					//fileSystemWatcherDirectory.Changed += new FileSystemEventHandler(FileSystemWatcher_Changed);
					this.fileSystemWatcherDirectory.Error += new ErrorEventHandler(FileSystemWatcher_Error);

					this.fileSystemWatcherInitialised = true;					
				}
			}
		}

		private object fileSystemMonitorLock = new object();

		private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
		{
			Trace.WriteLine("FileSystemWatcher generated an error: " + e.GetException().Message, "FileSystemWatcher");
		}

		public override int MaximumPathLength
		{
			get
			{
				switch (Environment.OSVersion.Platform)
				{
					case PlatformID.Win32NT:
						return 255 - ((LocalNodeAddress)this.RootAddress).RootPart.Length - 1;
					case PlatformID.Win32S:
					case PlatformID.Win32Windows:
					case PlatformID.WinCE:
					default:
						return 255 - ((LocalNodeAddress)this.RootAddress).RootPart.Length - 1;
				}
			}
		}

		private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (ActivityEvent != null)
			{
				NodeType nodeType;

				if (!e.FullPath.StartsWith(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath))
				{
					return;
				}

				nodeType = (sender == this.fileSystemWatcherFile) ? NodeType.File : NodeType.Directory;

				OnActivityEvent(new FileSystemRenamedActivityEventArgs(FileSystemActivity.Renamed,
				                                                       nodeType,
				                                                       Path.GetFileName(e.OldName),
				                                                       StringUriUtils.NormalizePath(
				                                                       	e.OldFullPath.Substring(
				                                                       		this.fileSystemWatcherFile.Path.Length - 1)),
				                                                       Path.GetFileName(e.Name),
				                                                       StringUriUtils.NormalizePath(
				                                                       	e.FullPath.Substring(
				                                                       		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.
				                                                       			Length - 1))));
			}
		}

		private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (ActivityEvent != null)
			{
				NodeType nodeType;

				if (!e.FullPath.StartsWith(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath))
				{
					return;
				}

				/*
				 * There is a race here between when the watcher sees the event and when it is processed
				 * which may mean that a changed event meant for a dir goes to a file (or vice versa).
				 * There's nothing we can do about it but since the Changed event is pretty opaque
				 * (users will have to figure what has changed anyway) it doesn't matter too much.
				 */

				nodeType = GetNodeType(e.FullPath.Substring(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length - 1));

				if (nodeType == NodeType.None)
				{
					OnActivityEvent(new FileSystemActivityEventArgs(FileSystemActivity.Changed, NodeType.File, e.Name,
					                                                StringUriUtils.NormalizePath(
					                                                	e.FullPath.Substring(
					                                                		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length -
					                                                		1))));

					OnActivityEvent(new FileSystemActivityEventArgs(FileSystemActivity.Changed, NodeType.Directory, e.Name,
					                                                StringUriUtils.NormalizePath(
					                                                	e.FullPath.Substring(
					                                                		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length -
					                                                		1))));

					return;
				}
				else
				{
					OnActivityEvent(new FileSystemActivityEventArgs(FileSystemActivity.Changed, nodeType, e.Name,
					                                                StringUriUtils.NormalizePath(
					                                                	e.FullPath.Substring(
					                                                		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length -
					                                                		1))));
				}
			}
		}

		private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if (ActivityEvent != null)
			{
				NodeType nodeType;

				if (!e.FullPath.StartsWith(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath))
				{
					return;
				}

				nodeType = (sender == this.fileSystemWatcherFile) ? NodeType.File : NodeType.Directory;

				OnActivityEvent(new FileSystemActivityEventArgs(FileSystemActivity.Created, nodeType, e.Name,
				                                                StringUriUtils.NormalizePath(
				                                                	e.FullPath.Substring(
				                                                		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length -
				                                                		1))));
			}
		}

		private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (ActivityEvent != null)
			{
				NodeType nodeType;

				if (!e.FullPath.StartsWith(((LocalNodeAddress) this.RootAddress).AbsoluteNativePath))
				{
					return;
				}

				nodeType = (sender == this.fileSystemWatcherFile) ? NodeType.File : NodeType.Directory;

				OnActivityEvent(new FileSystemActivityEventArgs(FileSystemActivity.Deleted, nodeType, e.Name,
				                                                StringUriUtils.NormalizePath(
				                                                	e.FullPath.Substring(
				                                                		((LocalNodeAddress) this.RootAddress).AbsoluteNativePath.Length -
				                                                		1))));
			}
		}

		public override INode Resolve(INodeAddress address, NodeType nodeType)
		{
			if (nodeType == NodeType.Any)
			{
				INode node;

				node = Resolve(address, NodeType.File);

				if (node.Exists)
				{
					return node;
				}
				
				node = Resolve(address, NodeType.Directory);

				if (node.Exists)
				{
					return node;
				}
				
				return base.Resolve(address, NodeType.File);
			}

			// LocalFileSystem always refreshes node before returning it

			return base.Resolve(address, nodeType).Refresh();
		}

		/// <summary>
		/// <see cref="INode.Resolve(string, NodeType, AddressScope)"/>
		/// </summary>
		protected override INode CreateNode(INodeAddress address, NodeType nodeType)
		{
			if (nodeType == NodeType.File)
			{
				return new LocalFile(this, (LocalNodeAddress)address);
			}
			else if (nodeType == NodeType.Directory)
			{
				return new LocalDirectory(this, (LocalNodeAddress)address);
			}
			else if (nodeType == NodeType.Any)
			{
				if (Directory.Exists(address.RootUri + address.AbsolutePath))
				{
					return new LocalDirectory(this, (LocalNodeAddress)address);
				}
				else
				{
					return new LocalFile(this, (LocalNodeAddress)address);
				}
			}
			else
			{
				throw new NodeTypeNotSupportedException(nodeType);
			}
		}

		public override bool PathsEqual(string path1, string path2, int length)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				if (length > path1.Length || length > path2.Length)
				{
					return false;
				}

				for (int i = 0; i < length; i++)
				{
					if (!path1[i].Equals(path2[i]))
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return base.PathsEqual(path1, path2, length);
			}
		}

	    private volatile bool disposed;

        internal protected virtual void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("LocalFileSystem: " + this.RootAddress.Uri);
            }
        }

        public override bool IsDisposed
        {
            get
            {
                return this.disposed;
            }
        }

	    public override void Dispose()
        {
            try
            {
                this.disposed = true;

                this.fileSystemWatcherDirectory.EnableRaisingEvents = false;
                this.fileSystemWatcherFile.EnableRaisingEvents = false;
            }
            catch
            {
            }
        }
	}
}
