namespace Platform.VirtualFileSystem
{
	public interface ITransactionService
		: IService
	{		
		void Commit();
		void Rollback();
	}
}
