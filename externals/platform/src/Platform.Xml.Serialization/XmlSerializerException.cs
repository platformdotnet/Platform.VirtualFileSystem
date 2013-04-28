using System;

namespace Platform.Xml.Serialization
{
	public class XmlSerializerException
		: ApplicationException
	{
		public XmlSerializerException()
			: base()
		{
		}

		public XmlSerializerException(string message)
			: base(message)
		{			
		}
	}
}
