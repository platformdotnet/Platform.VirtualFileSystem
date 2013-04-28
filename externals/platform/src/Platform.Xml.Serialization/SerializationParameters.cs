using System;
using System.Collections.Generic;

namespace Platform.Xml.Serialization
{
	public class SerializationParameters
	{
		private readonly IDictionary<string, object> parameters;

		private static readonly SerializationParameters empty = new SerializationParameters();

		public static SerializationParameters Empty
		{
			get
			{
				return empty;
			}
		}

		public XmlSerializerNamespaces Namespaces
		{
			get;
			private set;
		}

		public SerializationParameters()
		{
			this.Namespaces = new XmlSerializerNamespaces();
			parameters = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);
		}

		public virtual object this[string name]
		{
			get
			{
				return parameters[name];
			}

			set
			{
				if (this == empty)
				{
					return;
				}

				parameters[name] = value;
			}
		}

		public virtual bool ContainsByName(string name)
		{
			return parameters.ContainsKey(name);
		}

		public virtual bool TryGetValue(string name, out object value)
		{
			return parameters.TryGetValue(name, out value);
		}
	}
}
