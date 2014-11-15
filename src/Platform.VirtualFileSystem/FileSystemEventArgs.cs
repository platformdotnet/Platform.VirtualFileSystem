using System;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Event handler for file system activity events.
	/// </summary>
	/// <remarks>
	/// <seealso cref="FileSystemActivityEventArgs"/>
	/// </remarks>
	public delegate void FileSystemActivityEventHandler(object sender, FileSystemActivityEventArgs eventArgs);

	/// <summary>
	/// Provides state information about file system activity events.
	/// </summary>
	public class FileSystemActivityEventArgs
		: EventArgs
	{
		public virtual FileSystemActivity Activity { get; private set; }

		public string Path { get; set; }
		public string Name { get; set; }
		public NodeType NodeType { get; set; }
		
		public FileSystemActivityEventArgs(FileSystemActivity activity, NodeType nodeType, string name, string path)
		{
			this.Activity = activity;
			this.Name = name;
			this.Path = path;
			this.NodeType = nodeType;
		}
	}
}
