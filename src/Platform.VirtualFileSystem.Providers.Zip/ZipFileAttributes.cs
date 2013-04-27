namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipFileAttributes
		: ZipNodeAttributes
	{
		public ZipFileAttributes(IZipNode zipNode)
			: base(zipNode)
		{			
		}

		[NodeAttribute]
		public virtual long Length
		{
			get
			{
				lock (this)
				{
					VerifyZipEntry();

					return this.zipNode.ZipEntry.Size;
				}				
			}
		}
	}
}
