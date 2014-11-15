using System;
using System.Xml;
using System.Configuration;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers.Zip
{
	[XmlElement("Configuration")]
	public class ConfigurationSection
	{
		public static ConfigurationSection Instance
		{
			get
			{
				if (instance != null)
				{
					return instance;
				}

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

				return instance;
			}
		}
		private static ConfigurationSection instance;

		public XmlNode XmlNode { get; private set; }

		[XmlElement]
		public int AutoShadowThreshold { get; set; }


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
