using System;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	public abstract class TypeSerializerFactory
	{
		public abstract TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, SerializationMemberInfo memberInfo, TypeSerializerCache cache);
		public abstract TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, TypeSerializerCache cache);
		public abstract TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, SerializationMemberInfo memberInfo, TypeSerializerCache cache);
		public abstract TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, TypeSerializerCache cache);
	}
}
