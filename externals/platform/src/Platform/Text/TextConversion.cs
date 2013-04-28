using System;
using System.IO;
using System.Collections;
using System.Text;
using Platform.IO;

namespace Platform.Text
{
	/// <summary>
	/// Provides useful methods for converting text.
	/// </summary>
	public class TextConversion
	{
		public static readonly char[] Base32Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
		public static readonly char[] Base32AlphabetLowercase = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
		
		public static readonly char[] HexValues = "0123456789ABCDEF".ToCharArray();
		public static readonly char[] HexValuesLowercase = "0123456789abcdef".ToCharArray();

		/// <summary>
		/// Decodes a string from an escaped hex string (URL encoded).
		/// </summary>
		/// <param name="s">The escaped hex encoded string</param>
		/// <returns>The deocded string</returns>
		public static string FromEscapedHexString(string s)
		{
			return Uri.UnescapeDataString(s);
		}

		public static readonly char[] soundexValues =
		{
			//A  B   C   D   E   F   G   H   I   J   K   L   M
			'0','1','2','3','0','1','2','0','0','2','2','4','5',
			//N  O   P   W   R   S   T   U   V   W   X   Y   Z
			'5','0','1','2','6','2','3','0','1','0','2','0','2'
		};

		public static void WriteSoundex(TextWriter writer, string value)
		{
			var length = 0;
			var previousChar = '?';
			
			if (value.Length > 0)
			{
				writer.Write(value[0]);

				for (var i = 1; i < value.Length && length < 4; i++)
				{
					var currentChar = Char.ToUpper(value[i]);

					if (currentChar >= 'A' && currentChar <= 'Z' && currentChar != previousChar)
					{
						var soundexChar = soundexValues[currentChar - 'A'];

						if (soundexChar != '0')
						{
							length++;
							writer.Write(soundexChar);
						}

						previousChar = currentChar;
					}
				}
			}

			while (length < 4)
			{
				length++;
				writer.Write('0');
			}
		}

		public static string ToSoundex(string value)
		{
			var previousChar = '?';
			var retval = new StringBuilder(4);

			if (value.Length > 0)
			{
				retval.Append(value[0]);

				for (var i = 1; i < value.Length && retval.Length < 4; i++)
				{
					var currentChar = Char.ToUpper(value[i]);

					if (currentChar >= 'A' && currentChar <= 'Z' && currentChar != previousChar)
					{
						var soundexChar = soundexValues[currentChar - 'A'];

						if (soundexChar != '0')
						{
							retval.Append(soundexChar);
						}

						previousChar = currentChar;
					}
				}
			}

			while (retval.Length < 4)
			{
				retval.Append('0');
			}
            
			return retval.ToString();
		}

		public static TextWriter WriteUnescapedHexString(StringReader reader, StringWriter writer)
		{
			int x, y;
			int charcount;
			Decoder decoder;
			byte[] bytes = new byte[1];
			char[] chars = new char[1];
			
			decoder = Encoding.UTF8.GetDecoder();
			
			for (;;)
			{
				x = reader.Read();

				if (x == -1)
				{
					break;
				}

				if (x != '%')
				{
					writer.Write((char)x);

					continue;
				}

				x = reader.Read();

				if (x == -1)
				{
					break;
				}

				if (!((char)x).IsHexDigit())
				{					
					writer.Write('%');
					writer.Write((char)x);

					continue;
				}

				y = reader.Read();

				if (y == -1)
				{
					break;
				}

				if (!((char)y).IsHexDigit())
				{
					writer.Write('%');
					writer.Write((char)x);
					writer.Write((char)y);
				}

				bytes[0] = (byte)(IntUtils.FromHexNoCheck((char)x) * 0x10 + IntUtils.FromHexNoCheck((char)y));

				charcount = decoder.GetChars(bytes, 0, 1, chars, 0);

				if (charcount > 0)
				{
					writer.Write(chars[0]);
				}
			}

			return writer;
		}

		private static readonly Predicate<char> c_IsAsciiLetterOrDigit = PredicateUtils.Not<char>(CharUtils.IsAsciiLetterOrDigit);

		public static string ToEscapedHexString(string s)
		{
			return ToEscapedHexString(s, c_IsAsciiLetterOrDigit);
		}

