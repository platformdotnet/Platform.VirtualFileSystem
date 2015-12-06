namespace Platform.VirtualFileSystem.Providers
{
	public class JumpPointFile
		: FileWrapper
	{
		public override INodeAddress Address
		{
			get
			{
				return this.address;
			}
		}
		private INodeAddress address;

		public JumpPointFile(IFile file, INodeAddress address)
			: base(file)
		{
			this.address = address;
		}
	}
}
