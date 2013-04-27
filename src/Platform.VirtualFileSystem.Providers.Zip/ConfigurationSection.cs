using System;
using System.Xml;
using System.Configuration;
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
						try
						{
#pragma warning disable 618
							instance = (ConfigurationSection)ConfigurationSettings.GetConfig("Platform/VirtualFileSystem/Zip/Configuration");
#pragma warning restore 618
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
							Console.WriteLine(e.InnerException.Message);
						}

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

		[XmlElement]
		public virtual int AutoShadowThreshold { get; set; }


		internal virtual void SetXmlNode(XmlNode node)
		{
			this.XmlNode = node;
		}

		public ConfigurationSection()
		{
			this.AutoShadowThreshold = 0;
		}
	}
}
