namespace Platform.VirtualFileSystem.Providers
{
	public class JumpPointDirectory
		: DirectoryDelegationWrapper
	{
		public override INodeAddress Address
		{
			get
			{
				return this.address;
			}
		}
		private readonly INodeAddress address;

		public JumpPointDirectory(IDirectory dir, INodeAddress address)
			: base(dir, new JumpPointResolver(dir), ConverterUtils<INode, INode>.NoConvert)
		{
			this.address = address;
		}
	}
}
