using System;
using ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal abstract class ZipNodeInfo
	{
		protected bool exists;

		public string AbsolutePath { get; set; }
		public ZipEntry ZipEntry { get; set; }

		protected ZipNodeInfo()
		{	
		}

		protected ZipNodeInfo(ZipEntry zipEntry)
		{
			this.ZipEntry = zipEntry;

			exists = zipEntry != null;
		}
		
		public virtual bool Exists
		{
			get
			{
				return exists;
			}
		}

		public virtual DateTime? DateTime
		{
			get
			{
				if (this.ZipEntry != null)
				{
					return this.ZipEntry.DateTime;
				}

				return null;
			}
		}

		public virtual void Create()
		{
			exists = true;
		}

		public virtual void Delete()
		{
			exists = false;
		}
	}
}
