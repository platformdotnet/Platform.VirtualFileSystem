namespace Platform.VirtualFileSystem.Providers
{
	public class StandardFileMovingService
		: StandardFileCopyingService
	{
		public StandardFileMovingService(IFile sourceFile, NodeMovingServiceType serviceType)
			: base(sourceFile, new NodeCopyingServiceType(serviceType.Destination, serviceType.Overwrite, serviceType.BufferSize))
		{
		}

		public override void Run()
		{
			base.Run();

			this.OperatingNode.Delete();
		}
	}
}
