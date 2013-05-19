using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class StreamDataSource
		: IStaticDataSource
	{
		private readonly Stream stream;

		public StreamDataSource(Stream stream)
		{
			this.stream = stream;
		}

		public Stream GetSource()
		{
			return stream;
		}
	}
}
