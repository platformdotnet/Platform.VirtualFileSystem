using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public class XmlExcludeAttribute
		: XmlSerializationAttribute
	{
	}
}