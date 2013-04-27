using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public class FileTransferServiceEventArgs
	{
		public virtual TransferState LastTransferState { get; set; }

		public virtual TransferState CurrentTransferState { get; set; }

		public FileTransferServiceEventArgs(TransferState lastTransferState, TransferState currentTransferState)
		{
			this.LastTransferState = lastTransferState;
			this.CurrentTransferState = currentTransferState;
		}
	}
}