using System.IO;

namespace Platform.VirtualFileSystem.Providers.Local
{
	internal class LocalFileAttributes
		: LocalNodeAttributes, IFileAttributes
	{
		public LocalFileAttributes(INode node, FileInfo fsInfo)
			: base(node, fsInfo)
		{
		}

		[NodeAttribute]
		public virtual long? Length
		{
			get
			{
				CheckRefresh();

				if (!this.Exists)
				{
					return null;
				}

				try
				{
					return ((FileInfo)fileSystemInfo).Length;
				}
				catch (FileNotFoundException)
				{
					return null;
				}
			}
		}

		IFileAttributes IFileAttributes.Refresh()
		{			
			return (IFileAttributes)Refresh();
		}
	}
}
