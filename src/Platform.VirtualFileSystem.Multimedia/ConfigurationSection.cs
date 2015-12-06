using System;
using System.Xml;
using System.Configuration;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Multimedia
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
		public static ConfigurationSection GetInstance()
		{
			if (instance == null)
			{
				lock (typeof(ConfigurationSection))
				{
					try
					{	
#pragma warning disable 618
						instance = (ConfigurationSection)ConfigurationSettings.GetConfig("Platform/VirtualFileSystem/Multimedia/Configuration");
#pragma warning restore 618
					}
					catch (Exception e)
					{
						throw e.InnerException;
					}

					if (instance == null)
					{
						instance = new ConfigurationSection();
					}
				}
			}

			return instance;
		}
		private static ConfigurationSection instance;

		public virtual XmlNode XmlNode { get; private set; }

		[XmlElement("MediaFileFactory")]
		public class MediaFileFactoryEntry
		{
			[XmlAttribute("type")]
			[XmlVerifyRuntimeType(typeof(MediaFileFactory))]
			public Type Type;
		}

		[XmlElement]
		public virtual MediaFileFactoryEntry[] MediaFileFactories { get; set; }

		internal virtual void SetXmlNode(XmlNode node)
		{
			this.XmlNode = node;
		}
	}
}
