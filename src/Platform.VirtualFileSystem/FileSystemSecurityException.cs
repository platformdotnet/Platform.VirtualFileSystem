using System.IO;

namespace Platform.VirtualFileSystem
{
	public class FileSystemSecurityException
		: IOException
	{
		public virtual INodeAddress NodeAddress { get; set; }

		public FileSystemSecurityException(INodeAddress address)
		{
			this.NodeAddress = address;
		}	
	}
}