namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipFileAttributes
		: ZipNodeAttributes, IFileAttributes
	{
		private readonly object lockObject = new object();

		public ZipFileAttributes(IZipNode zipNode)
			: base(zipNode)
		{			
		}

		[NodeAttribute]
		public virtual long Length
		{
			get
			{
				lock (lockObject)
				{
					VerifyZipEntry();

					return this.zipNode.ZipEntry.Size;
				}				
			}
		}

		public IFileAttributes Refresh()
		{
			return this;
		}

		long? IFileAttributes.Length => Length;
	}
}
