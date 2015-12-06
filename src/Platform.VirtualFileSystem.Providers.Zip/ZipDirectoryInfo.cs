namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipDirectoryInfo
		: ZipNodeInfo
	{
		public ZipDirectoryInfo(bool exists)
		{
			this.exists = exists;
		}
	}
}
