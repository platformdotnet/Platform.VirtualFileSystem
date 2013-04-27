namespace Platform.VirtualFileSystem
{
	public class TempIdentityFileServiceType
		: ServiceType
	{
		public virtual string UniqueIdentifier { get; set; }
		public virtual IFileSystem TempFileSystem { get; set; }

		public TempIdentityFileServiceType(string uniqueIdentifier)
			: this(uniqueIdentifier, null)
		{
		}

		public TempIdentityFileServiceType(string uniqueIdentifier, IFileSystem tempFileSystem)
		{
			this.TempFileSystem = tempFileSystem;
			this.UniqueIdentifier = uniqueIdentifier;
		}
	}
}