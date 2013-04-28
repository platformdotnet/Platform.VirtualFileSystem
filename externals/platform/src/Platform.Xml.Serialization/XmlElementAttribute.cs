using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlElementAttribute
		: XmlApproachAttribute
	{
		public XmlElementAttribute()
		{
			ValueNodeAttributeName = "value";
		}

		public XmlElementAttribute(Type type)
			: base(type)
		{
			ValueNodeAttributeName = "value";
		}

		public XmlElementAttribute(string name)
			: base(name)
		{
			ValueNodeAttributeName = "value";
		}

		public XmlElementAttribute(string name, Type type)
			: base(name, type)
		{
			ValueNodeAttributeName = "value";
		}

		public XmlElementAttribute(string ns, string name, Type type)
			: base(ns, name, type)
		{
			ValueNodeAttributeName = "value";
		}

		/// <summary>
		/// Set to true if you would like node to be serialized as a node with the data stored
		/// in an attribute named <c>ValueAttributeName</c> rather than inside the element itself.
		/// </summary>
		public bool SerializeAsValueNode
		{
			get;
			set;
		}

		public string ValueNodeAttributeName
		{
			get;
			set;
		}
	}
}