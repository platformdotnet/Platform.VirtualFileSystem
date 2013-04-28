using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Platform.Xml.Serialization
{
	#region DateTime attributes
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]	
	public class XmlDateTimeFormatAttribute
		: XmlSerializationAttribute
	{
		public string Format
		{
			get;

			set;
		}

		public XmlDateTimeFormatAttribute(string format)
		{
			Format = format;
		}
	}
	#endregion

	public class DateTimeTypeSerializer
		: TypeSerializerWithSimpleTextSupport
	{
		private readonly bool formatSpecified = false;

		/// <summary>
		/// 
		/// </summary>
		private readonly XmlDateTimeFormatAttribute formatAttribute;

		/// <summary>
		/// 
		/// </summary>
		public override bool MemberBound
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override Type SupportedType
		{
			get
			{
				return typeof(DateTime);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="memberInfo"></param>
		/// <param name="cache"></param>
		/// <param name="options"></param>
		public DateTimeTypeSerializer(SerializationMemberInfo memberInfo, TypeSerializerCache cache, SerializerOptions options)
		{
			formatAttribute = (XmlDateTimeFormatAttribute)memberInfo.GetFirstApplicableAttribute(typeof(XmlDateTimeFormatAttribute));
			
			if (formatAttribute == null)
			{				
				formatAttribute = new XmlDateTimeFormatAttribute("G");
				formatSpecified = false;
			}
			else
			{
				formatSpecified = true;
 			}
		}

		public override string Serialize(object obj, SerializationContext state)
		{
			return ((DateTime)obj).ToString(formatAttribute.Format);
		}

		public override object Deserialize(string value, SerializationContext state)
		{
			if (formatSpecified)
			{
				try
				{
					// Try parsing using the specified format.

					return DateTime.ParseExact(value, formatAttribute.Format, System.Globalization.CultureInfo.CurrentCulture);
				}
				catch 
				{				
				}
			}

			// Try parsing with the system supplied strategy.

			return DateTime.Parse(value);
		}
	}
}