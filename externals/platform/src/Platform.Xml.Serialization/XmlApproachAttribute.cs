using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public abstract class XmlApproachAttribute
		: XmlSerializationAttribute
	{
		public virtual string Name
		{
			get;
			set;
		}

		public virtual bool SerializeUnattribted
		{
			get;
			set;
		}

		public virtual bool MakeNameLowercase
		{
			get;
			set;
		}

		public virtual bool UseNameFromAttributedType
		{
			get;
			set;
		}

		public virtual string Namespace
		{
			get;
			set;
		}

		public virtual Type Type
		{
			get;
			set;
		}

		public virtual Type SerializerType
		{
			get;
			set;
		}

		protected XmlApproachAttribute()
		{
			Namespace = "";
			MakeNameLowercase = false;
			Name = "";
		}

		protected XmlApproachAttribute(Type type)
			: this("", type)
		{
		}

		protected XmlApproachAttribute(string name)
		{
			Namespace = "";
			MakeNameLowercase = false;
			Name = name;
		}

		protected XmlApproachAttribute(string name, Type type)
		{
			Namespace = "";
			MakeNameLowercase = false;
			Type = type;
			Name = name;
		}

		protected XmlApproachAttribute(string ns, string name, Type type)
		{
			MakeNameLowercase = false;
			Type = type;
			Name = name;
			Namespace = ns;
		}
	}
}