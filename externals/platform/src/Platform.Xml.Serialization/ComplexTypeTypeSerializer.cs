using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Serializes complex types (classes and structures).
	/// </summary>
	public class ComplexTypeTypeSerializer
		: TypeSerializer
	{
		#region Fields

		/// <summary>
		/// Dictiopnary of all the elements in this type.
		/// </summary>
		protected IDictionary elementMembersMap;

		/// <summary>
		/// Dictionary of all the attributes in this type.
		/// </summary>
		protected IDictionary attributeMembersMap;

		#endregion

		#region Properties

		/// <summary>
		/// <see cref="TypeSerializer.SupportedType"/>
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return supportedType;
			}
		}
		private readonly Type supportedType;

		public override bool MemberBound
		{
			get
			{
				return memberBound;
			}
		}
		private readonly bool memberBound = false;

		#endregion

		private readonly SerializationMemberInfo serializationMemberInfo;

		#region Constructors

		public ComplexTypeTypeSerializer(SerializationMemberInfo memberInfo, Type type, TypeSerializerCache cache, SerializerOptions options)
		{
			supportedType = type;

			serializationMemberInfo = memberInfo;

			elementMembersMap = new ListDictionary();
			attributeMembersMap = new ListDictionary();

			if (memberInfo != null && memberInfo.IncludeIfUnattributed)
			{
				memberBound = true;
			}

			cache.Add(this, memberInfo);

			Scan(cache, options);
		}

		#endregion

		#region Scan

		/// <summary>
		/// Scan the tyoe for properties and fields to serialize.
		/// </summary>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		protected virtual void Scan(TypeSerializerCache cache, SerializerOptions options)
		{
			var type = supportedType;

			while (type != typeof(object) && type != null)
			{
				var fields = supportedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				var properties = supportedType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

				foreach (FieldInfo field in fields)
				{
					AddMember(field, cache, options);
				}

				foreach (PropertyInfo property in properties)
				{
					AddMember(property, cache, options);
				}

				var serializeBase = true;
				var attribs = type.GetCustomAttributes(typeof(XmlSerializeBaseAttribute), false);

				foreach (XmlSerializeBaseAttribute attrib in attribs)
				{
					if (attrib.Applies(options))
					{
						serializeBase = attrib.SerializeBase;
					}
				}

				if (!serializeBase)
				{
					break;
				}

				type = type.BaseType;
			}
		}

		#endregion

		#region AddMember

		protected virtual void AddMember(MemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			SerializationMemberInfo localSerializationMemberInfo;

			if (this.MemberBound && this.serializationMemberInfo.IncludeIfUnattributed)
			{
				bool ok;
				var fieldInfo = memberInfo as FieldInfo;
				var propertyInfo = memberInfo as PropertyInfo;

				if (fieldInfo != null && fieldInfo.IsPublic)
				{
					ok = true;
				}
				else if (propertyInfo != null
					&& propertyInfo.CanRead && propertyInfo.CanWrite
					&& propertyInfo.GetSetMethod().IsPublic
					&& propertyInfo.GetGetMethod().IsPublic)
				{
					ok = true;
				}
				else
				{
					ok = false;
				}

				if (ok)
				{
					localSerializationMemberInfo = new SerializationMemberInfo(memberInfo, options, cache, true);

					elementMembersMap[localSerializationMemberInfo.SerializedName] = localSerializationMemberInfo;

					return;
				}
			}

			localSerializationMemberInfo = new SerializationMemberInfo(memberInfo, options, cache);

			if (localSerializationMemberInfo.SerializedNodeType == XmlNodeType.Element)
			{
				if (localSerializationMemberInfo.Namespace.Length > 0)
				{
					elementMembersMap[localSerializationMemberInfo.Namespace + (char)0xff + localSerializationMemberInfo.SerializedName] = localSerializationMemberInfo;
				}
				else
				{
					elementMembersMap[localSerializationMemberInfo.SerializedName] = localSerializationMemberInfo;
				}

				return;
			}
			else if (localSerializationMemberInfo.SerializedNodeType == XmlNodeType.Attribute)
			{
				if (localSerializationMemberInfo.Namespace.Length > 0)
				{
					attributeMembersMap[localSerializationMemberInfo.Namespace + (char)0xff + localSerializationMemberInfo.SerializedName] = localSerializationMemberInfo;
				}
				else
				{
					attributeMembersMap[localSerializationMemberInfo.SerializedName] = localSerializationMemberInfo;
				}
			}
		}

		#endregion

		protected virtual void SerializeAttributes(object obj, XmlWriter writer, SerializationContext state)
		{
			TypeSerializerWithSimpleTextSupport simpleSerializer;

			foreach (SerializationMemberInfo memberInfo in attributeMembersMap.Values)
			{
				object val;

				// Get the value of the field/property to be serialized from the object
				val = memberInfo.GetValue(obj);

				// If the valuetype field should be treated as null if it is empty then make val null.
				if (memberInfo.TreatAsNullIfEmpty)
				{
					if (memberInfo.ReturnType.IsValueType)
					{
						if (Activator.CreateInstance(memberInfo.ReturnType).Equals(val))
						{
							val = null;
						}
					}
				}

				// Make sure we aren't serializing recursively.
				if (state.ShouldSerialize(val))
				{
					try
					{
						state.PushCurrentMemberInfo(memberInfo);

						// Get the TypeSerializerWithSimpleTextSupport
						simpleSerializer = memberInfo.GetSerializer(obj) as TypeSerializerWithSimpleTextSupport;

						// Make sure the serializer supports SimpleText
						if (simpleSerializer == null)
						{
							throw new XmlSerializerException(String.Format(TextResources.NoTextSerializerWithSimpleTextSupport, memberInfo.MemberInfo.Name));
						}

						// Write the start of the attribute.
						writer.WriteStartAttribute(memberInfo.SerializedName, "");

						// Write the attribute value.
						writer.WriteString(simpleSerializer.Serialize(val, state));

						// Write the end of the attribute
						writer.WriteEndAttribute();
					}
					finally
					{
						state.PopCurrentMemberInfo();
					}
				}
			}
		}

		protected virtual void SerializeElements(object obj, XmlWriter writer, SerializationContext state)
		{
			TypeSerializer serializer;
			TypeSerializerWithSimpleTextSupport serializerWithSimpleText;

			foreach (SerializationMemberInfo memberInfo in elementMembersMap.Values)
			{
				state.PushCurrentMemberInfo(memberInfo);

				try
				{
					object val;

					val = memberInfo.GetValue(obj);

					if (memberInfo.TreatAsNullIfEmpty)
					{
						if (memberInfo.ReturnType.IsValueType)
						{
							if (Activator.CreateInstance(memberInfo.ReturnType).Equals(val))
							{
								val = null;
							}
						}
					}

					serializer = memberInfo.GetSerializer(val);
					serializerWithSimpleText = serializer as TypeSerializerWithSimpleTextSupport;

					if (state.ShouldSerialize(val))
					{
						if (memberInfo.Namespace.Length > 0)
						{
							// Write start element with namespace

							writer.WriteStartElement(state.Parameters.Namespaces.GetPrefix(memberInfo.Namespace), memberInfo.SerializedName, memberInfo.Namespace);
						}
						else
						{
							// Write start element without namespace

							writer.WriteStartElement(memberInfo.SerializedName, "");
						}
                        
						if (memberInfo.SerializeAsValueNodeAttributeName != null)
						{
							if (serializerWithSimpleText == null)
							{
								throw new XmlSerializerException(String.Format(TextResources.NoTextSerializerWithSimpleTextSupport, memberInfo.MemberInfo.Name));
							}

							writer.WriteAttributeString(memberInfo.SerializeAsValueNodeAttributeName, serializerWithSimpleText.Serialize(val, state));
						}
						else if (memberInfo.SerializeAsCData)
						{
							if (serializerWithSimpleText == null)
							{
								throw new XmlSerializerException(String.Format(TextResources.NoTextSerializerWithSimpleTextSupport, memberInfo.MemberInfo.Name));
							}

							writer.WriteCData(serializerWithSimpleText.Serialize(val, state));
						}
						else
						{
							memberInfo.GetSerializer(val).Serialize(val, writer, state);
						}

						writer.WriteEndElement();
					}
				}
				finally
				{
					state.PopCurrentMemberInfo();
				}
			}
		}

		protected virtual void DeserializeAttribute(object obj, XmlReader reader, SerializationContext state)
		{
			object value;
			SerializationMemberInfo serializationMember;

			if (reader.Prefix == "xmlns")
			{
				return;
			}

			if (reader.Prefix.Length > 0)
			{
				serializationMember = (SerializationMemberInfo)attributeMembersMap[state.Parameters.Namespaces.GetNamespace(reader.Prefix) + (char)0xff + reader.LocalName];
			}
			else
			{
				serializationMember = (SerializationMemberInfo)attributeMembersMap[reader.Name];
			}

			if (serializationMember == null)
			{
				if (obj is ISerializationUnhandledMarkupListener)
				{
					((ISerializationUnhandledMarkupListener)obj).UnhandledAttribute(reader.Name, reader.Value);
				}

				return;
			}

			state.PushCurrentMemberInfo(serializationMember);

			try
			{
				value = serializationMember.GetSerializer(reader).Deserialize(reader, state);
			}
			finally
			{
				state.PopCurrentMemberInfo();
			}

			serializationMember.SetValue(obj, value);
		}

		protected virtual bool CanDeserializeElement(object obj, XmlReader reader, SerializationContext state)
		{
			if (reader.Prefix.Length > 0)
			{
				return (SerializationMemberInfo)elementMembersMap[state.Parameters.Namespaces.GetNamespace(reader.Prefix) + (char)0xff + reader.LocalName] != null;
			}
			else
			{
				return (SerializationMemberInfo)elementMembersMap[reader.LocalName] != null;
			}
		}

		protected virtual void DeserializeElement(object obj, XmlReader reader, SerializationContext state)
		{
			SerializationMemberInfo serializationMember;

			if (reader.Prefix.Length > 0)
			{
				serializationMember = (SerializationMemberInfo)elementMembersMap[state.Parameters.Namespaces.GetNamespace(reader.Prefix) + (char)0xff + reader.LocalName];
			}
			else
			{
				serializationMember = (SerializationMemberInfo)elementMembersMap[reader.LocalName];
			}

			if (serializationMember == null)
			{
				XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
			}
			else
			{
				state.PushCurrentMemberInfo(serializationMember);

				try
				{
					if (serializationMember.SerializeAsValueNodeAttributeName != null
						&& serializationMember.GetSerializer(reader) is TypeSerializerWithSimpleTextSupport)
					{
						var s = reader.GetAttribute(serializationMember.SerializeAsValueNodeAttributeName);
						var serializer = serializationMember.GetSerializer(reader);

						serializationMember.SetValue(obj, ((TypeSerializerWithSimpleTextSupport)(serializer)).Deserialize(s, state));

						XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
					}
					else
					{
						serializationMember.SetValue(obj, serializationMember.GetSerializer(reader).Deserialize(reader, state));
					}
				}
				finally
				{
					state.PopCurrentMemberInfo();
				}
			}
		}

		public override void Serialize(object obj, XmlWriter writer, SerializationContext state)
		{
			try
			{
				// Start of serialization
				state.SerializationStart(obj);

				// Serialize attributes
				SerializeAttributes(obj, writer, state);

				// Serialize elements.
				SerializeElements(obj, writer, state);
			}
			finally
			{
				// End of serialization
				state.SerializationEnd(obj);
			}
		}

		protected virtual object CreateInstance(XmlReader reader, SerializationContext state)
		{
			return Activator.CreateInstance(supportedType);
		}

		public override object Deserialize(XmlReader reader, SerializationContext state)
		{
			return Deserialize(CreateInstance(reader, state), reader, state);
		}

		public virtual object Deserialize(object instance, XmlReader reader, SerializationContext state)
		{
			var obj = instance;

			state.DeserializationStart(obj);

			if (reader.AttributeCount > 0)
			{
				for (int i = 0; i < reader.AttributeCount; i++)
				{
					reader.MoveToAttribute(i);

					DeserializeAttribute(obj, reader, state);
				}

				reader.MoveToElement();
			}

			// If there's no subelements then exit

			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();

				return obj;
			}

			reader.ReadStartElement();

			// Read elements

			while (true)
			{
				XmlReaderHelper.ReadUntilAnyTypesReached(reader,
					new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement });

				if (reader.NodeType == XmlNodeType.Element)
				{
					DeserializeElement(obj, reader, state);

					//XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
				}
				else
				{
					if (reader.NodeType == XmlNodeType.EndElement)
					{
						reader.ReadEndElement();
					}
					else
					{
						// Unknown element
					}

					break;
				}
			}

			state.DeserializationEnd(obj);

			return obj;
		}
	}
}
