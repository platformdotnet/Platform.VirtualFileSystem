using System;

namespace Platform.Xml.Serialization
{
	public class XmlDictionaryElementTypeAttribute
		: XmlElementAttribute
	{
		public virtual Type ElementType
		{
			get;
			set;
		}

		public virtual string TypeAlias
		{
			get;
			set;
		}

		public XmlDictionaryElementTypeAttribute(Type elementType, string typeAlias)
		{
			this.ElementType = elementType;
			this.TypeAlias = typeAlias;
		}
	}
}