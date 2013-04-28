using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlVerifyRuntimeTypeAttribute
		: XmlSerializationAttribute
	{
		public virtual Type[] Types
		{
			get;
			set;
		}

		public XmlVerifyRuntimeTypeAttribute(params Type[] types)
			: this(LogicalOperation.All, types)
		{
		}

		public XmlVerifyRuntimeTypeAttribute(LogicalOperation logicalCheck, params Type[] types)
		{
			this.Types = types;
		}

		public virtual bool VerifiesAgainst(Type type)
		{
			foreach (var t in Types)
			{
				if (t.IsAssignableFrom(type))
				{
					return true;
				}
			}

			return false;
		}
	}
}