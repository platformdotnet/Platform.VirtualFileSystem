using System;
using System.IO;
using System.Xml;
using System.Text;

namespace Platform.Xml.Serialization
{	
	/// <summary>
	/// An advanced customizable and extensible XmlSerializer.  Use the <see cref="New()"/> method
	/// and associated overloads to construct XmlSerializers with the default <see cref="XmlSerializerFactory"/>.
	/// </summary>
	/// <typeparam name="T">
	/// The type of object the serializer supports.  Sometimes type <c>T</c> may be of a more generic type
	/// than the type that the serializer actually supports.
	/// </typeparam>
	public class XmlSerializer<T>
	{
		private readonly SerializationMemberInfo rootMemberInfo;

		/// <summary>
		/// The options for the XmlSeiralizer.
		/// </summary>
		public virtual SerializerOptions Options
		{
			get;
			private set;
		}

		/// <summary>
		/// Constructs a new XmlSerializer supporting type <see cref="T"/>
		/// </summary>
		internal XmlSerializer()
			: this(typeof(T))
		{
		}

		/// <summary>
		/// Constructs a new XmlSerializer supporting the type <see cref="type"/>
		/// </summary>
		/// <param name="type">The type supported by the serializer</param>
		internal XmlSerializer(Type type)
			: this(type, SerializerOptions.Empty)
		{
		}

		/// <summary>
		/// Constructs a new XmlSerializer supporting type <see cref="T"/> with the provided options
		/// </summary>
		/// <param name="options">The options</param>
		internal XmlSerializer(SerializerOptions options)
			: this(typeof(T), options)
		{
		}

		/// <summary>
		/// Constructs a new XmlSerializer supporting type <see cref="type"/> with the provided options
		/// </summary>
		/// <param name="options">The options</param>
		internal XmlSerializer(Type type, SerializerOptions options)
		{
			TypeSerializerCache cache;
			TypeSerializerFactory factory;		

			this.Options = options;
			
			factory = new StandardTypeSerializerFactory(options);
			
			cache = new TypeSerializerCache(factory);

			rootMemberInfo = new SerializationMemberInfo(type, options, cache);

			if (!rootMemberInfo.Serializable)
			{
				throw new XmlSerializerException("Unable to serialize given type, type must have [XmlElement] attribute.");
			}
		}
	
		/// <summary>
		/// Serializers the provided object using the provided <see cref="writer"/> and <see cref="parameters"/>.
		/// </summary>
		/// <param name="obj">The object to serializer</param>
		/// <param name="writer">The <see cref="XmlWriter"/> to serialize to</param>
		/// <param name="parameters">The parameters used for serialization<m/param>
		public virtual void Serialize(T obj, XmlWriter writer, SerializationParameters parameters)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			
			var state = new SerializationContext(this.Options, parameters);

			if (!parameters.ContainsByName("ExcludeXmlDecl"))
			{
				writer.WriteStartDocument();
			}
						
			if (rootMemberInfo.Namespace.Length > 0)
			{
				writer.WriteStartElement(parameters.Namespaces.GetPrefix(rootMemberInfo.Namespace), rootMemberInfo.SerializedName, rootMemberInfo.Namespace);
			}
			else
			{
				writer.WriteStartElement(rootMemberInfo.SerializedName);
			}

			foreach (string prefix in parameters.Namespaces.GetPrefixes())
			{
				writer.WriteAttributeString("xmlns", prefix, null, parameters.Namespaces.GetNamespace(prefix));
			}
			
			rootMemberInfo.GetSerializer(obj).Serialize(obj, writer, state);

			writer.WriteEndElement();
		}
		
		/// <summary>
		/// Deserializers an object from the given <see cref="reader"/>
		/// </summary>
		/// <param name="parameters">The paramters for deserialization</param>
		/// <param name="reader">The reader<see cref="XmlReader"/> to deserialize from</param>
		/// <returns>The deserialized object</returns>		
		public virtual T Deserialize(XmlReader reader, SerializationParameters parameters)
		{
			var state = new SerializationContext(this.Options, parameters);

			reader.Read();

			XmlReaderHelper.ReadUntilTypeReached(reader, XmlNodeType.Element);

			if (reader.NodeType != XmlNodeType.Element)
			{
				throw new XmlSerializerException("Can't find root XML node.");
			}

			var reval = (T)rootMemberInfo.GetSerializer(reader).Deserialize(reader, state);

			return reval;
		}

