using System;
using Platform.Collections;

namespace Platform.Xml.Serialization
{
	public class SerializerOptions
		: DictionaryWrapper<string, object>
	{
		public static readonly string WriteXmlheader = "WriteXmlheader";

		private static SerializerOptions empty = new SerializerOptions();

		public static SerializerOptions Empty
		{
			get
			{
				return empty;
			}
		}

		public SerializerOptions(params object[] options)
			: base(null)
		{
			var dictionary = new LinearHashDictionary<string, object>();

			for (int i = 0; i < options.Length; i += 2)
			{
				dictionary[options[i].ToString()] = options[i + 1];
			}

			this.Wrappee = dictionary.ToReadOnly();
		}
	}
}
