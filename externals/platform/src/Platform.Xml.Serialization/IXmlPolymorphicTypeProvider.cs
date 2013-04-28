using System;
using System.Xml;

namespace Platform.Xml.Serialization
{
	public interface IXmlDynamicTypeProvider
	{
		Type GetType(object instance);
		Type GetType(XmlReader reader);
	}
}
