using System;
using System.Text;
using Platform.Text;

namespace Platform.VirtualFileSystem.Network.Text.Protocol
{
	/// <summary>
	/// Converter class to and form c# and protocol types.
	/// </summary>
	public class ProtocolTypes
	{
		private static readonly object[] m_TypeNames = 
		{
			typeof(bool),
			"b",
			typeof(byte),
			"i8",
			typeof(char),
			"c",
			typeof(short),
			"i16",
			typeof(int),
			"i32",
			typeof(Int64),
			"i64",
            typeof(DateTime),
            "dt",
			typeof(string),
			"s",
			typeof(byte[]),
			"b[]"
		};

		public static string DecodeString(string value, string encoding)
		{
			switch (encoding)
			{
				case "url":
					return TextConversion.FromEscapedHexString(value);
				case "b64":
					return Encoding.UTF8.GetString(TextConversion.FromBase64String(value));
				default:
					throw new NotSupportedException();
			}
		}

		public static string ToString(object value)
        {
			if (Array.IndexOf(m_TypeNames, value.GetType()) < 0)
			{
				throw new NotSupportedException(value.GetType().ToString());
			}

            if (value is DateTime)
            {
                return (((DateTime)value).ToUniversalTime().ToString(DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString));
            }
			else if (value is bool)
			{
				return ((bool)value) ? "t" : "f";
			}
			else if (value is byte[])
			{
				return TextConversion.ToBase64String((byte[])value);
			}
			else
			{
				return Convert.ToString(value);
			}
        }

		private static object PrivateFromString(string s, bool escaped)
		{
			int x;
			Type type;
			string typeName, value;

			x = s.IndexOf(':');

			if (x < 0)
			{
				throw new ArgumentException();
			}

			typeName = s.Substring(0, x);
			value = s.Substring(x + 1);

			if ((type = GetType(typeName)) == null)
			{
				throw new NotSupportedException(typeName);
			}

			if (type == typeof(DateTime))
			{
				DateTime retval;

				retval =
					DateTime.ParseExact(value, DateTimeFormats.SortableUtcDateTimeFormatWithFractionSecondsString,
					                    System.Globalization.CultureInfo.InvariantCulture);
				retval = DateTime.SpecifyKind(retval, DateTimeKind.Utc);

				return retval.ToLocalTime();
			}
			else if (type == typeof(byte[]))
			{
				byte[] retval;

				retval = TextConversion.FromBase64String(value);

				return retval;
			}
			else if (type == typeof(bool))
			{
				value = value.ToLower();

				if (value == "t")
				{
					return true;
				}
				else if (value == "f")
				{
					return false;
				}
				else
				{
					return Convert.ChangeType(value, type);
				}
			}
			else
			{
				if (escaped)
				{
					if (type == typeof(string))
					{
						return TextConversion.FromEscapedHexString(value);
					}
				}

				return Convert.ChangeType(value, type);
			}
		}

		public static object FromString(string s)
		{
			return PrivateFromString(s, false);
		}

		public static object FromEscapedString(string s)
		{
			return PrivateFromString(s, true);
		}
        
        public static string ToEscapedString(object value)
        {
			if (value is string)
			{
				return TextConversion.ToEscapedHexString
				(
					(string)value,
					c => !Char.IsLetterOrDigit(c)
					     && c != '.'
					     && c != '_'
				);
			}
			else
			{
				return ToString(value);
			}
        }

		public static string GetTypeName(Type type)
		{
			for (int i = 0; i < m_TypeNames.Length; i += 2)
			{
				if ((Type)m_TypeNames[i] == type)
				{
					return (string)m_TypeNames[i + 1];
				}
			}

			return null;
		}

		public static Type GetType(string name)
		{
			for (int i = 1; i < m_TypeNames.Length; i += 2)
			{
				if (m_TypeNames[i].Equals(name))
				{
					return (Type)m_TypeNames[i - 1];
				}
			}

			return null;
		}
	}
}
