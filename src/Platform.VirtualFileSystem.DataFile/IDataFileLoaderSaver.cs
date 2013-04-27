namespace Platform.VirtualFileSystem.DataFile
{
	public interface IDataFileLoaderSaver<T>
		where T : class
	{
		T Load(DataFile<T> dataFile);
		void Save(DataFile<T> dataFile);
	}
}
