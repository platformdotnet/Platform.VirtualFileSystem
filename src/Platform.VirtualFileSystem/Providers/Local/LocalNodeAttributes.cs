using System;
using System.IO;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers.Local
{
	internal class LocalNodeAttributes
		: AbstractTypeBasedNodeAttributes
	{
		protected FileSystemInfo fileSystemInfo;

		private IEnumerable<string> BaseGetNames()
		{
			return base.Names;
		}

		public override IEnumerable<string> Names
		{
			get
			{
				foreach (var name in BaseGetNames())
				{
					yield return name;
				}

				var enumerator = Native.GetInstance().ListExtendedAttributes(fileSystemInfo.FullName).GetEnumerator();

				using (enumerator)
				{
					while (true)
					{
						try
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
						}
						catch (NotSupportedException)
						{
							break;
						}
						catch (NodeNotFoundException)
						{
							break;
						}

						yield return "extended:" + enumerator.Current;
					}
				}
			}
		}

		private IEnumerable<object> BaseGetValues()
		{
			return base.Values;
		}

		public override IEnumerable<object> Values
		{
			get
			{
				foreach (var value in BaseGetValues())
				{
					yield return value;
				}

				var enumerator = Native.GetInstance().ListExtendedAttributes(fileSystemInfo.FullName).GetEnumerator();

				using (enumerator)
				{
					while (true)
					{
						try
						{
							if (!enumerator.MoveNext())
							{
								break;
							}
						}
						catch (NotSupportedException)
						{
							break;
						}
						catch (NodeNotFoundException)
						{
							break;
						}

						yield return GetExtendedAttribute(enumerator.Current);
					}
				}
			}
		}

		public override bool? IsHidden
		{
			get
			{
				CheckRefresh();

				return (fileSystemInfo.Attributes & global::System.IO.FileAttributes.Hidden) != 0;
			}
			set
			{
				if (value == null || value.GetValueOrDefault(false) == true)
				{
					fileSystemInfo.Refresh();

					fileSystemInfo.Attributes |= global::System.IO.FileAttributes.Hidden;
				}
				else
				{
					fileSystemInfo.Attributes &= ~global::System.IO.FileAttributes.Hidden;
				}
			}
		}

		protected internal static DriveInfo GetDriveInfo(string path)
		{
			string symTarget;
			string driveFormat = "";
			DriveInfo driveInfo;
			bool unix = false;
			
			driveInfo = null;

			if (Environment.OSVersion.Platform == PlatformID.Win32NT
				|| Environment.OSVersion.Platform == PlatformID.Win32S
				|| Environment.OSVersion.Platform == PlatformID.Win32Windows
				|| Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				if (path.Length != 3)
				{
					return null;
				}

				if (path[1] != ':')
				{
					return null;
				}

				if (!Char.IsLetter(path[0]))
				{
					return null;
				}

				path = path.Left(2) + "\\";
			}
			else if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				unix = true;
			}

			ActionUtils.IgnoreExceptions(() => driveInfo = new DriveInfo(path));

			if (unix && driveInfo != null)
			{
				try
				{
					driveFormat = driveInfo.DriveFormat;
				}
				catch (IOException)
				{
				}
			}	
			
			if (driveInfo == null || driveFormat == "Unknown")				
			{
				if ((symTarget = Native.GetInstance().GetSymbolicLinkTarget(path)) != null)
				{
					ActionUtils.IgnoreExceptions(() => driveInfo = new DriveInfo(symTarget));

					if (unix && driveInfo != null)
					{
						try
						{
							driveFormat = driveInfo.DriveFormat;
						}
						catch (IOException)
						{
						}
					}
				}
			}

			if (driveInfo == null || driveFormat == "Unknown")
			{
				return null;
			}

			return driveInfo;
		}

		private DriveInfo driveInfo;

		public LocalNodeAttributes(INode node, FileSystemInfo fsInfo)
			: base(node)
		{
			fileSystemInfo = fsInfo;

			if (fileSystemInfo is DirectoryInfo)
			{
				driveInfo = GetDriveInfo(fileSystemInfo.FullName);
			}
		}

		protected void CheckRefresh()
		{
			lock (this.SyncLock)
			{
				if (this.needsRefresh)
				{
					try
					{
						fileSystemInfo.Refresh();
						this.needsRefresh = false;
					}
					catch (FileNotFoundException)
					{
						throw new FileNodeNotFoundException(this.Node.Address);
					}
					catch (DirectoryNodeNotFoundException)
					{
						throw new DirectoryNodeNotFoundException(this.Node.Address);
					}
				}
			}
		}

		public override DateTime? CreationTime
		{
			get
			{
				CheckRefresh();

				if (!this.Exists)
				{
					return null;
				}

				try
				{					
					return fileSystemInfo.CreationTime;
				}
				catch (IOException)
				{
					return null;
				}				
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				fileSystemInfo.CreationTime = (DateTime)value;
			}
		}

		[NodeAttribute]
		public virtual bool? IsSystem
		{
			get
			{
				CheckRefresh();

				if (!this.Exists)
				{
					return null;
				}

				try
				{
					return (fileSystemInfo.Attributes & global::System.IO.FileAttributes.System) != 0;
				}
				catch (IOException)
				{
					return null;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				if (value == true)
				{
					fileSystemInfo.Attributes |= global::System.IO.FileAttributes.System;
				}
				else
				{
					fileSystemInfo.Attributes |= (fileSystemInfo.Attributes & ~global::System.IO.FileAttributes.System);
				}
			}
		}

		public override bool Exists
		{
			get
			{
				CheckRefresh();

				return fileSystemInfo.Exists;
			}
		}

		public override DateTime? LastAccessTime
		{
			get
			{
				CheckRefresh();

				if (!this.Exists)
				{
					return null;
				}

				try
				{
					return fileSystemInfo.LastAccessTime;
				}
				catch (IOException)
				{
					return null;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				fileSystemInfo.LastAccessTime = (DateTime)value;
			}
		}

		public override DateTime? LastWriteTime
		{
			get
			{
				CheckRefresh();

				if (!this.Exists)
				{
					return null;
				}

				try
				{
					return fileSystemInfo.LastWriteTime;
				}
				catch (IOException)
				{
					return null;
				}
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				fileSystemInfo.LastWriteTime = (DateTime)value;
			}
		}

		private bool needsRefresh = false;

		public override INodeAttributes Refresh()
		{
			try
			{
				needsRefresh = true;

				if (fileSystemInfo is DirectoryInfo)
				{
					driveInfo = GetDriveInfo(fileSystemInfo.FullName);
				}
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(this.Node.Address);
			}
			catch (DirectoryNodeNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(this.Node.Address);
			}

			return this;
		}

		[NodeAttribute]
		public virtual string DriveFormat
		{
			get
			{
				if (!this.Exists)
				{
					return null;
				}

				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						if (driveInfo.IsReady)
						{
							try
							{
								return driveInfo.DriveFormat;
							}
							catch (IOException)
							{
							}
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual long? TotalSize
		{
			get
			{
				if (!this.Exists)
				{
					return null;
				}

				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						if (driveInfo.IsReady)
						{
							try
							{
								return driveInfo.TotalSize;
							}
							catch (IOException)
							{
							}
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual long? TotalFreeSpace
		{
			get
			{
				if (!this.Exists)
				{
					return null;
				}

				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						if (driveInfo.IsReady)
						{
							try
							{
								return driveInfo.TotalFreeSpace;
							}
							catch (IOException)
							{
							}
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual string VolumeLabel
		{
			get
			{
				if (!this.Exists)
				{
					return null;
				}

				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						if (driveInfo.IsReady)
						{
							try
							{
								return driveInfo.VolumeLabel;
							}
							catch (IOException)
							{
							}
						}
					}
				}

				return null;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				driveInfo.VolumeLabel = value;
			}
		}
		
		[NodeAttribute]
		public virtual long? AvailableFreeSpace
		{
			get
			{
				if (!this.Exists)
				{
					return null;
				}

				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						if (driveInfo.IsReady)
						{
							try
							{
								return driveInfo.AvailableFreeSpace;
							}
							catch (IOException)
							{
							}
						}
					}
				}

				return null;
			}
			set
			{
				this.availableFreeSpace = value;
			}
		}
		private long? availableFreeSpace;

		protected override object GetValue(string name)
		{
			if (name.StartsWith("extended:"))
			{
				return GetExtendedAttribute(name.Substring("extended:".Length));
			}
			else
			{
				return base.GetValue(name);
			}
		}

		protected override void SetValue(string name, object value)
		{
			if (name.StartsWith("extended:"))
			{
				SetExtendedAttribute(name.Substring("extended:".Length), value);
			}
			else
			{
				base.SetValue(name, value);
			}
		}		

		private void SetExtendedAttribute(string name, object value)
		{
			byte[] buffer = null;

			if (value != null)
			{
				if ((buffer = value as byte[]) == null)
				{
					buffer = global::System.Text.Encoding.UTF8.GetBytes(value.ToString());
				}
			}

			Native.GetInstance().SetExtendedAttribute(fileSystemInfo.FullName, name, buffer, 0, buffer == null ? 0 : buffer.Length);
		}

		private object GetExtendedAttribute(string name)
		{
			try
			{
				return Native.GetInstance().GetExtendedAttribute(fileSystemInfo.FullName, name);
			}
			catch (FileNodeNotFoundException)
			{
				throw new FileNodeNotFoundException(this.Node.Address);
			}
			catch (NotSupportedException)
			{
				return null;
			}
		}

		[NodeAttribute]
		public virtual string DriveType
		{
			get
			{
				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						try
						{
							if (driveInfo.DriveType == global::System.IO.DriveType.Removable)
							{
								if (driveInfo.Name.Equals("A:\\", StringComparison.CurrentCultureIgnoreCase)
									|| driveInfo.Name.Equals("B:\\", StringComparison.CurrentCultureIgnoreCase))
								{
									return "floppy";
								}
							}

							if (driveInfo.DriveType == global::System.IO.DriveType.Unknown)
							{
								if (driveInfo.Name.ToLower().Contains("cdrom"))
								{
									return "cdrom";
								}
								else if (driveInfo.Name.ToLower().Contains("usb"))
								{
									return "removable";
								}
								else if (driveInfo.Name.ToLower().Contains("floppy"))
								{
									return "floppy";
								}
								else if (driveInfo.Name.ToLower().Contains("/fd"))
								{
									return "floppy";
								}
								else if (driveInfo.Name.ToLower().Contains("removeable"))
								{
									return "removable";
								}
								else
								{
									return "fixed";
								}
							}

							return driveInfo.DriveType.ToString();
						}
						catch (IOException)
						{
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual string MountPath
		{
			get
			{
				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						try
						{
							return driveInfo.Name;
						}
						catch (IOException)
						{
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual bool? IsReady
		{
			get
			{
				if (driveInfo != null)
				{
					lock (typeof(DriveInfo))
					{
						try
						{
							return driveInfo.IsReady;
						}
						catch (IOException)
						{
						}
					}
				}

				return null;
			}
		}

		[NodeAttribute]
		public virtual bool? IsDrive
		{
			get
			{
				return driveInfo == null ? null : (bool?)true;
			}
		}
	}
}
