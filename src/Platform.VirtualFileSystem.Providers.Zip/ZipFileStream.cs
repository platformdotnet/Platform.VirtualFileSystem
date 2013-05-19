using System.IO;
using Platform.IO;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	internal class ZipFileStream
		: StreamWrapper
	{
		private readonly ZipFile zipFile;

		public static ZipFileStream CreateInputStream(ZipFile zipFile)
		{
			zipFile.Refresh();

			var zipEntry = ((IZipNode)zipFile).ZipEntry;

			if (zipEntry == null)
			{
				throw new FileNotFoundException(zipFile.Address.Uri);
			}

			var tempFile = ((ZipFileSystem)zipFile.FileSystem).GetTempFile(zipFile, false);

			if (tempFile != null)
			{
				return new ZipFileStream(zipFile, tempFile.GetContent().GetInputStream());
			}
			else
			{
				return new ZipFileStream(zipFile, ((ZipFileSystem)zipFile.FileSystem).GetInputStream(zipEntry));
			}
		}

		public static ZipFileStream CreateOutputStream(ZipFile zipFile)
		{
			var tempFile = ((ZipFileSystem)zipFile.FileSystem).GetTempFile(zipFile, true);

			return new ZipFileStream(zipFile, tempFile.GetContent().GetOutputStream());
		}

		private ZipFileStream(ZipFile zipFile, Stream stream)
			: base(stream)
		{
			this.zipFile = zipFile;
		}
	}
}