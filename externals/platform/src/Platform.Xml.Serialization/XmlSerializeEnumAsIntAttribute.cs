using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Xml.Serialization
{
	public class XmlSerializeEnumAsIntAttribute
		: XmlAttributeAttribute
	{
		public bool Value
		{
			get;
			set;
		}

		public XmlSerializeEnumAsIntAttribute()
			: this(true)
		{
		}

		public XmlSerializeEnumAsIntAttribute(bool value)
		{
			this.Value = value;
		}
	}
}
