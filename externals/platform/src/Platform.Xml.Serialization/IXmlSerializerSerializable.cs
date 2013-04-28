using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Xml.Serialization
{
	public interface IXmlSerializerSerializable
	{
		XmlSerializer<object> GetXmlSerializer();
	}

	public interface IXmlSerializerSerializable<T>
		: IXmlSerializerSerializable
		where T : new()
	{
		new XmlSerializer<T> GetXmlSerializer();
	}
}
