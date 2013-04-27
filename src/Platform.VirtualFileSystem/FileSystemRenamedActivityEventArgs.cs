namespace Platform.VirtualFileSystem
{
	public class FileSystemRenamedActivityEventArgs
		: FileSystemActivityEventArgs
	{
		public virtual string NewPath { get; set; }

		public virtual string NewName { get; set; }

		public FileSystemRenamedActivityEventArgs(FileSystemActivity activity, NodeType nodeType, string name, string path, string newName, string newPath)
			: base(activity, nodeType, name, path)
		{
			this.NewPath = newPath;
			this.NewName = newName;
		}
	}
}