using System.IO;

namespace Platform.VirtualFileSystem
{
	public interface IStreamHashingService
		: IHashingService
	{
		Stream OperatingStream
		{
			get;
		}
	}
}