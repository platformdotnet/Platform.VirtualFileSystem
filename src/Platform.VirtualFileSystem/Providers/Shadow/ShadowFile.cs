using System;

namespace Platform.VirtualFileSystem.Providers.Shadow
{
	/// <summary>
	/// Represents a file whose content is shadowed in a temporary file system.
	/// </summary>
	/// <remarks>
	/// <p>
	/// ShadowFiles can be resolved by adding shadow=true to a file URI's query string.  A ShadowFile
	/// creates a temporary copy of the file for each InputStream and OutputStream created.  Shadowed
	/// InputStreams may be shared because they don't change whereas shadowed OutputStreams are unique
	/// to every call to <see cref="ShadowFile.GetContent().GetOutputStream()"/>.
	/// </p>
	/// <p>
	/// If the file being shadowed changes after an shadow InputStream to that file has been created, 
	/// the InputStream will continue to refer to a copy of the unchanged file until it is disposed.
	/// A new call to <see cref="ShadowFile.GetContent().GetOutputStream()"/> will create and return
	/// a shadow InputStream that will refer to the changed data in the shadowed file (the original file
	/// the ShadowFile is shadowing).
	/// </p>
	/// <p>
	/// Data written to shadow OutputStreams will not be reflected in the shadowed file until the
	/// shadow OutputStream is closed.  To prevent incomplete data from being reflected, shadow
	/// OutputStreams must be explicitly closed with a call to close.  The stream finalizer will not 
	/// automatically reflect the written data to the shadowed file.
	/// </p>
	/// <p>
	/// Users must be careful when writing to ShadowFiles.  Any data written to the shadowed file after 
	/// a shadow OutputStream has been created but before the OutputStream is closed  will be lost when 
	/// the OutputStream is closed.  Also, any data written to a shadow OutputStream will be lost if
	/// the OutputStream is never closed (this can happen if the application crashes and even a call to 
	/// flush will not prevent this data loss).  Data written to a shadow OutputStream is reflected
	/// only after the shadow OutputStream is closed which helps ensure that the shadowed file will
	/// always reflect a file in its entirety (atmocitiy).
	/// </p>
	/// <p>
	/// ShadowFiles support is part of the CoreNodeServices included in all FileSystems.
	/// </p>
	/// </remarks>
	internal class ShadowFile
		: FileWrapper
	{
		private readonly INodeAddress address;
		private readonly IFileSystem tempFileSystem;

		public override INodeAddress Address
		{
			get
			{
				return address;
			}
		}

		internal IFile ShadowedFile
		{
			get
			{
				return this.Wrappee;
			}
		}

		public ShadowFile(IFile file, INodeAddress address)
			: this(file, address, FileSystemManager.GetManager().Resolve("temp:///").FileSystem)
		{			
		}

		public ShadowFile(IFile file, INodeAddress address, IFileSystem tempFileSystem)
			: base(file)
		{
			this.address = address;
			this.tempFileSystem = tempFileSystem;
		}

		public override IService GetService(ServiceType serviceType)
		{
			if (serviceType.Equals(ServiceType.NativePathServiceType))
			{
				var retval = ((ShadowNodeContent)GetContent()).GetReadFile(TimeSpan.FromMinutes(1)).GetService(serviceType);

				if (retval != null)
				{
					return retval;
				}
			}

			try
			{
				return this.FileSystem.GetService(this, serviceType);
			}
			catch (NotSupportedException)
			{
			}

			return base.GetService (serviceType);
		}

		public override INodeContent GetContent()
		{
			if (content == null)
			{
				lock (this.SyncLock)
				{
					if (content == null)
					{
						var localContent = new ShadowNodeContent(this, this.Wrappee, tempFileSystem);

						System.Threading.Thread.MemoryBarrier();

						content = localContent;
					}
				}
			}

			return content;
		}
		private INodeContent content;
	}
}
