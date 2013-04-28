using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// 
	/// </summary>
	public interface IXmlSerializationShouldSerializeProvider
	{
		bool ShouldSerialize(SerializerOptions options, SerializationParameters parameters);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IXmlSerializationStartListener
	{
		/// <summary>
		/// Called when the object is about to be serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object before its properties have been serilized.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlSerializationStart(SerializationParameters parameters);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IXmlSerializationEndListener
	{	
		/// <summary>
		/// Called when an object has been serialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been serialized.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlSerializationEnd(SerializationParameters parameters);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IXmlDeserializationStartListener
	{
		/// <summary>
		/// Called when an object is about to be deserialized.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after it has been constructed but
		/// before its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlDeserializationStart(SerializationParameters parameters);
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IXmlDeserializationEndListener
	{
		/// <summary>
		/// Called when an object has been deseriazlied.
		/// </summary>
		/// <remarks>
		/// This method is called on an object after its properties have been set.
		/// </remarks>
		/// <param name="parameters"></param>
		void XmlDeserializationEnd(SerializationParameters parameters);
	}
}