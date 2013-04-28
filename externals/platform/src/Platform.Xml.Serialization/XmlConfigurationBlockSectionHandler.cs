using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Platform.Xml.Serialization;

namespace Platform
{
	public class XmlConfigurationBlockSectionHandler<T>
		: IConfigurationSectionHandler
		where T : new()
	{
		public object Create(object parent, object configContext, System.Xml.XmlNode section)
		{
			var serializer = XmlSerializer<T>.New();
			var retval = serializer.Deserialize(new XmlNodeReader(section));

			return retval;
		}
	}
}
