namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// The fle system activities that can be monitored.
	/// </summary>
	/// <remarks>
	/// <seealso cref="FileSystemActivityEventArgs"/>
	/// </remarks>
	public enum FileSystemActivity
	{
		Changed,
		Created,
		Deleted,
		Renamed
	}
}