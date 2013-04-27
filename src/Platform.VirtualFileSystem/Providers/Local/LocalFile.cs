using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Platform.VirtualFileSystem.Providers.Local
{
	/// <summary>
	/// Implementation of <see cref="IFile"/> for <see cref="LocalFileSystem"/>s.
	/// </summary>
	public class LocalFile
		: AbstractFile, INativePathService
	{	
		private readonly FileInfo fileInfo;
		
		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}

		public LocalFile(LocalFileSystem fileSystem, LocalNodeAddress address)
			: base(address, fileSystem)
		{
			this.FileSystem = fileSystem;
			this.fileInfo = new FileInfo(address.AbsoluteNativePath);
		}
		
		public override INode DoCreate(bool createParent)
		{
			if (createParent)
			{
				this.ParentDirectory.Refresh();

				if (!this.ParentDirectory.Exists)
				{
					this.ParentDirectory.Create(true);
				}

				try
				{
					this.fileInfo.Create().Close();
				}
				catch (DirectoryNotFoundException)
				{
				}

				this.ParentDirectory.Create(true);

				try
				{
					this.fileInfo.Create().Close();
				}
				catch (DirectoryNotFoundException)
				{
					throw new DirectoryNodeNotFoundException(this.Address);	
				}
				catch (FileNotFoundException)
				{
					throw new FileNodeNotFoundException(this.Address);	
				}

				Refresh();
			}
			else
			{
				try
				{
					this.fileInfo.Create().Close();
				}
				catch (DirectoryNotFoundException)
				{
					throw new DirectoryNodeNotFoundException(this.Address);	
				}
				catch (FileNotFoundException)
				{
					throw new FileNodeNotFoundException(this.Address);	
				}
			}

			return this;
		}

		protected override INode DoDelete()
		{
			try
			{
				Native.GetInstance().DeleteFileContent(this.fileInfo.FullName, null);

				Refresh();
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(Address);
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(Address);
			}

			return this;
		}

		protected override INode DoCopyTo(INode target, bool overwrite)
		{
			return CopyTo(target, overwrite, false);
		}

		private INode CopyTo(INode target, bool overwrite, bool deleteOriginal)
		{
			string targetLocalPath = null;

			if (target.NodeType.IsLikeDirectory)
			{
				target = target.ResolveFile(this.Address.Name);
			}

			try
			{
				var service = (INativePathService)target.GetService(typeof(INativePathService));

				targetLocalPath = service.GetNativePath();
			}
			catch (NotSupportedException)
			{
			}

			var thisLocalPath = ((LocalNodeAddress)this.Address).AbsoluteNativePath;

			if (targetLocalPath == null
				|| !Path.GetPathRoot(thisLocalPath).EqualsIgnoreCase(Path.GetPathRoot(targetLocalPath)))
			{
				if (deleteOriginal)
				{
					base.DoMoveTo(target, overwrite);
				}
				else
				{
					base.DoCopyTo(target, overwrite);
				}
			}
			else
			{
				target.Refresh();
				
				if (overwrite && target.Exists)
				{
					try
					{
						target.Delete();
					}
					catch (FileNodeNotFoundException)
					{
					}
				}

				try
				{
					if (deleteOriginal)
					{
						try
						{
							File.Move(thisLocalPath, targetLocalPath);
						}
						catch (System.IO.DirectoryNotFoundException)
						{
							throw new DirectoryNodeNotFoundException(this.Address);	
						}
						catch (System.IO.FileNotFoundException)
						{
							throw new FileNodeNotFoundException(this.Address);	
						}
					}
					else
					{
						try
						{
							File.Copy(thisLocalPath, targetLocalPath);
						}
						catch (System.IO.DirectoryNotFoundException)
						{
							throw new DirectoryNodeNotFoundException(this.Address);	
						}
						catch (System.IO.FileNotFoundException)
						{
							throw new FileNodeNotFoundException(this.Address);	
						}
					}

					return this;
				}
				catch (IOException)
				{
					if (overwrite && target.Exists)
					{
						try
						{
							target.Delete();
						}
						catch (FileNotFoundException)
						{
						}
					}
					else
					{
						throw;
					}
				}

				if (deleteOriginal)
				{
					try
					{
						File.Move(thisLocalPath, targetLocalPath);
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						throw new DirectoryNodeNotFoundException(this.Address);	
					}
					catch (System.IO.FileNotFoundException)
					{
						throw new FileNodeNotFoundException(this.Address);	
					}
				}
				else
				{
					try
					{
						File.Copy(thisLocalPath, targetLocalPath);
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						throw new DirectoryNodeNotFoundException(this.Address);	
					}
					catch (System.IO.FileNotFoundException)
					{
						throw new FileNodeNotFoundException(this.Address);	
					}
				}
			}

			return this;
		}

		protected override INode DoMoveTo(INode target, bool overwrite)
		{
			return CopyTo(target, overwrite, true);
		}

		private class LocalFileMovingService
			: AbstractRunnableService
		{
			private readonly LocalFile localFile;
			private readonly LocalFile destinationFile;
			private readonly bool overwrite;

			public override IMeter Progress
			{
				get
				{
					return this.progress;
				}
			}
			private readonly MutableMeter progress = new MutableMeter(0, 1, 0, "files");

			public LocalFileMovingService(LocalFile localFile, LocalFile destinationFile, bool overwrite)
			{
				this.localFile = localFile;
				this.destinationFile = destinationFile;
				this.overwrite = overwrite;
			}

			public override void DoRun()
			{
				this.progress.SetCurrentValue(0);
				this.localFile.DoMoveTo(this.destinationFile, this.overwrite);
				this.progress.SetCurrentValue(1);
			}

		}

		public override IService GetService(ServiceType serviceType)
		{
			var typedServiceType = serviceType as NodeMovingServiceType;

			if (typedServiceType != null)
			{
				if (typedServiceType.Destination is LocalFile)
				{
					if (typedServiceType.Destination.Address.RootUri.Equals(this.Address.RootUri))
					{
						return new LocalFileMovingService(this, (LocalFile)typedServiceType.Destination, typedServiceType.Overwrite);
					}
				}
			}
			else if (serviceType.Is(typeof(INativePathService)))
			{
				return this;
			}
			
			return base.GetService(serviceType);
		}

		protected override Stream DoOpenStream(string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			try
			{
				if (string.IsNullOrEmpty(contentName) || contentName == Native.GetInstance().DefaultContentName)
				{					
					return new FileStream(this.fileInfo.FullName, fileMode, fileAccess, fileShare);
				}
				else
				{
					return Native.GetInstance().OpenAlternateContentStream
					(
						this.fileInfo.FullName,
						contentName,
						fileMode, fileAccess, fileShare
					);
				}
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(Address);
			}
		}

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode mode, FileShare sharing)
		{
			encoding = null;

			try
			{
				if (string.IsNullOrEmpty(contentName)
					|| contentName == Native.GetInstance().DefaultContentName)
				{
					for (var i = 0; i < 10; i++)
					{
						try
						{
							return new FileStream(this.fileInfo.FullName, mode, FileAccess.Read, sharing);
						}
						catch (DirectoryNotFoundException)
						{
							throw;
						}
						catch (FileNotFoundException)
						{
							throw;
						}
						catch (IOException)
						{
							if (i == 9)
							{
								throw;
							}

							Thread.Sleep(500);
						}
					}

					throw new IOException();
				}
				else
				{
					return Native.GetInstance().OpenAlternateContentStream
					(
						this.fileInfo.FullName,
						contentName,
						mode, FileAccess.Read, sharing
					);
				}
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(Address);
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(Address);
			}
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			try
			{
				if (string.IsNullOrEmpty(contentName) || contentName == Native.GetInstance().DefaultContentName)
				{
					return new FileStream(this.fileInfo.FullName, mode, FileAccess.Write, sharing);
				}
				else
				{
					return Native.GetInstance().OpenAlternateContentStream
					(
						this.fileInfo.FullName,
						contentName,
						mode, FileAccess.Write, sharing
					);
				}
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(Address);
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(Address);
			}
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new AutoRefreshingFileAttributes(new LocalFileAttributes(this, this.fileInfo), -1);
		}

		protected override bool SupportsAlternateContent()
		{
			return true;
		}

		string INativePathService.GetNativePath()
		{
			return ((LocalNodeAddress)this.Address).AbsoluteNativePath;
		}

		string INativePathService.GetNativeShortPath()
		{
			return Native.GetInstance().GetShortPath(((LocalNodeAddress)this.Address).AbsoluteNativePath);
		}

		public override string DefaultContentName
		{
			get
			{
				return Native.GetInstance().DefaultContentName;
			}
		}

		public override System.Collections.Generic.IEnumerable<string> GetContentNames()
		{
			return Native.GetInstance().GetContentInfos(((LocalNodeAddress)this.Address).AbsoluteNativePath).Select(contentInfo => contentInfo.Name);
		}

		protected override void DeleteContent(string contentName)
		{
			if (contentName == null)
			{
				contentName = Native.GetInstance().DefaultContentName;
			}

			try
			{
				Native.GetInstance().DeleteFileContent(((LocalNodeAddress)this.Address).AbsoluteNativePath, contentName);
			}
			catch (FileNotFoundException)
			{
				throw new FileNodeNotFoundException(Address);
			}
			catch (DirectoryNotFoundException)
			{
				throw new DirectoryNodeNotFoundException(Address);
			}
		}

		protected override INode DoRenameTo(string name, bool overwrite)
		{
			name = StringUriUtils.RemoveQuery(name);

			var destPath = Path.Combine(this.fileInfo.DirectoryName, name);

			for (var i = 0; i < 5; i++)
			{
				try
				{
					if (overwrite)
					{
						File.Delete(destPath);
					}

					//
					// Don't use FileInfo.MoveTo as it changes the existing FileInfo
					// use the new path.
					//

					File.Move(this.fileInfo.FullName, destPath);
					this.fileInfo.Refresh();

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

		protected override IFile DoCreateHardLink(IFile targetFile, bool overwrite)
		{
			string target;
			var path = this.fileInfo.FullName;

			try
			{
				var service = (INativePathService)targetFile.GetService(typeof(INativePathService));

				target = service.GetNativePath();
			}
			catch (NotSupportedException)
			{
				throw new NotSupportedException();
			}

			targetFile.Refresh();

			if (this.Exists && !overwrite)
			{
				throw new IOException("Hardlink already exists");
			}
			else
			{
				ActionUtils.ToRetryAction<object>
				(
					delegate
					{
						if (this.Exists)
						{
							this.Delete();
						}

						try
						{
							Native.GetInstance().CreateHardLink(path, target);
						}
						catch (TooManyLinksException)
						{
							throw new TooManyLinksException(this, targetFile);
						}
					},
					TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.25)
				)(null);
			}

			return this;
		}
	}
}
