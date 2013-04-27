namespace Platform.VirtualFileSystem
{
	public class FileComparingServiceType
		: ServiceType
	{
		public virtual FileComparingFlags Flags { get; set; }
		public virtual IFile TargetFile { get; set; }

		public FileComparingServiceType(IFile targetFile, FileComparingFlags flags)
		{
			this.Flags = flags;
			this.TargetFile = targetFile;
		}
	}
}