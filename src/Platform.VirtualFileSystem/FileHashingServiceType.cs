namespace Platform.VirtualFileSystem
{
	public class FileHashingServiceType
		: HashingServiceType
	{
		public FileHashingServiceType()
			: this("md5")
		{
		}

		public FileHashingServiceType(string algorithmName)
			: base(typeof(IFileHashingService), algorithmName)
		{
		}
	}
}