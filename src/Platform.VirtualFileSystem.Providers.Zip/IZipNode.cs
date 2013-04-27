using ZLib = ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{	
	public interface IZipNode
		: INode
	{
		ZLib.ZipEntry ZipEntry
		{
			get;
		}

		string ZipPath
		{
			get;
		}

		void SetZipEntry(ZLib.ZipEntry zipEntry);
	}
}
