using System;
using System.IO;
using Platform.IO;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkFile
		: AbstractFile
	{
		public new NetworkFileSystem FileSystem
		{
			get
			{
				return (NetworkFileSystem)base.FileSystem;
			}
		}

		public new NetworkNodeAddress Address
		{
			get
			{
				return (NetworkNodeAddress)base.Address;
			}
		}

		public NetworkFile(IFileSystem fileSystem, INodeAddress address)
			: base(address, fileSystem)
		{
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new NetworkNodeAndFileAttributes(this);
		}

		public override IService GetService(ServiceType serviceType)
		{
			if (serviceType is FileHashingServiceType)
			{
				return new NetworkFileHashingService(this, (FileHashingServiceType)serviceType);
			}
			else if (serviceType is StreamHashingServiceType)
			{
				return new NetworkStreamHashingService(this, (StreamHashingServiceType)serviceType);
			}

			return base.GetService(serviceType);
		}

		protected override IFile DoCreateHardLink(IFile targetFile, bool overwrite)
		{
			NetworkFileSystem.FreeClientContext clientContext = null;

			// Get a new client context

			if (!targetFile.FileSystem.Equals(this.FileSystem))
			{
				throw new NotSupportedException("CreateHardLink/ForiegnFilesystem");
			}

			using (clientContext = this.FileSystem.GetFreeClientContext())			
			{
				clientContext.NetworkFileSystemClient.CreateHardLink(((NetworkNodeAddress)this.Address).RemoteUri, ((NetworkNodeAddress)targetFile.Address).RemoteUri, overwrite);
			}

			return this;
		}

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode fileMode, FileShare fileShare)
		{
			encoding = null;

			return DoOpenStream(contentName, fileMode, FileAccess.Read, fileShare);
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode fileMode, FileShare fileShare)
		{
			return DoOpenStream(contentName, fileMode, FileAccess.Write, fileShare);
		}

		protected override Stream DoOpenStream(string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			Stream stream = null;
			NetworkFileSystem.FreeClientContext clientContext = null;

			// Get a new client context optimised for binary access

			clientContext = this.FileSystem.GetFreeClientContext(true);

			try
			{
				// Create a new RandomAcessStream based on the client

				stream = clientContext.NetworkFileSystemClient.OpenRandomAccessStream(this.Address.RemoteUri, fileMode, fileAccess, fileShare);

				// Make sure the client context is disposed after the stream is closed

				((IStreamWithEvents)stream).AfterClose += delegate
				{					
					clientContext.Dispose();

					if (fileAccess == FileAccess.ReadWrite || fileAccess == FileAccess.Write)
					{
						((NetworkNodeAndFileAttributes)this.Attributes).SetValue<bool>("exists", true);
						((NetworkNodeAndFileAttributes)this.Attributes).SetValue<long>("length", stream.Length);
					
						((NetworkDirectory)this.ParentDirectory).AddChild(this);

						NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)this.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Changed, this.NodeType, this.Name, this.Address.AbsolutePath));
					}
				};
			}
			finally
			{				
				if (stream == null)
				{
					// Explicitly dispose the client context because the
					// stream can't do it cause it doesn't exist

					clientContext.Dispose();
				}
			}

			return stream;
		}

		//
		// Create/Delete/Copy/Move
		//

		public override INode DoCreate(bool createParent)
		{			
			NetworkNode.DoCreate(this, createParent);

			return this;
		}

		protected override INode DoDelete()
		{
			NetworkNode.DoDelete(this, false);

			return this;
		}

		protected override INode DoCopyTo(INode target, bool overwrite)
		{
			NetworkNode.DoCopyTo(this, target, base.CopyTo, overwrite);

			return this;
		}

		protected override INode DoMoveTo(INode target, bool overwrite)
		{
			NetworkNode.DoMoveTo(this, target, base.MoveTo, overwrite);

			return this;
		}
	}
}
