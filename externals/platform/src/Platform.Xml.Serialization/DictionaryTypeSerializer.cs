using System;
using System.Collections;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Type serializer that supports serialization of objects supporting IDictionary.
	/// </summary>
	/// <example>
	/// <code>
	/// public class Pig
	/// {
	///   [XmlElement]
	///   public string Name = "piglet"
	/// }
	/// 
	/// public class Cow
	/// {
	///   [XmlAttribute]
	///   public string Name = "daisy"
	/// }
	/// 
	/// public class Farm
	/// {
	///   [DictionaryElementType(typeof(Pig), "pig")]
	///   [DictionaryElementType(typeof(Cow), "cow")]	
	///   public IDictionary Animals;
	///   
	///   public Farm()
	///   {
	///     IDictionary dictionary = new Hashtable();
	///     dictionary["Piglet"] = new Pig();
	///     dictionary["Daisy"] = new Cow();
	///   }
	/// }
	/// </code>
	/// 
	/// Serialized output of Farm:
	/// 
	/// <code>
	/// <Farm>
	///   <Animals>
	///      <Piglet typealias="pig"><name>piglet</name></Piglet>
	///      <Daisy typealias="cow" name="daisy"></Daisy>
	///   </Animals>
	/// </Farm>
	/// </code>
	/// If only one DictionaryElementAttribute is present, the dictionary is assumed to only
	/// contain the type specified by that attribute.  The typealias is therefore not needed
	/// and will be omitted from the serialized output.  If a serializer with multiple type
	/// aliases deserializes an element without a typealias then the first type as defined by
	/// the first DictionaryElementAttribute is used.
	/// </example>
	public class DictionaryTypeSerializer
		: ComplexTypeTypeSerializer
	{
		/// <summary>
		/// Returns true.
		/// </summary>
		public override bool MemberBound
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns <c>typeof(IDictionary)</c>.
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return typeof(System.Collections.IDictionary);
			}
		}

		private readonly IDictionary<Type, DictionaryItem> typeToItemMap;
		private readonly IDictionary<string, DictionaryItem> aliasToItemMap;

		private class DictionaryItem
		{
			public string TypeAlias;
			public XmlDictionaryElementTypeAttribute Attribute;
			public TypeSerializer Serializer;			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		public DictionaryTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
			: base(memberInfo, memberInfo.ReturnType, cache, options)
		{
			typeToItemMap = new Dictionary<Type, DictionaryItem>();
			aliasToItemMap = new Dictionary<string, DictionaryItem>();

			Scan(memberInfo, cache, options);
		}

		private void Scan(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			IList<Attribute> attributes;
			SerializationMemberInfo smi;
			XmlSerializationAttribute[] attribs;

			attributes = new List<Attribute>();

			// Get the ElementType attributes specified on the type itself as long
			// as we're not the type itself!

			if (memberInfo.MemberInfo != memberInfo.ReturnType)
			{
				smi = new SerializationMemberInfo(memberInfo.ReturnType, options, cache);

				attribs = smi.GetApplicableAttributes(typeof(XmlDictionaryElementTypeAttribute));

				foreach (XmlSerializationAttribute a in attribs)
				{
					attributes.Add(a);
				}
			}

			// Get the ElementType attributes specified on the member.

			attribs = memberInfo.GetApplicableAttributes(typeof(XmlDictionaryElementTypeAttribute));

			foreach (var a in attribs)
			{
				attributes.Add(a);
			}

			foreach (XmlDictionaryElementTypeAttribute attribute in attributes)
			{
				SerializationMemberInfo smi2;
				var dictionaryItem = new DictionaryItem();
								
				smi2 = new SerializationMemberInfo(attribute.ElementType, options, cache);
				
				if (attribute.TypeAlias == null)
				{
					attribute.TypeAlias = smi2.SerializedName;
				}
				
				dictionaryItem.Attribute = attribute;
				dictionaryItem.TypeAlias = attribute.TypeAlias;

				// Check if a specific type of serializer is specified.

				if (attribute.SerializerType == null)
				{
					// Figure out the serializer based on the type of the element.

					dictionaryItem.Serializer = cache.GetTypeSerializerBySupportedType(attribute.ElementType, smi2);
				}
				else
				{
					// Get the type of serializer they specify.

					dictionaryItem.Serializer = cache.GetTypeSerializerBySerializerType(attribute.SerializerType, smi2);
				}

				primaryDictionaryItem = dictionaryItem;

				typeToItemMap[attribute.ElementType] = dictionaryItem;
				aliasToItemMap[attribute.TypeAlias] = dictionaryItem;
			}

			if (aliasToItemMap.Count != 1)
			{
				primaryDictionaryItem = null;
			}
		}

		private DictionaryItem primaryDictionaryItem = null;

		protected override void SerializeElements(object obj, XmlWriter writer, SerializationContext state)
		{
			DictionaryItem item;			
			var dicObj = (IDictionary)obj;
						
			foreach (var key in dicObj.Keys)
			{
				if (primaryDictionaryItem == null)
				{
					item = typeToItemMap[obj.GetType()];
				}
				else
				{
					item = primaryDictionaryItem;
				}

				writer.WriteStartElement(key.ToString());

				if (item.Attribute != null
						&& item.Attribute.SerializeAsValueNode
						&& item.Attribute.ValueNodeAttributeName != null
						&& item.Serializer is TypeSerializerWithSimpleTextSupport)
				{
					writer.WriteAttributeString(item.Attribute.ValueNodeAttributeName,
						((TypeSerializerWithSimpleTextSupport)item.Serializer).Serialize(dicObj[key], state));
				}
				else
				{
					item.Serializer.Serialize(dicObj[key], writer, state);
				}

				writer.WriteEndElement();
			}
		}
	}
}
