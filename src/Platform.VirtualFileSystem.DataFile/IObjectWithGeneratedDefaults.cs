namespace Platform.VirtualFileSystem.DataFile
{
	public interface IObjectWithGeneratedDefaults
	{
		bool RequiresGeneratedDefaults
		{
			get;
		}

		void GenerateDefaults();
	}
}
