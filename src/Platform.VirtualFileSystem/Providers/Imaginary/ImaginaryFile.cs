namespace Platform.VirtualFileSystem.Providers.Imaginary
{
	public class ImaginaryFile
		: AbstractFile
	{
		public ImaginaryFile(IFileSystem fileSystem, INodeAddress address)
			: base(address, fileSystem)
		{			
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new DictionaryBasedNodeAttributes();
		}
	}
}