		public static string ToEscapedHexString(string s, string charsToEscape)
		{
			var builder = new StringBuilder((int)(s.Length * 1.5));

			foreach (char c in s)
			{
				if (c == '%' || charsToEscape.IndexOf(c) >= 0)
				{
					builder.Append('%');
					builder.Append(HexValues[(c & '\x00f0') >> 4]);
					builder.Append(HexValues[c & '\x000f']);
				}
				else
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		public static bool IsStandardUrlEscapedChar(char c)
		{
			return !IsStandardUrlUnEscapedChar(c);
		}

		public static bool IsStandardUrlUnEscapedChar(char c)
		{
			if (c == ' ' || c == '|' || c == '{' || c == '}' || c == '^'
				|| c == '<' || c == '>' || c == '"' || c == '\\' || c == '%'
				|| c == ';')
			{
				return false;
			}

			return ((int)c) >= 21 && ((int)c) <= 126;
		}

		public static string ToReEscapedHexString(string s, Predicate<char> shouldEscape)
		{
			int skip = 0;

			return ToEscapedHexString
			(
				s,
				delegate(Pair<string, int> context)
				{
					char c;

					if (skip > 0)
					{
						skip--;

						return false;
					}

					c = context.Left[context.Right];

					if (Uri.IsHexEncoding(context.Left, context.Right))
					{
						skip = 2;

						return false;
					}

					if (c == '%' || shouldEscape(c))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			);
		}

		public static string ToEscapedHexString(string s, Predicate<Pair<string, int>> shouldEscapeChar)
		{
			byte[] buffer;
			char[] chars = new char[1];
			Encoding encoding = Encoding.UTF8;
			StringBuilder builder = new StringBuilder((int)(s.Length * 1.5));

			buffer = new byte[encoding.GetMaxByteCount(1)];
						
			for (int i = 0; i < s.Length; i++)
			{			
				if (shouldEscapeChar(new Pair<string, int>(s, i)))
				{
					int bytecount;

					chars[0] = s[i];

					bytecount = encoding.GetBytes(chars, 0, 1, buffer, 0);

					for (int j = 0; j < bytecount; j++)
					{
						builder.Append('%');
						builder.Append(HexValues[(buffer[j] & '\x00f0') >> 4]);
						builder.Append(HexValues[buffer[j] & '\x000f']);
					}
				}
				else
				{					
					builder.Append(s[i]);
				}
			}

			return builder.ToString();
		}
		
		public static string ToEscapedHexString(string s, Predicate<char> shouldEscapeChar)
		{
			return ToEscapedHexString
			(
				s,
				delegate(Pair<string, int> stringInt)
				{
					return shouldEscapeChar(stringInt.Key[stringInt.Value]);
				}
			);
		}

		public static TextWriter WriteEscapedHexString(TextReader reader, TextWriter writer)
		{
			return WriteEscapedHexString
				(
					reader,
					writer,
					c_IsAsciiLetterOrDigit
				);
		}

		public static TextWriter WriteEscapedHexString(TextReader reader, TextWriter writer, Predicate<char> shouldEscapeChar)
		{
			byte[] buffer;
			char[] chars = new char[1];
			Encoding encoding = Encoding.UTF8;
			
			buffer = new byte[encoding.GetMaxByteCount(1)];

			return reader.ConvertAndDump(writer,
			                delegate(char c, TextWriter w)
			                	{
			                		if (c == '%' || shouldEscapeChar(c))
			                		{
			                			int bytecount;

			                			chars[0] = c;

			                			bytecount = encoding.GetBytes(chars, 0, 1, buffer, 0);

			                			for (int j = 0; j < bytecount; j++)
			                			{
			                				w.Write('%');
			                				w.Write(HexValues[(buffer[j] & '\x00f0') >> 4]);
			                				w.Write(HexValues[(buffer[j] & '\x00f0') >> 4]);								
			                			}
			                		}
			                		else
			                		{
			                			w.Write(c);
			                		}
			                	});
		}

		public static string ToString(string s, Converter<char, char> convert)
		{
			return StringUtils.Convert(s, convert);
		}

		public static string ToString<I, O>(System.Collections.Generic.IEnumerable<I> input, Converter<I, O> convert)
		{
			StringBuilder builder;

			builder = new StringBuilder();

			foreach (I value in input)
			{
				builder.Append(convert(value));
			}

			return builder.ToString();
		}

		public static bool IsHexChar(char c)
		{
			return (c >= 'A' && c <= 'Z')
				|| (c >= 'a' && c <= 'z')
				|| (c >= '0' && c <= '9');
		}

		public static int FromHex(char c)
		{
			if (c >= '0' && c <= '9')
			{
				return c - '0';
			}
			else if (c >= 'A' && c <= 'Z')
			{
				return c - 'A' + 10;
			}
			else if (c >= 'a' && c <= 'z')
			{
				return c - 'a' + 10;
			}
			else
			{
				throw new ArgumentException("Not a hex char", "c");
			}
		}

		public static char ToHexChar(long x)
		{
			if (x > Int32.MaxValue || x < Int32.MinValue)
			{
				throw new ArgumentException("Not a hex char", "x");
			}

			return ToHexChar((int)x, false);
		}

		public static char ToHexChar(short x)
		{
			return ToHexChar((int)x, false);
		}

		public static char ToHexChar(byte x)
		{
			return ToHexChar((int)x, false);
		}

		public static char ToHexChar(int x)
		{
			return ToHexChar(x, false);
		}

		public static char ToHexChar(int x, bool lowercase)
		{
			return ToHexChar(x, lowercase ? HexValuesLowercase : HexValues);
		}

		private static char ToHexChar(int x, char[] hexValues)
		{
			if (x < 0 || x > 16)
			{
				throw new ArgumentException("Not a hex char", "c");
			}

			return hexValues[x];
		}

		public static byte[] FromHexString(string s)
		{
			byte[] bytes;

			bytes = new byte[s.Length / 2];

			for (int i = 0; i < s.Length; i += 2)
			{
				int x, y;
		
				if (!IsHexChar(s[i])
					|| !IsHexChar(s[i + 1]))
				{
					throw new ArgumentException("Contains invlaid hexadecimal values", "s");
				}

				try
				{
					x = FromHex(s[i]);
					y = FromHex(s[i + 1]);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);

					throw;
				}

				bytes[i / 2] = (byte)((x << 4) | y);
			}

			return bytes;
		}

		public static string ToHexString(long value)
		{
			byte[] bytes = new byte[8];

			bytes[7] = (byte)((value) >> 0 & 0xf);
			bytes[6] = (byte)((value) >> 8 & 0xf);
			bytes[5] = (byte)((value) >> 16 & 0xf);
			bytes[4] = (byte)((value) >> 24 & 0xf);
			bytes[3] = (byte)((value) >> 32 & 0xf);
			bytes[2] = (byte)((value) >> 40 & 0xf);
			bytes[1] = (byte)((value) >> 48 & 0xf);
			bytes[0] = (byte)((value) >> 56 & 0xf);

			return ToHexString(bytes);
		}
		
		public static string ToHexString(int value)
		{
			byte[] bytes = new byte[4];

			bytes[2] = (byte)((value >> 0) & 0xf);
			bytes[3] = (byte)((value >> 8) & 0xf);
			bytes[1] = (byte)((value >> 16) & 0xf);
			bytes[0] = (byte)((value >> 24) & 0xf);

			return ToHexString(bytes);
		}

		public static string ToHexString(byte[] bytes)
		{
			return ToHexString(bytes, false);
		}

		public static string ToHexString(byte[] bytes, bool lowercase)
		{
			return ToHexString(bytes, lowercase ? HexValuesLowercase : HexValues);	
		}

		private static string ToHexString(byte[] bytes, char[] hexValues)
		{
			StringBuilder builder;
	
			builder = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)
			{
				builder.Append(hexValues[((int)b >> 4)]);
				builder.Append(hexValues[((int)b & 15)]);
			}

			return builder.ToString();
		}

		public static string ToBase32String(long x)
		{
			byte[] data;

			data = new byte[8];

			data[0] = (byte)((x & 0xff));
			data[1] = (byte)((x >> 8) & 0xff);
			data[2] = (byte)((x >> 16) & 0xff);
			data[3] = (byte)((x >> 24) & 0xff);
			data[4] = (byte)((x >> 32) & 0xff);
			data[5] = (byte)((x >> 40) & 0xff);
			data[6] = (byte)((x >> 48) & 0xff);
			data[7] = (byte)((x >> 56) & 0xff);

			return ToBase32String(data);
		}

		public static string ToBase32String(int x)
		{
			byte b0, b1, b2, b3, b4;
			char[] out_block = new char[8];

			b0 = (byte)((x & 0xff));
			b1 = (byte)((x & 0xff00) >> 8);
			b2 = (byte)((x & 0xff0000) >> 16);
			b3 = (byte)((x & 0xff000000) >> 24);
			b4 = 0;
					
			out_block[0] = Base32Alphabet[((b0 & 0xF8) >> 3)];
			out_block[1] = Base32Alphabet[((b0 & 0x7) << 2) | ((b1 & 0xc0) >> 6)];
			out_block[2] = Base32Alphabet[((b1 & 0x3e) >> 1)];
			out_block[3] = Base32Alphabet[((b1 & 0x1) << 4) | ((b2 & 0xf0) >> 4)];
			out_block[4] = Base32Alphabet[((b2 & 0xf) << 1) | ((b3 & 0x80) >> 7)];
			out_block[5] = Base32Alphabet[((b3 & 0x7c) >> 2)];
			out_block[6] = Base32Alphabet[((b3 & 0x3) << 3) | ((b4 & 0xe0) >> 5)];
			out_block[7] = Base32Alphabet[((b4 & 0x1f))];

			return new string(out_block);
		}

		public static byte[] FromBase64CharArray(char[] array, int offset, int length)
		{
			return Convert.FromBase64CharArray(array, offset, length);
		}

		public static byte[] FromBase64String(string s)
		{
			return Convert.FromBase64String(s);
		}

		public static string ToBase64String(byte[] input)
		{
			return Convert.ToBase64String(input, 0, input.Length);
		}

		public static string ToBase64String(byte[] input, int offset, int length)
		{
			return Convert.ToBase64String(input, offset, length);
		}

		public static int ToBase64CharArray(byte[] input, int offsetIn, int length, char[] outArray, int offsetOut)
		{
			return Convert.ToBase64CharArray(input, offsetIn, length, outArray, offsetOut);
		}

		public static int ToBase32CharArray(byte[] input, int offsetIn, int length, char[] outArray, int offsetOut)
		{
			string value = ToBase32String(input, offsetIn, length);

			value.CopyTo(0, outArray, offsetOut, value.Length);

			return value.Length;
		}

		public static string ToBase32String(byte[] input)
		{
			return ToBase32String(input, 0, input.Length);
		}

		public static string ToBase32String(byte[] input, int offset, int length)
		{
			int ex = -1;
			byte b0, b1, b2, b3, b4;
			char[] out_block = new char[8];
			StringBuilder buffer = new StringBuilder(input.Length * 2);

			for (int i = offset; i < offset + input.Length; i += 5)
			{
				if (i + 4 < input.Length)
				{
					b0 = input[i];
					b1 = input[i+1];
					b2 = input[i+2];
					b3 = input[i+3];
					b4 = input[i+4];
					
					out_block[0] = Base32Alphabet[((b0 & 0xF8) >> 3)];
					out_block[1] = Base32Alphabet[((b0 & 0x7) << 2) | ((b1 & 0xc0) >> 6)];
					out_block[2] = Base32Alphabet[((b1 & 0x3e) >> 1)];
					out_block[3] = Base32Alphabet[((b1 & 0x1) << 4) | ((b2 & 0xf0) >> 4)];
					out_block[4] = Base32Alphabet[((b2 & 0xf) << 1) | ((b3 & 0x80) >> 7)];
					out_block[5] = Base32Alphabet[((b3 & 0x7c) >> 2)];
					out_block[6] = Base32Alphabet[((b3 & 0x3) << 3) | ((b4 & 0xe0) >> 5)];
					out_block[7] = Base32Alphabet[((b4 & 0x1f))];
				}
				else
				{
					b0 = b1 = b2 = b3 = b4 = 0;

					if (i < input.Length)
					{
						b0 = input[i];
					}

					if (i + 1 < input.Length)
					{
						b1 = input[i+1];
					}

					if (i + 2 < input.Length)
					{
						b2 = input[i+2];
					}

					if (i + 3 < input.Length)
					{
						b3 = input[i+3];
					}

					if (i + 4 < input.Length)
					{
						b4 = input[i+4];
					}

					out_block[0] = Base32Alphabet[((b0 & 0xF8) >> 3)];
					out_block[1] = Base32Alphabet[((b0 & 0x7) << 2) | ((b1 & 0xc0) >> 6)];

					if (i + 1 >= input.Length)
					{
						ex = 2;
						goto end;
					}

					out_block[2] = Base32Alphabet[((b1 & 0x3e) >> 1)];
					out_block[3] = Base32Alphabet[((b1 & 0x1) << 4) | ((b2 & 0xf0) >> 4)];

					if (i + 2 >= input.Length)
					{
						ex = 4;
						goto end;
					}

					out_block[4] = Base32Alphabet[((b2 & 0xf) << 1) | ((b3 & 0x80) >> 7)];

					if (i + 3 >= input.Length)
					{
						ex = 5;
						goto end;
					}

					out_block[5] = Base32Alphabet[((b3 & 0x7c) >> 2)];
					out_block[6] = Base32Alphabet[((b3 & 0x3) << 3) | ((b4 & 0xe0) >> 5)];

					if (i + 4 >= input.Length)
					{
						ex = 7;
						goto end;
					}

					out_block[7] = Base32Alphabet[((b4 & 0x1f))];
				}

			end:

				if (ex > -1)
				{
					for (int j = ex; j < out_block.Length; j++)
					{
						out_block[j] = '=';
					}
				}

				ex = -1;

				buffer.Append(out_block);
			}

			return buffer.ToString();
		}
	}
}
