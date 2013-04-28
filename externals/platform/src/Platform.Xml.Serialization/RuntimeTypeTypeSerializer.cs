using System;
using Platform;

namespace Platform.Xml.Serialization
{
	public class RuntimeTypeTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		private readonly XmlSerializationAttribute[] verifyAttributes;

		public RuntimeTypeTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			verifyAttributes = (XmlSerializationAttribute[])memberInfo.GetApplicableAttributes(typeof(XmlVerifyRuntimeTypeAttribute));
		}

		public override Type SupportedType
		{
			get
			{
				return typeof(Type);
			}
		}

		public override string Serialize(object obj, SerializationContext state)
		{
			bool ok = verifyAttributes.Length == 0;

			foreach (XmlVerifyRuntimeTypeAttribute attribute in verifyAttributes)
			{
				if (attribute.VerifiesAgainst(obj.GetType()))
				{
					ok = true;
					break;
				}
			}

			if (!ok)
			{
				throw new XmlSerializerException("Invalid type");
			}

			return ((Type)obj).AssemblyQualifiedName;
		}

		public override object Deserialize(string value, SerializationContext state)
		{
			bool ok = verifyAttributes.Length == 0;

			var type = Type.GetType(value);

			if (type == null)
			{
				throw new TypeLoadException("The type could not be loaded: " + value);
			}

			foreach (XmlVerifyRuntimeTypeAttribute attribute in verifyAttributes)
			{
				if (attribute.VerifiesAgainst(type))
				{
					ok = true;
					break;
				}
			}

			if (!ok)
			{
				throw new XmlSerializerException("Invalid type");
			}

			return type;
		}
	}
}
