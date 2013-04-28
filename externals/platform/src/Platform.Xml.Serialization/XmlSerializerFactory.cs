using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using Platform.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// A factory that creates <see cref="XmlSerializer{T}"/> objects.
	/// </summary>
	public abstract class XmlSerializerFactory
	{
		private static readonly XmlSerializerFactory factory = new CachingXmlSerializerFactory();

		/// <summary>
		/// Gets an instance of the default XmlSerializer.
		/// </summary>
		/// <returns></returns>
		public static XmlSerializerFactory Default
		{
			get
			{
				return factory;
			}
		}

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <typeparam name="T">The serializer's supported object type</typeparam>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<T> NewXmlSerializer<T>();

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <typeparam name="T">The serializer's supported object type</typeparam>
		/// <param name="options">The options for the serializer</param>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<T> NewXmlSerializer<T>(SerializerOptions options);

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <param name="type">The type supported by the serializer</param>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<object> NewXmlSerializer(Type type);

		/// <summary>
		/// Creates a new <see cref="XmlSerializer{T}"/>
		/// </summary>
		/// <param name="type">The type supported by the serializer</param>
		/// <param name="options">The options for the serializer</param>
		/// <returns>A new <see cref="XmlSerializer{T}"/></returns>
		public abstract XmlSerializer<object> NewXmlSerializer(Type type, SerializerOptions options);
	}

	/// <summary>
	/// An <see cref="XmlSerializerFactory"/> that supports caching of serializers
	/// </summary>
	public class CachingXmlSerializerFactory
		: XmlSerializerFactory
	{
		private ILDictionary<Pair<Type, SerializerOptions>, object> cache;
		private ILDictionary<Pair<Type, SerializerOptions>, object> cacheForDynamic;

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}()<>"/>
		/// </summary>
		public override XmlSerializer<T> NewXmlSerializer<T>()
		{
			return NewXmlSerializer<T>(null);
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer(Type)"/>
		/// </summary>
		public override XmlSerializer<object> NewXmlSerializer(Type type)
		{
			return NewXmlSerializer(type, null);
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer(Type, SerializerOptions)"/>
		/// </summary>
		public override XmlSerializer<object> NewXmlSerializer(Type type, SerializerOptions options)
		{
			object value;
			XmlSerializer<object> serializer;
			Pair<Type, SerializerOptions> key;

			key = new Pair<Type, SerializerOptions>(type, options);

			if (cacheForDynamic == null)
			{
				cacheForDynamic = new LinearHashDictionary<Pair<Type, SerializerOptions>, object>();
			}

			if (cacheForDynamic.TryGetValue(key, out value))
			{
				return (XmlSerializer<object>)value;
			}

			if (options == null)
			{
				serializer = new XmlSerializer<object>(type);
			}
			else
			{
				serializer = new XmlSerializer<object>(type, options);
			}

			cacheForDynamic[key] = serializer;

			return serializer;
		}

		/// <summary>
		/// <see cref="XmlSerializerFactory.NewXmlSerializer{T}(SerializerOptions)"/>
		/// </summary>
		public override XmlSerializer<T> NewXmlSerializer<T>(SerializerOptions options)
		{
			object value;
			XmlSerializer<T> serializer;
			Pair<Type, SerializerOptions> key;

			key = new Pair<Type, SerializerOptions>(typeof(T), options);

			if (cache == null)
			{
				 cache = new LinearHashDictionary<Pair<Type, SerializerOptions>, object>();
			}

			if (cache.TryGetValue(key, out value))
			{
				return (XmlSerializer<T>)value;
			}

			if (options == null)
			{
				serializer = new XmlSerializer<T>();
			}
			else
			{
				serializer = new XmlSerializer<T>(options);
			}

			cache[key] = serializer;

			return serializer;
		}
	}
}