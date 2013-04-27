using System;
using System.Xml;
using System.Configuration;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Network.Server
{
	public class ConfigurationSectionHandler
		: IConfigurationSectionHandler
	{
		public object Create(object parent, object configContext, XmlNode section)
		{
			var serializer = XmlSerializer<ConfigurationSection>.New();

			var config = serializer.Deserialize(new XmlNodeReader(section));

			config.SetXmlNode(section);

			return config;
		}
	}
	
	[XmlElement("Configuration")]
	public class ConfigurationSection
	{
		public static ConfigurationSection Instance
		{
			get
			{
				if (instance == null)
				{
					lock (typeof(ConfigurationSection))
					{
#pragma warning disable 618
						instance = (ConfigurationSection)ConfigurationSettings.GetConfig("Platform/VirtualFileSystem/Network/Server/Configuration");
#pragma warning restore 618

						if (instance == null)
						{
							instance = new ConfigurationSection();
						}
					}
				}

				return instance;
			}
		}
		private static ConfigurationSection instance;

		public virtual XmlNode XmlNode { get; private set; }

		[XmlElement(SerializeAsValueNode=true, ValueNodeAttributeName="type")]
		public virtual Type CommandProcessorProvider { get; set; }

		internal virtual void SetXmlNode(XmlNode node)
		{
			this.XmlNode = node;
		}

		[XmlElement]
		public virtual int MaxThreadPoolCount { get; set; }

		public ConfigurationSection()
		{
			this.MaxThreadPoolCount = 250;
		}
	}
}
