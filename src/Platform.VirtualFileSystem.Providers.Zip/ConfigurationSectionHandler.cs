using System.Configuration;
using System.Xml;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	public class ConfigurationSectionHandler
		: IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			var serializer = XmlSerializer<ConfigurationSection>.New();
			var config = (ConfigurationSection)serializer.Deserialize(new XmlNodeReader(section));

			config.SetXmlNode(section);

			return config;
		}
	}
}