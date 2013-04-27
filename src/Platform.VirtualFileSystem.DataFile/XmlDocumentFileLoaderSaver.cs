using System.IO;
using System.Xml;

namespace Platform.VirtualFileSystem.DataFile
{
	public class XmlDocumentFileLoaderSaver
		: IDataFileLoaderSaver<XmlDocument>
	{
		public static XmlDocumentFileLoaderSaver Instance
		{
			get
			{
				return instance;
			}
		}
		private static readonly XmlDocumentFileLoaderSaver instance = new XmlDocumentFileLoaderSaver();

		public virtual XmlDocument Load(DataFile<XmlDocument> dataFile)
		{			
			using (var reader = dataFile.File.GetContent().GetReader(FileShare.Read))
			{
				var document = new XmlDocument();

				document.Load(reader);

				return document;
			}
		}

		public virtual void Save(DataFile<XmlDocument> dataFile)
		{
			using (var writer = dataFile.File.GetContent().GetWriter(FileShare.None))
			{
				dataFile.Value.Save(writer);
			}
		}
	}
}
