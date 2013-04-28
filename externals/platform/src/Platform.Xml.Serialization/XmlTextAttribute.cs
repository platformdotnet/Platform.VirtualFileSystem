using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlTextAttribute
		: XmlSerializationAttribute
	{
	}
}