using System;
using System.Collections;

namespace Platform.Xml.Serialization
{
	public abstract class XmlSerializationAttribute
		: Attribute
	{
		protected XmlSerializationAttribute()
		{
			Constraints = "";
		}

		public string Constraints
		{
			get;
			set;
		}

		/// <summary>
		/// Tests if this attribute should be applied/considered when serializing.
		/// </summary>
		/// <param name="options"></param>
		internal virtual bool Applies(SerializerOptions options)
		{
			if (Constraints.Length > 0)
			{
				var constraints = Constraints.Split(';');

				foreach (string s in constraints)
				{
					object value;
					var forNotEquals = true;

					var namevalue = s.Split(new string[] { "!=" }, StringSplitOptions.None);

					if (namevalue.Length < 2)
					{
						namevalue = s.Split('=');

						forNotEquals = false;
					}

					if (!options.TryGetValue(namevalue[0], out value))
					{
						if (forNotEquals)
						{
							return true;

						}
						return false;
					}

					if (forNotEquals)
					{
						if (value.ToString().EqualsIgnoreCase(namevalue[1]))
						{
							return false;
						}
					}
					else
					{
						if (!value.ToString().EqualsIgnoreCase(namevalue[1]))
						{
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}
