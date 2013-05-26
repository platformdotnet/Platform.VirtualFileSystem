using System;
using ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipFileInfo
		: ZipNodeInfo
	{
		public IFile ShadowFile { get; private set; }

		public override bool Exists
		{
			get
			{
				if (this.ShadowFile != null)
				{
					return true;
				}

				return base.Exists;
			}
		}

		public override DateTime? DateTime
		{
			get
			{
				if (this.ShadowFile != null)
				{
					return this.ShadowFile.Attributes.CreationTime;
				}

				return base.DateTime;
			}
		}

		public ZipFileInfo(ZipEntry zipEntry)
			: base(zipEntry)
		{
			
		}

		public virtual IFile GetShadowFile(bool createIfNecessary)
		{
			if (this.ShadowFile != null)
			{
				return this.ShadowFile;
			}

			if (!createIfNecessary)
			{
				return null;
			}

			var uniqueId = Guid.NewGuid();

			var retval = FileSystemManager.Default.ResolveFile("temp:///" + uniqueId.ToString("N") + ".zipfs.tmp");

			retval.Create();
			
			this.ShadowFile = retval;

			return retval;
		}

		public override void Create()
		{
			this.GetShadowFile(true).Create();

			base.Create();
		}

		public override void Delete()
		{
			base.Delete();
			
			if (this.ShadowFile != null)
			{
				try
				{
					this.ShadowFile.Delete();
				}
				catch
				{	
				}
			}

			this.ShadowFile = null;
		}
	}
}