		/// <summary>
		/// Deserializers an object from the given <see cref="reader"/>
		/// </summary>
		/// <param name="instance">An existing instance to populate</param>
		/// <param name="parameters">The paramters for deserialization</param>
		/// <param name="reader">The reader<see cref="XmlReader"/> to deserialize from</param>
		/// <returns>The deserialized object</returns>		
		public virtual T Deserialize(T instance, XmlReader reader, SerializationParameters parameters)
		{
			var state = new SerializationContext(this.Options, parameters);

			reader.Read();

			XmlReaderHelper.ReadUntilTypeReached(reader, XmlNodeType.Element);

			if (reader.NodeType != XmlNodeType.Element)
			{
				throw new XmlSerializerException("Can't find root XML node.");
			}

			var reval = (T)((ComplexTypeTypeSerializer)rootMemberInfo.GetSerializer(reader)).Deserialize(instance, reader, state);

			return reval;
		}

		/// <summary>
		/// Serializes the given object to the given <see cref="XmlWriter"/>
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="writer">The <see cref="XmlWriter"/> to serialize to</param>		
		public virtual void Serialize(T obj, XmlWriter writer)
		{
			Serialize(obj, writer, SerializationParameters.Empty);
		}
		
		/// <summary>
		/// Serializes the given object to the given <see cref="TextWriter"/>
		/// </summary>
		/// <param name="obj">The object to serialize</param>
		/// <param name="writer">The <see cref="TextWriter"/> to serialize to</param>		
		public virtual void Serialize(T obj, TextWriter writer)
		{
			Serialize(obj, writer, SerializationParameters.Empty);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="writer"></param>
		/// <param name="parameters"></param>		
		public virtual void Serialize(T obj, TextWriter writer, SerializationParameters parameters)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}

			var textWriter = new XmlTextWriter(writer);

			textWriter.Formatting = Formatting.Indented;

