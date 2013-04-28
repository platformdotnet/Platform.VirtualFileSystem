using System;
using System.Xml;
using System.Configuration;

namespace Platform.Xml.Serialization
{
	public abstract class AbstractXmlConfigurationSectionHandler<T>
		: IConfigurationSectionHandler
		where T : new()
	{
		private readonly XmlSerializer<T> serializer;

		protected AbstractXmlConfigurationSectionHandler(Type type)
			: this(XmlSerializer<T>.New())
		{			
		}

		protected AbstractXmlConfigurationSectionHandler(XmlSerializer<T> serializer)
		{
			this.serializer = serializer;
		}

		#region IConfigurationSectionHandler Members

		public virtual object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			return serializer.Deserialize(new XmlNodeReader(section));
		}

		#endregion
	}
}
