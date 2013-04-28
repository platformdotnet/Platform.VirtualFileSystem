using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// A <see cref="TypeSerializer"/> that supports serializing
	/// various primitive types to and from strings.
	/// </summary>
	public class StringableTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		/// <summary>
		/// A serializer that supports bools.
		/// </summary>
		public static readonly StringableTypeSerializer BoolSerializer;

		/// <summary>
		/// A serializer that supports bytes.
		/// </summary>
		public static readonly StringableTypeSerializer ByteSerializer;

		/// <summary>
		/// A serializer that supports sbytes.
		/// </summary>
		public static readonly StringableTypeSerializer SByteSerializer;

		/// <summary>
		/// A serializer that supports chars.
		/// </summary>
		public static readonly StringableTypeSerializer CharSerializer;

		/// <summary>
		/// A serializer that supports decimals.
		/// </summary>
		public static readonly StringableTypeSerializer DecimalSerializer;

		/// <summary>
		/// A serializer that supports doubles.
		/// </summary>
		public static readonly StringableTypeSerializer DoubleSerializer;

		/// <summary>
		/// A serializer that supports floats.
		/// </summary>
		public static readonly StringableTypeSerializer FloatSerializer;

		/// <summary>
		/// A serializer that supports ints.
		/// </summary>
		public static readonly StringableTypeSerializer IntSerializer;

		/// <summary>
		/// A serializer that supports uints.
		/// </summary>
		public static readonly StringableTypeSerializer UIntSerializer;

		/// <summary>
		/// A serializer that supports longs.
		/// </summary>
		public static readonly StringableTypeSerializer LongSerializer;

		/// <summary>
		/// A serializer that supports ulongs.
		/// </summary>
		public static readonly StringableTypeSerializer ULongSerializer;

		/// <summary>
		/// A serializer that supports shorts.
		/// </summary>
		public static readonly StringableTypeSerializer ShortSerializer;

		/// <summary>
		/// A serializer that supports ushorts.
		/// </summary>
		public static readonly StringableTypeSerializer UShortSerializer;

		/// <summary>
		/// A serializer that supports strings.
		/// </summary>
		public static readonly StringableTypeSerializer StringSerializer;

		/// <summary>
		/// A serializer that supports guids.
		/// </summary>
		public static readonly StringableTypeSerializer GuidSerializer;

		/// <summary>
		/// A serializer that supports colors.
		/// </summary>
		public static readonly StringableTypeSerializer ColorSerializer;
 
		static StringableTypeSerializer()
		{
			BoolSerializer = new StringableTypeSerializer(typeof(bool));
			ByteSerializer = new StringableTypeSerializer(typeof(byte));
			SByteSerializer = new StringableTypeSerializer(typeof(sbyte));
			CharSerializer = new StringableTypeSerializer(typeof(char));
			DecimalSerializer = new StringableTypeSerializer(typeof(decimal));
			DoubleSerializer = new StringableTypeSerializer(typeof(double));
			FloatSerializer = new StringableTypeSerializer(typeof(float));
			IntSerializer = new StringableTypeSerializer(typeof(int));
			UIntSerializer = new StringableTypeSerializer(typeof(uint));
			LongSerializer = new StringableTypeSerializer(typeof(long));
			ULongSerializer = new StringableTypeSerializer(typeof(ulong));
			ShortSerializer = new StringableTypeSerializer(typeof(short));
			UShortSerializer = new StringableTypeSerializer(typeof(ushort));
			StringSerializer = new StringableTypeSerializer(typeof(string));
			GuidSerializer = new GuidSerializerImpl(typeof(Guid));
			ColorSerializer = new ColorSerializerImpl(typeof(Color));
		}
		
		public override Type SupportedType
		{
			get
			{
				return supportedType;
			}
		}
		private readonly Type supportedType;

		public StringableTypeSerializer(Type type)
		{
			supportedType = type;
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Serialize(object, SerializationContext)"/>
		/// </summary>
		public override string Serialize(object obj, SerializationContext state)
		{
			return obj.ToString();
		}

		/// <summary>
		/// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
		/// </summary>
		public override object Deserialize(string value, SerializationContext state)
		{
			return Convert.ChangeType(value, supportedType);
		}

		private class GuidSerializerImpl
			: StringableTypeSerializer
		{
			public GuidSerializerImpl(Type type)
				: base(type)
			{
			}

			/// <summary>
			/// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
			/// </summary>
			public override object Deserialize(string value, SerializationContext state)
			{
				return new Guid(value);
			}
		}

		private class ColorSerializerImpl
			: StringableTypeSerializer
		{
			public ColorSerializerImpl(Type type)
				: base(type)
			{
			}

			public override string Serialize(object obj, SerializationContext state)
			{
				if (obj == null)
				{
					return "";
				}
				
				if (((Color)obj).ToKnownColor() != 0)
				{
					return ((Color)obj).Name;
				}
				else
				{
					return ColorTranslator.ToHtml((Color)obj);
				}
			}

			/// <summary>
			/// <see cref="TypeSerializerWithSimpleTextSupport.Deserialize(string, SerializationContext)"/>
			/// </summary>
			public override object Deserialize(string value, SerializationContext state)
			{
				try
				{
					return ColorTranslator.FromHtml(value);
				}
				catch (Exception)
				{
					return Color.FromName(value);
				}
			}
		}
	}
}
