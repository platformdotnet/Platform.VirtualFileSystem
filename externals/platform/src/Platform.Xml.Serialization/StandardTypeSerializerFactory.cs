using System;
using System.Drawing;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	public class StandardTypeSerializerFactory
		: TypeSerializerFactory
	{
		private readonly SerializerOptions options;

		public StandardTypeSerializerFactory(SerializerOptions options)
		{
			this.options = options;
		}

		public override TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, TypeSerializerCache cache)
		{
			return NewTypeSerializerBySupportedType(supportedType, null, cache);
		}

		public override TypeSerializer NewTypeSerializerBySupportedType(Type supportedType, SerializationMemberInfo memberInfo, TypeSerializerCache cache)
		{
			const string error = "A TypeSerializer can't be created for the given type without a memberInfo";

			if (typeof(Enum).IsAssignableFrom(supportedType))
			{
				if (memberInfo == null)
				{
					throw new XmlSerializerException(error);
				}

				return new EnumTypeSerializer(memberInfo, cache, options);
			}
			else if (typeof(IDictionary).IsAssignableFrom(supportedType))
			{
				if (memberInfo == null)
				{
					throw new ArgumentNullException("memberInfo");
				}

				return new DictionaryTypeSerializer(memberInfo, cache, options);
			}
			else if (typeof(Type).IsAssignableFrom(supportedType))
			{
				return new RuntimeTypeTypeSerializer(memberInfo, cache, options);
			}
			else if (supportedType == typeof(XmlNode))
			{
				return XmlNodeNodeTypeSerializer.Default;
			}
			else if (supportedType == typeof(bool))
			{
				return StringableTypeSerializer.BoolSerializer;
			}
			else if (supportedType == typeof(byte))
			{
				return StringableTypeSerializer.ByteSerializer;
			}
			else if (supportedType == typeof(char))
			{
				return StringableTypeSerializer.CharSerializer;
			}
			else if (supportedType == typeof(decimal))
			{
				return StringableTypeSerializer.DecimalSerializer;
			}
			else if (supportedType == typeof(double))
			{
				return StringableTypeSerializer.DoubleSerializer;
			}
			else if (supportedType == typeof(float))
			{
				return StringableTypeSerializer.FloatSerializer;
			}
			else if (supportedType == typeof(int))
			{
				return StringableTypeSerializer.IntSerializer;
			}
			else if (supportedType == typeof(long))
			{
				return StringableTypeSerializer.LongSerializer;
			}
			else if (supportedType == typeof(sbyte))
			{
				return StringableTypeSerializer.SByteSerializer;
			}
			else if (supportedType == typeof(short))
			{
				return StringableTypeSerializer.ShortSerializer;
			}
			else if (supportedType == typeof(string))
			{
				return StringableTypeSerializer.StringSerializer;
			}
			else if (supportedType == typeof(uint))
			{
				return StringableTypeSerializer.UIntSerializer;
			}
			else if (supportedType == typeof(ulong))
			{
				return StringableTypeSerializer.ULongSerializer;
			}
			else if (supportedType == typeof(ushort))
			{
				return StringableTypeSerializer.UShortSerializer;
			}
			else if (supportedType == typeof(Guid))
			{
				return StringableTypeSerializer.GuidSerializer;
			}
			else if (supportedType == typeof(Color))
			{
				return StringableTypeSerializer.ColorSerializer;
			}
			else if (supportedType == typeof(DateTime))
			{
				if (memberInfo == null)
				{
					throw new ArgumentNullException("memberInfo");
				}

				return new DateTimeTypeSerializer(memberInfo, cache, options);
			}
			else if (supportedType.IsGenericType
				&& supportedType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				return NewTypeSerializerBySupportedType
				(
					Nullable.GetUnderlyingType(supportedType),
					memberInfo,
					cache
				);
			}
			else
			{
				bool implementsList = false;
				bool implementsGenericList = false;

				implementsList = typeof(IList).IsAssignableFrom(supportedType);
				
				implementsGenericList = supportedType.FindInterfaces
				(
					delegate(Type type, object criterea)
					{
						return type.IsGenericType
							&& type.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IList<>);
					},
					null
				).Length > 0;

				if (implementsList || implementsGenericList)
				{
					if (memberInfo == null)
					{
						throw new XmlSerializerException(error);
					}

					return new ListTypeSerializer(memberInfo, cache, options);
				}

				if (supportedType.Name.Contains("list"))
				{
					Console.WriteLine("OOPS: " + supportedType);
					Console.WriteLine(implementsList);
					Console.WriteLine(implementsGenericList);
				}

				return new ComplexTypeTypeSerializer(memberInfo, supportedType, cache, options);
			}			
		}

		public override TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, TypeSerializerCache cache)
		{
			return NewTypeSerializerBySerializerType(serializerType, null, cache);
		}

		public override TypeSerializer NewTypeSerializerBySerializerType(Type serializerType, SerializationMemberInfo memberInfo, TypeSerializerCache cache)
		{
			TypeSerializer retval = null;			

			if (retval == null)
			{
				try
				{
					retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { cache, options });
				}
				catch (Exception)
				{
				}
			}

			if (retval == null && memberInfo != null)
			{
				try
				{
					retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { memberInfo, cache, options });
				}
				catch (Exception)
				{
				}
			}

			if (retval == null && memberInfo != null)
			{
				try
				{
					retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[] { memberInfo.ReturnType });
				}
				catch (Exception)
				{
				}
			}

			if (retval == null)
			{
				try
				{
					retval = (TypeSerializer)Activator.CreateInstance(serializerType, new object[0]);
				}
				catch (Exception)
				{
				}
			}
			
			if (retval == null)
			{
				throw new XmlSerializerException("Unable to create TypeSerializer: " + serializerType.GetType().ToString());
			}

			return retval;
		}
	}
}
