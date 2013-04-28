using System;

namespace Platform.Xml.Serialization
{
	public class XmlPolymorphicTypeAttribute
		: XmlSerializationAttribute
	{
		public Type PolymorphicTypeProvider
		{
			get;
			set;
		}

		public XmlPolymorphicTypeAttribute(Type polymorphicTypeProvider)
		{
			PolymorphicTypeProvider = polymorphicTypeProvider;
		}
	}
}