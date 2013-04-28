using System;
using System.IO;
using System.Xml;
using System.Reflection;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Abstract base class for classes that support serializing objects.
	/// </summary>
	public abstract class TypeSerializer
	{
		/// <summary>
		/// Returns true if this serializer can only be used with a specific member.
		/// </summary>
		public virtual bool MemberBound
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns the type that this serializer supports.
		/// </summary>
		public abstract Type SupportedType
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="writer"></param>
		/// <param name="state"></param>
		public abstract void Serialize(object obj, XmlWriter writer, SerializationContext state);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public abstract object Deserialize(XmlReader reader, SerializationContext state);
	}
}
