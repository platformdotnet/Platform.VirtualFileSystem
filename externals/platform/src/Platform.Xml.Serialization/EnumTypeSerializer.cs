using System;

namespace Platform.Xml.Serialization
{
	public class EnumTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		public override Type SupportedType
		{
			get
			{
				return supportedType;
			}
		}

		private readonly Type supportedType;

		public EnumTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{			
			supportedType = memberInfo.ReturnType;

			if (!typeof(Enum).IsAssignableFrom(supportedType))
			{
				throw new ArgumentException(this.GetType().Name + " only works with Enum types");
			}
		}

		public override string Serialize(object obj, SerializationContext state)
		{
			var attribute = (XmlSerializeEnumAsIntAttribute)state.GetCurrentMemberInfo().GetFirstApplicableAttribute(typeof(XmlSerializeEnumAsIntAttribute));

			if (attribute != null && attribute.Value)
			{
				return Convert.ToInt32(obj).ToString();
			}

			return Enum.Format(supportedType, obj, "G");
		}

		public override object Deserialize(string value, SerializationContext state)
		{
			return Enum.Parse(supportedType, value, true);
		}
	}
}
