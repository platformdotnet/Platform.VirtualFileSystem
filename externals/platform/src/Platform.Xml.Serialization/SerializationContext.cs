using System;
using System.Collections;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Maintains state required to perform serialization.
	/// </summary>
	public class SerializationContext
	{
		private readonly IList stack = new ArrayList();

		public SerializationParameters Parameters
		{
			get;
			private set;
		}

		public virtual SerializerOptions SerializerOptions
		{
			get;
			set;
		}

		public SerializationContext(SerializerOptions options, SerializationParameters parameters)
		{			
			this.Parameters = parameters;
			SerializerOptions = options;
		}
		
		/// <summary>
		/// Checks if see if an object should be serialized.
		/// </summary>
		/// <remarks>
		/// <p>
		/// An object shouldn't be serialized if it has already been serialized.
		/// This method automatically checks if the object has been serialized
		/// by examining the serialization stack.  This stack is maintained by
		/// the SerializationStart and SerializationEnd methods.
		/// </p>
		/// <p>
		/// You should call SerializationStart and SerializationEnd when you start
		/// and finish serializing an object.
		/// </p>
		/// </remarks>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool ShouldSerialize(object obj)
		{
			IXmlSerializationShouldSerializeProvider shouldSerialize;

			if (obj == null)
			{
				return false;
			}

			if ((shouldSerialize = obj as IXmlSerializationShouldSerializeProvider) != null)
			{
				if (!shouldSerialize.ShouldSerialize(this.SerializerOptions, this.Parameters))
				{
					return false;
				}
			}

			for (int i = 0; i < stack.Count; i++)
			{
				if (stack[i] == obj)
				{
					return false;
				}
			}

			return true;
		}

		private readonly Stack serializationMemberInfoStack = new Stack();

		public void PushCurrentMemberInfo(SerializationMemberInfo memberInfo)
		{
			serializationMemberInfoStack.Push(memberInfo);
		}

		public void PopCurrentMemberInfo()
		{
			serializationMemberInfoStack.Pop();
		}

		public SerializationMemberInfo GetCurrentMemberInfo()
		{
			return (SerializationMemberInfo)serializationMemberInfoStack.Peek();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void DeserializationStart(object obj)
		{
			var listener = obj as IXmlDeserializationStartListener;

			if (listener != null)
			{
				listener.XmlDeserializationStart(Parameters);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		public void DeserializationEnd(object obj)
		{
			IXmlDeserializationEndListener listener;
			
			listener = obj as IXmlDeserializationEndListener;

			if (listener != null)
			{
				listener.XmlDeserializationEnd(Parameters);
			}
		}

		/// <summary>
		/// Prepares an object for serialization/
		/// </summary>
		/// <remarks>
		/// The object is pushed onto the serialization stack. 
		/// This prevents the object from being serialized in cycles.
		/// </remarks>
		/// <param name="obj"></param>
		public void SerializationStart(object obj)
		{
			stack.Add(obj);

			var listener = obj as IXmlSerializationStartListener;

			if (listener != null)
			{
				listener.XmlSerializationStart(Parameters);
			}
		}

		/// <summary>
		/// Call when an object has been serialized.
		/// </summary>
		/// <remarks>
		/// The object is popped off the serialization stack.		
		/// </remarks>
		/// <param name="obj"></param>
		public void SerializationEnd(object obj)
		{
			IXmlSerializationEndListener listener;			

			if (stack[stack.Count - 1] != obj)
			{
				stack.RemoveAt(stack.Count - 1);

				throw new InvalidOperationException("Push/Pop misalignment.");
			}

			stack.RemoveAt(stack.Count - 1);

			listener = obj as IXmlSerializationEndListener;

			if (listener != null)
			{
				listener.XmlSerializationEnd(Parameters);
			}
		}
	}
}
