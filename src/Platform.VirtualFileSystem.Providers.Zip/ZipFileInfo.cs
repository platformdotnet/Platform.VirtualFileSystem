using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ZipFileInfo
	{
		private bool created;

		public ZipEntry ZipEntry { get; set; }
		private IFile ShadowFile { get; set; }

		public bool Exists
		{
			get
			{
				if (this.ShadowFile != null)
				{
					return true;
				}

				if (this.ZipEntry != null)
				{
					return true;
				}

				return created;
			}
		}

		public DateTime? DateTime
		{
			get
			{
				if (this.ShadowFile != null)
				{
					return this.ShadowFile.Attributes.CreationTime;
				}

				if (this.ZipEntry != null)
				{
					return this.ZipEntry.DateTime;
				}

				return null;
			}
		}

		public IFile GetShadowFile(bool createIfNecessary)
		{
			var uniqueId = Guid.NewGuid();

			var retval = FileSystemManager.Default.ResolveFile("tmp:///" + uniqueId.ToString("N")).Refresh();

			if (!retval.Exists)
			{
				retval.Create();
			}

			this.ShadowFile = retval;

			return retval;
		}

		public void Create()
		{
			
		}

		public void Delete()
		{
			created = false;

			this.ZipEntry = null;
			this.Exists = false;
			this.DateTime = null;

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