			Serialize(obj, textWriter, parameters);
		}

		public virtual string SerializeToString(T obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}

			var buffer = new StringBuilder(0x100);

			Serialize(obj, new StringWriter(buffer));

			return buffer.ToString();
		}

		/// <summary>
		/// Deserializes an object from the given <see cref="XmlReader"/>
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> to deserialize from</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(XmlReader reader)
		{
			return Deserialize(reader, SerializationParameters.Empty);
		}

		/// <summary>
		/// Deserializes an object from the given <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to deserialize from</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(TextReader reader)
		{
			return Deserialize(new XmlTextReader(reader), SerializationParameters.Empty);
		}

		/// <summary>
		/// Deserializes an object from the given <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to deserialize from</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(T instance, TextReader reader)
		{
			return Deserialize(instance, new XmlTextReader(reader), SerializationParameters.Empty);
		}

		/// <summary>
		/// Deserializes an object from the given <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to deserialize from</param>
		/// <param name="parameters">The <see cref="SerializationParameters"/> for deserialization</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(TextReader reader, SerializationParameters parameters)
		{
			return Deserialize(new XmlTextReader(reader), parameters);
		}

		/// <summary>
		/// Deserializes an object from the given <see cref="TextReader"/>
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> to deserialize from</param>
		/// <param name="parameters">The <see cref="SerializationParameters"/> for deserialization</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(T instance, TextReader reader, SerializationParameters parameters)
		{
			return Deserialize(instance, new XmlTextReader(reader), parameters);
		}

		/// <summary>
		/// Deserializes an object from the given xml string.
		/// </summary>
		/// <param name="xml">The xml string to deserialize from</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(string xml)
		{
			return Deserialize(new StringReader(xml));
		}

		/// <summary>
		/// Deserializes an object from the given xml string.
		/// </summary>
		/// <param name="xml">The xml string to deserialize from</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(T instance, string xml)
		{
			return Deserialize(instance, new StringReader(xml));
		}

		/// <summary>
		/// Deserializes an object from the given xml string.
		/// </summary>
		/// <param name="xml">The xml string to deserialize from</param>
		/// <param name="parameters">The <see cref="SerializationParameters"/> for deserialization</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(string xml, SerializationParameters parameters)
		{
			return Deserialize(new StringReader(xml), parameters);
		}

		/// <summary>
		/// Deserializes an object from the given xml string.
		/// </summary>
		/// <param name="xml">The xml string to deserialize from</param>
		/// <param name="parameters">The <see cref="SerializationParameters"/> for deserialization</param>
		/// <returns>The deserialized object</returns>
		public virtual T Deserialize(T instance, string xml, SerializationParameters parameters)
		{
			return Deserialize(instance, new StringReader(xml), parameters);
		}

		/// <summary>
		/// Creates an XmlSerializer for the supplied type <see cref="T"/> using the default <see cref="XmlSerializerFactory"/>.
		/// </summary>
		public static XmlSerializer<T> New()
		{
			return XmlSerializerFactory.Default.NewXmlSerializer<T>();
		}

		/// <summary>
		/// Creates an XmlSerializer for the supplied type <see cref="T"/> using the default <see cref="XmlSerializerFactory"/>.
		/// </summary>
		public static XmlSerializer<T> New(SerializerOptions options)
		{
			return XmlSerializerFactory.Default.NewXmlSerializer<T>(options);
		}

		/// <summary>
		/// Creates an XmlSerializer for the supplied type <see cref="type"/> using the default <see cref="XmlSerializerFactory"/>.
		/// </summary>
		public static XmlSerializer<object> New(Type type)
		{
			return XmlSerializerFactory.Default.NewXmlSerializer(type);
		}

		/// <summary>
		/// Creates an XmlSerializer for the supplied type <see cref="type"/> using the default <see cref="XmlSerializerFactory"/>
		/// and given <see cref="SerializerOptions"/>
		/// </summary>
		public static XmlSerializer<object> New(Type type, SerializerOptions options)
		{
			return XmlSerializerFactory.Default.NewXmlSerializer(type, options);
		}

		/// <summary>
		/// Changes the current serializer to a different serializer type.
		/// </summary>
		/// <typeparam name="U">The type for the new serializer</typeparam>
		/// <returns>
		/// The xml <see cref="XmlSerializer{T}"/> dynamically typed to serialize objects of type <see cref="U"/>
		/// </returns>
		public virtual XmlSerializer<U> ChangeType<U>()
		{
			return new DynamicallyTypedXmlSerializer<U>(this);
		}

		#region DynamicallyTypedXmlSerializer

		/// <summary>
		/// Used to support the <see cref="XmlSerializer{T}.ChangeType{U}<>"/> method
		/// </summary>
		/// <typeparam name="U"></typeparam>
		private class DynamicallyTypedXmlSerializer<U>
			: XmlSerializer<U>
		{
			private readonly XmlSerializer<T> serializer;

			public override SerializerOptions Options
			{
				get
				{
					return serializer.Options;
				}
			}

			public DynamicallyTypedXmlSerializer(XmlSerializer<T> serializer)
			{
				this.serializer = serializer;
			}

			public override void Serialize(U obj, XmlWriter writer, SerializationParameters parameters)
			{				
				serializer.Serialize((T)(object)obj, writer, parameters);
			}

			public override U Deserialize(XmlReader reader, SerializationParameters parameters)
			{
				return (U)(object)serializer.Deserialize(reader, parameters);
			}

			public override void Serialize(U obj, XmlWriter writer)
			{
				serializer.Serialize((T)(object)obj, writer);
			}

			public override void Serialize(U obj, TextWriter writer)
			{
				serializer.Serialize((T)(object)obj, writer);
			}

			public override void Serialize(U obj, TextWriter writer, SerializationParameters parameters)
			{
				serializer.Serialize((T)(object)obj, writer, parameters);
			}

			public override string SerializeToString(U obj)
			{
				return serializer.SerializeToString((T)(object)obj);
			}

			public override U Deserialize(XmlReader reader)
			{
				return (U)(object)serializer.Deserialize(reader);
			}

			public override U Deserialize(TextReader reader)
			{
				return (U)(object)serializer.Deserialize(reader);
			}

			public override U Deserialize(TextReader reader, SerializationParameters parameters)
			{
				return (U)(object)serializer.Deserialize(reader, parameters);
			}

			public override U Deserialize(string xml, SerializationParameters parameters)
			{
				return (U)(object)serializer.Deserialize(xml, parameters);
			}

			public override U Deserialize(string xml)
			{
				return (U)(object)serializer.Deserialize(xml);
			}
		}

		#endregion
	}
}
