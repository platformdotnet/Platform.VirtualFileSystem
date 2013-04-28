using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
	public class XmlSerializeBaseAttribute
		: XmlSerializationAttribute
	{
		public bool SerializeBase
		{
			get;

			set;
		}

		public XmlSerializeBaseAttribute()
		{
		}

		public XmlSerializeBaseAttribute(bool serializeBase)
		{
			SerializeBase = serializeBase;
		}
	}
}