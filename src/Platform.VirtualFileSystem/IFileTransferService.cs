using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public delegate void FileTransferServiceEventHandler(object sender, FileTransferServiceEventArgs eventArg);

	public interface IFileTransferService
		: INodeCopyingService
	{
		event FileTransferServiceEventHandler TransferStateChanged;

		TransferState TransferState
		{
			get;
		}
	}
}
