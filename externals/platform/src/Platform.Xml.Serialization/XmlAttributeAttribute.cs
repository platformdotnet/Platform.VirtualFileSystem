using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlAttributeAttribute
		: XmlApproachAttribute
	{
		public XmlAttributeAttribute()
		{
		}

		public XmlAttributeAttribute(Type type)
			: base(type)
		{
		}

		public XmlAttributeAttribute(string name)
			: base(name)
		{			
		}

		public XmlAttributeAttribute(string name, Type type)
			: base(name, type)
		{
		}

		public XmlAttributeAttribute(string ns, string name, Type type)
			: base(ns, name, type)
		{
		}
	}
}