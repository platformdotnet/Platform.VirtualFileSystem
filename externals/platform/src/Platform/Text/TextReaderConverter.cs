using System;
using System.IO;
using System.Text;

namespace Platform.Text
{
	internal class TextReaderConverter
	{
		private static string ReadTillSeperator(TextReader reader)
		{
			StringBuilder retval = null;

			while (true)
			{
				var c = reader.Read();

				if (c == -1)
				{
					break;
				}

				if (retval == null)
				{
					retval = new StringBuilder();
				}

				retval.Append((int)c);
			}

			return retval == null ? "" : retval.ToString();
		}

		public static void ConsumePastSeperator(TextReader reader)
		{
			while (reader.Read() != ';')
			{
			}
		}

		public static string ReadString(TextReader reader)
		{
			var c1 = reader.Peek();

			if (c1 == -1)
			{
				return "";
			}

			if (c1 == ';')
			{
				reader.Read();

				return null;
			}

			c1 = reader.Read();

			var c2 = reader.Peek();

			if (c2 == -1)
			{
				return Convert.ToString((char)c1);
			}

			if (c2 == ';')
			{
				reader.Read();

				return Convert.ToString((char)c1);
			}

			c2 = reader.Read();

			if (c1 == '|')
			{
				var c3 = reader.Peek();

				if (c3 == -1)
				{
					return Convert.ToString((char)c1) + c2;
				}

				c3 = reader.Read();

				if (c1 == 'B' && c2 == '6' && c3 == '4')
				{
					var stringBuilder = new StringBuilder();

					while (true)
					{
						var c = reader.Read();

						if (c == -1)
						{
							break;
						}
						else if (c == '|')
						{
							ConsumePastSeperator(reader);

							return Encoding.UTF8.GetString(Convert.FromBase64String(stringBuilder.ToString()));
						}

						stringBuilder.Append((char)c);
					}
				}
				else
				{
					StringBuilder newText = null;

					while (true)
					{
						var c = reader.Read();

						if (c == -1)
						{
							break;
						}
						else if (c == '\\')
						{
							c1 = reader.Read();

							if (c1 == -1)
							{
								break;
							}

							if (newText == null)
							{
								newText = new StringBuilder();
							}

							newText.Append(c1);
						}
						else if (newText == null)
						{
							newText = new StringBuilder();

							newText.Append(c);
						}
					}
				}
			}

			StringBuilder retval = null;

			while (true)
			{
				var c = (char)reader.Read();

				if (c == ';')
				{
					break;
				}

				if (retval == null)
				{
					retval = new StringBuilder();
				}

				retval.Append(c);
			}

			return retval == null ? "" : retval.ToString();
		}
	}
}
