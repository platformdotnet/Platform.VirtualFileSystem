using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Platform.Linq;
using Platform.Reflection;

namespace Platform.Text
{
	public class TextSerializer
	{
		internal static readonly MethodInfo WriteValueMethod;
		internal static readonly MethodInfo WriteStringValueMethod;
		internal static readonly MethodInfo WriteDateTimeValueMethod;
		internal static readonly MethodInfo WriteLiteralStringValueMethod;
		internal static readonly MethodInfo ReadStringMethod = typeof(TextSerializer).GetMethod("ReadString", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo ReadListMethod = typeof(TextSerializer).GetMethod("ReadList", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo ReadArrayMethod = typeof(TextSerializer).GetMethod("ReadArray", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo ReadDictionaryMethod = typeof(TextSerializer).GetMethod("ReadDictionary", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo TextConversionFromEscapedHexStringMethod = typeof(TextConversion).GetMethod("FromEscapedHexString", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
		internal static readonly string GuidFormat = "N";
		internal const char LiteralChar = '|';
		internal const string CharsToEscapeInKey = "%=`";
		internal const string CharsToEscapeInValue = "[%;]";
		internal const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff";
		internal static readonly MethodInfo GetSerializerMethod = typeof(TextSerializer).GetMethod("GetSerializer");
		internal static readonly MethodInfo TextWriterWrite = typeof(TextWriter).GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo TextWriterWriteChar = typeof(TextWriter).GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(char) }, null);
		internal static readonly MethodInfo DeserializeFromStringMethod = typeof(TextSerializer).GetMethod("DeserializeFromString", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
		internal static readonly MethodInfo DeserializeFromReaderMethod = typeof(TextSerializer).GetMethod("DeserializeFromReader", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(TextReader) }, null);

		internal static readonly MethodInfo SerializeListToWriterMethod;
		internal static readonly MethodInfo SerializeArrayToWriterMethod;
		internal static readonly MethodInfo SerializeDictionaryToWriterMethod;

		static TextSerializer()
		{
			SerializeListToWriterMethod =
				typeof(TextSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static).First
				(
					c => c.Name == "SerializeToWriter" && !c.GetParameters()[0].ParameterType.IsArray && typeof(IList<>).IsAssignableFromIgnoreGenericParameters(c.GetParameters()[0].ParameterType)
				);

			SerializeArrayToWriterMethod =
				typeof(TextSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static).First
				(
					c => c.Name == "SerializeToWriter" && c.GetParameters()[0].ParameterType.IsArray
				);


			SerializeDictionaryToWriterMethod =
				typeof(TextSerializer).GetMethods(BindingFlags.Public | BindingFlags.Static).First
				(
					c => c.Name == "SerializeToWriter" && typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(c.GetParameters()[0].ParameterType)
				);

			WriteValueMethod = typeof(TextSerializer).GetMethod("WriteValue", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(TextWriter) }, null);
			WriteDateTimeValueMethod = typeof(TextSerializer).GetMethod("WriteValue", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(DateTime), typeof(TextWriter) }, null);
			WriteStringValueMethod = typeof(TextSerializer).GetMethod("WriteValue", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(TextWriter) }, null);
			WriteLiteralStringValueMethod = typeof(TextSerializer).GetMethod("WriteLiteralValue", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(TextWriter) }, null);
		}

		public static string ReadString(string text)
		{
			if (text == "[]")
			{
				return "";
			}

			var len = text.Length;

			if (len >= 2 && text[0] == '|' && text[len - 1] == '|')
			{
				StringBuilder builder = null;

				for (var i = 1; i < len - 1; i++)
				{
					var c1 = text[i];

					if (c1 == '|')
					{
						var c2 = text[++i];

						if (c2 == '|')
						{
							if (builder == null)
							{
								builder = new StringBuilder(text.Length);
								builder.Append(text, 1, i - 1);
							}
							else
							{
								builder.Append('|');
							}
						}
						else
						{
							break;
						}
					}
					else if (builder != null)
					{
						builder.Append(c1);
					}
				}

				return builder == null ? text.Substring(1, text.Length - 2) : builder.ToString();
			}

			return TextConversion.FromEscapedHexString(text);
		}

		internal static int GetNextOffset(string s, int offset, char endChar)
		{
			var depth = 0;

			if (s[offset] == '|')
			{
				return GetNextOffsetLiteral(s, offset);
			}

			for (var i = offset; i < s.Length; i++)
			{
				char c = s[i];

				if (c == endChar && depth == 0)
				{
					return i;
				}
				else if (c == '[')
				{
					depth++;
				}
				else if (c == ']')
				{
					depth--;
				}
			}

			return s.Length - 1;
		}

		internal static int GetNextOffsetLiteral(string text, int offset)
		{
			for (int i = offset + 1; i < text.Length; i++)
			{
				var c1 = text[i];

				if (c1 == '|')
				{
					var c2 = text[++i];

					if (c2 != '|')
					{
						return i;
					}
				}
			}

			return text.Length - 1;
		}

		internal static IEnumerable<string> GetValues(string stringValue)
		{
			for (var i = 1; i < stringValue.Length - 1; i++)
			{
				var offset = GetNextOffset(stringValue, i, ';');
				var value = stringValue.Substring(i, offset - i);

				i = offset;

				yield return value;
			}
		}

		internal static IEnumerable<KeyValuePair<string, string>> GetKeyValues(string stringValue)
		{
			for (var i = 1; i < stringValue.Length - 1; i++)
			{
				var keyOffset = GetNextOffset(stringValue, i, '=');
				var key = stringValue.Substring(i, keyOffset - i);
				var valueOffset = GetNextOffset(stringValue, i + key.Length, ';');
				var value = stringValue.Substring(i + key.Length + 1, valueOffset - (i + key.Length + 1));

				i = valueOffset;

				yield return new KeyValuePair<string, string>(key, value);
			}
		}

		public static L ReadList<L, T>(string text)
			where L : IList<T>, new()
		{
			var retval = new L();

			var serializer = TextSerializer.GetSerializer<T>();

			foreach (var subText in GetValues(text))
			{
				var value = serializer.Deserialize(subText);

				retval.Add(value);
			}

			return retval;
		}

		public static T[] ReadArray<T>(string text)
		{
			var retval = new List<T>();

			var serializer = TextSerializer.GetSerializer<T>();

			foreach (var subText in GetValues(text))
			{
				var value = serializer.Deserialize(subText);

				retval.Add(value);
			}

			return retval.ToArray();
		}

		public static D ReadDictionary<D, K, V>(string text)
			where D : IDictionary<K, V>, new()
		{
			var retval = new D();

			var keySerializer = TextSerializer.GetSerializer<K>();
			var valueSerializer = TextSerializer.GetSerializer<V>();

			foreach (var kvp in GetKeyValues(text))
			{
				var key = keySerializer.Deserialize(kvp.Key);
				var value = valueSerializer.Deserialize(kvp.Value);

				retval[key] = value;
			}

			return retval;
		}

		internal struct KVP<K, V>
		{
			public K Key;
			public V Value;
		}

		private const int DateTimeValueCacheSize = 101;
		private static readonly KVP<DateTime, string>[] CachedDateTimeValues = new KVP<DateTime, string>[DateTimeValueCacheSize];

		internal static void WriteValue(DateTime dateTime, TextWriter writer)
		{
			string value;
			var x = Math.Abs(dateTime.GetHashCode()) % DateTimeValueCacheSize;

			lock (CachedDateTimeValues)
			{
				if (CachedDateTimeValues[x].Key.Ticks == dateTime.Ticks)
				{
					writer.Write(CachedDateTimeValues[x].Value);

					return;
				}

				CachedDateTimeValues[x].Key = dateTime;
				value = dateTime.ToString(DateTimeFormat);
				CachedDateTimeValues[x].Value = value;
			}

			writer.Write(value);
		}

		internal static void WriteValue(object value, TextWriter writer)
		{
			if (value == null)
			{
				return;
			}

			writer.Write(value);
		}

		internal static void WriteValue(string value, TextWriter writer)
		{
			if (value == null)
			{
				return;
			}

			if (value.Length == 0)
			{
				writer.Write("[]");

				return;
			}

			var i = 0;
			var foundInvalidChar = false;

			foreach (char c in value)
			{
				if (c == '%' || CharsToEscapeInValue.IndexOf(c) >= 0 || (c == '|' && i == 0))
				{
					if (!foundInvalidChar)
					{
						foundInvalidChar = true;

						writer.Write(value.Substring(0, i));
					}

					writer.Write('%');
					writer.Write(TextConversion.HexValues[(c & '\x00f0') >> 4]);
					writer.Write(TextConversion.HexValues[c & '\x000f']);

					i++;
					continue;
				}

				if (foundInvalidChar)
				{
					writer.Write(c);
				}

				i++;
			}

			if (!foundInvalidChar)
			{
				writer.Write(value);
			}
		}

		internal static void WriteLiteralValue(string value, TextWriter writer)
		{
			if (value == null)
			{
				return;
			}

			if (value.Length == 0)
			{
				writer.Write("[]");

				return;
			}

			if (value.IndexOf('`') >= 0)
			{
				writer.Write('`');

				foreach (var c in value)
				{
					if (c == '`')
					{
						writer.Write('`');
						writer.Write('`');
					}
					else
					{
						writer.Write(c);
					}
				}

				writer.Write('`');
			}
			else
			{
				writer.Write('|');

				writer.Write(value);

				writer.Write('|');
			}
		}

		public static string SerializeToString<K, V>(IDictionary<K, V> dictionary)
		{
			var builder = new StringBuilder();

			SerializeToWriter(dictionary, new StringWriter(builder));

			return builder.ToString();
		}

		public static string SerializeToString<T>(IList<T> list)
		{
			var builder = new StringBuilder();

			SerializeToWriter(list, new StringWriter(builder));

			return builder.ToString();
		}

		public static void SerializeToWriter<T>(T[] list, TextWriter writer)
		{
			var x = 0;
			var count = list.Length;

			writer.Write('[');

			var serializer = GetSerializer<T>();

			foreach (T value in list)
			{
				serializer.Serialize(value, writer);

				x++;

				if (x != count)
				{
					writer.Write(';');
				}
			}

			writer.Write(']');
		}

		public static void SerializeToWriter<T>(IList<T> list, TextWriter writer)
		{
			var x = 0;
			var count = list.Count;

			writer.Write('[');

			var serializer = GetSerializer<T>();

			foreach (T value in list)
			{
				serializer.Serialize(value, writer);

				x++;

				if (x != count)
				{
					writer.Write(';');
				}
			}

			writer.Write(']');
		}

		public static void SerializeToWriter<K, V>(IDictionary<K, V> dictionary, TextWriter writer)
		{
			var x = 0;
			var count = dictionary.Count;

			writer.Write('[');

			var keySerializer = GetSerializer<K>();
			var valueSerializer = GetSerializer<V>();

			foreach (var pair in dictionary)
			{
				writer.Write(keySerializer.Serialize(pair.Key));
				writer.Write("=");
				writer.Write(valueSerializer.Serialize(pair.Value));

				x++;

				if (x != count)
				{
					writer.Write(';');
				}
			}

			writer.Write(']');
		}

		public static TextSerializer<T> GetSerializer<T>()
		{
			return TextSerializer<T>.Serializer;
		}

		public static string SerializeToString<T>(T value)
		{
			return GetSerializer<T>().Serialize(value);
		}

		public static void SerializeToWriter<T>(T value, TextWriter writer)
		{
			GetSerializer<T>().Serialize(value, writer);
		}

		public static T DeserializeFromString<T>(string text)
		{
			return GetSerializer<T>().Deserialize(text);
		}

		public static T DeserializeFromReader<T>(TextReader reader)
		{
			return GetSerializer<T>().Deserialize(reader);
		}
	}

	/// <summary>
	/// A serializer that serializes objects to strings
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TextSerializer<T>
	{
		internal static readonly TextSerializer<T> Serializer;

		private Action<T, TextWriter> serializerFunction;
		private readonly Dictionary<string, Type> propertyTypes = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, Delegate> deserializerFunctions = new Dictionary<string, Delegate>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<string, Func<T, string, T>> deserializerSetters = new Dictionary<string, Func<T, string, T>>(StringComparer.InvariantCultureIgnoreCase);

		static TextSerializer()
		{
			Serializer = new TextSerializer<T>();
		}

		internal TextSerializer()
		{
			BuildSerializerFunction();
			BuildObjectParser();
		}

		private bool IsSimplePropertyType(Type type)
		{
			if (Nullable.GetUnderlyingType(type) != null)
			{
				type = Nullable.GetUnderlyingType(type);
			}

			if (type.IsPrimitive || type.IsEnum)
			{
				return true;
			}

			if (type == typeof(DateTime))
			{
				return true;
			}

			if (type == typeof(string))
			{
				return true;
			}

			if (type == typeof(Guid))
			{
				return true;
			}

			if (typeof(IList<>).IsAssignableFromIgnoreGenericParameters(type))
			{
				return true;
			}

			if (typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(type))
			{
				return true;
			}

			return false;
		}

		private IEnumerable<PropertyInfo> GetApplicableProperties()
		{
			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				var attribute = propertyInfo.GetFirstCustomAttribute<TextFieldAttribute>(true);

				if (attribute == null)
				{
					continue;
				}

				if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
				{
					continue;
				}

				yield return propertyInfo;
			}
		}

		private void BuildSerializerFunction()
		{
			var dynamicMethod = new DynamicMethod("Serialize", typeof(void), new[] { typeof(T), typeof(TextWriter) }, true);

			var generator = dynamicMethod.GetILGenerator();

			var retLabel = generator.DefineLabel();
			var target = generator.DeclareLocal(typeof(T));

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Stloc, target);

			EmitSkipNullValue(generator, target, retLabel);

			if (IsSimplePropertyType((typeof(T))))
			{
				var local = generator.DeclareLocal(typeof(T));

				generator.Emit(OpCodes.Ldarg_0);
				generator.Emit(OpCodes.Stloc, local);

				WriteSimpleType(generator, local, retLabel, typeof(T));
			}
			else
			{
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldc_I4, (int)'[');
				generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWriteChar);

				int x = 0;
				var applicableProperties = GetApplicableProperties().ToList();

				var writtenOneProperty = generator.DeclareLocal(typeof(bool));

				generator.Emit(OpCodes.Ldc_I4_0);
				generator.Emit(OpCodes.Stloc, writtenOneProperty);

				foreach (var propertyInfo in applicableProperties)
				{
					var attribute = propertyInfo.GetFirstCustomAttribute<TextFieldAttribute>(true);

					var nosemicolonLabel = generator.DefineLabel();
					var nextPropertyLabel = generator.DefineLabel();
					var nextPropertySkipMarkerLabel = generator.DefineLabel();

					var local = generator.DeclareLocal(propertyInfo.PropertyType);

					if (typeof(T).IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
						generator.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
						generator.Emit(OpCodes.Stloc, local);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Callvirt, propertyInfo.GetGetMethod());
						generator.Emit(OpCodes.Stloc, local);
					}

					EmitSkipNullValue(generator, local, nextPropertySkipMarkerLabel);

					// Write the semicolon
					generator.Emit(OpCodes.Ldloc, writtenOneProperty);
					generator.Emit(OpCodes.Brfalse, nosemicolonLabel);
					generator.Emit(OpCodes.Ldarg_1);
					generator.Emit(OpCodes.Ldc_I4, (int)';');
					generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWriteChar);

					generator.MarkLabel(nosemicolonLabel);

					// Write the property name
					generator.Emit(OpCodes.Ldarg_1);
					generator.Emit(OpCodes.Ldstr, propertyInfo.Name + "=");
					generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWrite);

					if (IsSimplePropertyType(propertyInfo.PropertyType))
					{
						WriteSimpleType(generator, local, nextPropertyLabel, propertyInfo);
					}
					else
					{
						var getSerializerMethod = TextSerializer.GetSerializerMethod.MakeGenericMethod(propertyInfo.PropertyType);
						var serializeMethod = getSerializerMethod.ReturnType.GetMethod("Serialize", BindingFlags.Instance | BindingFlags.Public, null, new[] { propertyInfo.PropertyType, typeof(TextWriter) }, null);

						generator.Emit(OpCodes.Call, getSerializerMethod);
						generator.Emit(OpCodes.Ldloc, local);
						generator.Emit(OpCodes.Ldarg_1);
						generator.Emit(OpCodes.Callvirt, serializeMethod);
					}

					generator.Emit(OpCodes.Ldc_I4_1);
					generator.Emit(OpCodes.Stloc, writtenOneProperty);

					generator.MarkLabel(nextPropertyLabel);
					generator.MarkLabel(nextPropertySkipMarkerLabel);

					x++;
				}

				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldc_I4, ']');
				generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWriteChar);
			}

			generator.MarkLabel(retLabel);
			generator.Emit(OpCodes.Ret);
			serializerFunction = (Action<T, TextWriter>)dynamicMethod.CreateDelegate(typeof(Action<T, TextWriter>));
		}

		private void EmitSkipNullValue(ILGenerator generator, LocalBuilder local, Label skipLabel)
		{
			var underlyingType = local.LocalType.IsValueType ? Nullable.GetUnderlyingType(local.LocalType) : null;

			if (underlyingType != null)
			{
				generator.Emit(OpCodes.Ldloca, local);
				generator.Emit(OpCodes.Call, local.LocalType.GetProperty("HasValue").GetGetMethod());
				generator.Emit(OpCodes.Brfalse, skipLabel);
			}
			else if (local.LocalType.IsValueType)
			{
				var defaultValue = generator.DeclareLocal(local.LocalType);

				generator.Emit(OpCodes.Ldloca, defaultValue);
				generator.Emit(OpCodes.Initobj, local.LocalType);

				var methodInfo = local.LocalType.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public, null, new[] { local.LocalType }, null);

				if (methodInfo != null)
				{
					generator.Emit(OpCodes.Ldloca, local);
					generator.Emit(OpCodes.Ldloc, defaultValue);

					if (!methodInfo.GetParameters()[0].ParameterType.IsValueType)
					{
						// Object.Equals(Object)
						generator.Emit(OpCodes.Box, local.LocalType);
						generator.Emit(OpCodes.Constrained, local.LocalType);
						generator.Emit(OpCodes.Callvirt, methodInfo);
					}
					else
					{
						// T.Equals(T)
						generator.Emit(OpCodes.Call, methodInfo);
					}

					generator.Emit(OpCodes.Brtrue, skipLabel);
				}
			}
			else
			{
				// Check for null object
				generator.Emit(OpCodes.Ldloc, local);
				generator.Emit(OpCodes.Brfalse, skipLabel);
			}
		}

		private Func<string, T> deserializeFunction;

		private void BuildObjectParser()
		{
			Expression body;

			if (IsSimplePropertyType(typeof(T)))
			{
				var textParameter = Expression.Parameter(typeof(string), "text");

				body = BuildPropertyParser(typeof(T), textParameter, typeof(T));

				//deserializeFunction = (Func<string, T>)Expression.Lambda(body, textParameter).Compile();
				deserializeFunction = (Func<string, T>)ExtendedLambdaExpressionCompiler.Compile(Expression.Lambda(body, textParameter));

				return;
			}

			foreach (var propertyInfo in this.GetApplicableProperties())
			{
				var bindings = new List<MemberBinding>();
				var source = Expression.Parameter(typeof(T), "source");
				var textParameter = Expression.Parameter(typeof(string), "text");
				var propertyValue = BuildPropertyParser(propertyInfo.PropertyType, textParameter, propertyInfo);

				var binding = Expression.Bind(propertyInfo, propertyValue);

				bindings.Add(binding);

				var populateExpression = new MemberPopulateExpression(typeof(T), source, bindings);

				deserializerSetters[propertyInfo.Name] = (Func<T, string, T>)ExtendedLambdaExpressionCompiler.Compile(Expression.Lambda<Func<T, string, T>>(populateExpression, source, textParameter));

				propertyTypes[propertyInfo.Name] = propertyInfo.PropertyType;
			}
		}

		private Expression BuildPropertyParser(Type type, ParameterExpression text, MemberInfo memberInfo)
		{
			Expression body;
			var originalType = type;
			var underlyingType = Nullable.GetUnderlyingType(type);

			if (underlyingType != null)
			{
				type = underlyingType;
			}

			if (type.IsPrimitive)
			{
				var methodInfo = typeof(Convert).GetMethod("To" + type.Name, new[] { typeof(string) });

				if (methodInfo != null)
				{
					body = Expression.Call(null, methodInfo, text);
				}
				else
				{
					throw new NotSupportedException(type.ToString());
				}
			}
			else if (type.IsEnum)
			{
				var parseMethod = typeof(Enum).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(string) }, null);
				body = Expression.Convert(Expression.Call(Expression.Constant(null), parseMethod, Expression.Constant(type), text), type);
			}
			else if (type == typeof(DateTime))
			{
				var methodInfo = typeof(DateTime).GetMethod("ParseExact", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(string), typeof(IFormatProvider) }, null);

				body = Expression.Call(null, methodInfo, text, Expression.Constant(TextSerializer.DateTimeFormat), Expression.Constant(null, typeof(IFormatProvider)));
			}
			else if (type == typeof(Guid))
			{
				body = Expression.New(typeof(Guid).GetConstructor(new[] { typeof(string) }), text);
			}
			else if (type.IsArray)
			{
				body = Expression.Call(Expression.Constant(null), TextSerializer.ReadArrayMethod.MakeGenericMethod(type.GetElementType()), text);
			}
			else if (typeof(IList<>).IsAssignableFromIgnoreGenericParameters(type))
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>))
				{
					var argType = type.GetGenericArguments()[0];

					type = typeof(List<>).MakeGenericType(argType);
				}

				body = Expression.Call(Expression.Constant(null), TextSerializer.ReadListMethod.MakeGenericMethod(type, type.GetSequenceElementType()), text);
			}
			else if (typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(type))
			{
				var idictionaryi = type.GetInterfaces().FirstOrDefault(c => typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(c));

				body = Expression.Call(Expression.Constant(null), TextSerializer.ReadDictionaryMethod.MakeGenericMethod(type, idictionaryi.GetGenericArguments()[0], idictionaryi.GetGenericArguments()[1]), text);
			}
			else
			{
				if (type == typeof(string))
				{
					var attribute = memberInfo.GetFirstCustomAttribute<TextLiteralAttribute>(true);

					body = Expression.Call(null, TextSerializer.ReadStringMethod, text);
				}
				else
				{
					body = Expression.Call(null, TextSerializer.DeserializeFromStringMethod.MakeGenericMethod(type), text);
				}
			}

			Expression ifTrue;
			var test = Expression.Equal(Expression.Constant(""), text);

			ifTrue = Expression.Constant(originalType.GetDefaultValue(), originalType);
			body = Expression.Condition(test, ifTrue, originalType != type ? Expression.Convert(body, originalType) : body);

			return body;
		}

		private void WriteSimpleType(ILGenerator generator, LocalBuilder local, Label nextPropertyLabel, MemberInfo memberInfo)
		{
			var unwrappedLocal = local;
			var underlyingType = Nullable.GetUnderlyingType(local.LocalType);

			if (underlyingType != null)
			{
				generator.DeclareLocal(underlyingType);
			}

			if (underlyingType != null)
			{
				var label2 = generator.DefineLabel();

				unwrappedLocal = generator.DeclareLocal(underlyingType);

				generator.Emit(OpCodes.Ldloca, local);
				generator.Emit(OpCodes.Call, local.LocalType.GetProperty("HasValue").GetGetMethod());
				generator.Emit(OpCodes.Brtrue, label2);

				// Write "null"
				generator.Emit(OpCodes.Ldnull);
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Call, TextSerializer.WriteValueMethod);
				generator.Emit(OpCodes.Br, nextPropertyLabel);

				generator.MarkLabel(label2);

				generator.Emit(OpCodes.Ldloca, local);
				generator.Emit(OpCodes.Call, local.LocalType.GetProperty("Value").GetGetMethod());
				generator.Emit(OpCodes.Stloc, unwrappedLocal);
			}

			var writeMethod = typeof(TextWriter).GetMethod("Write", BindingFlags.Instance | BindingFlags.Public, null, new[] { unwrappedLocal.LocalType }, null);

			if (unwrappedLocal.LocalType.IsEnum)
			{
				generator.Emit(OpCodes.Ldarg_1);

				generator.Emit(OpCodes.Ldloca, unwrappedLocal);
				generator.Emit(OpCodes.Constrained, unwrappedLocal.LocalType);
				generator.Emit(OpCodes.Callvirt, typeof(Object).GetMethod("ToString"));

				generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWrite);
			}
			else if (unwrappedLocal.LocalType == typeof(DateTime))
			{
				/*
				generator.Emit(OpCodes.Ldarg_1);

				generator.Emit(OpCodes.Ldloca, unwrappedLocal);
				generator.Emit(OpCodes.Ldstr, TextSerializer.DateTimeFormat);
				generator.Emit(OpCodes.Call, typeof(DateTime).GetMethod("ToString", new [] { typeof(string) } ));
				
				generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWrite);
				*/

				generator.Emit(OpCodes.Ldloc, unwrappedLocal);
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Call, TextSerializer.WriteDateTimeValueMethod);
			}
			else if (unwrappedLocal.LocalType == typeof(Guid))
			{
				generator.Emit(OpCodes.Ldarg_1);

				generator.Emit(OpCodes.Ldloca, unwrappedLocal);
				generator.Emit(OpCodes.Ldstr, TextSerializer.GuidFormat);
				generator.Emit(OpCodes.Call, typeof(Guid).GetMethod("ToString", new[] { typeof(string) }));

				generator.Emit(OpCodes.Callvirt, TextSerializer.TextWriterWrite);
			}
			else if (typeof(IList<>).IsAssignableFromIgnoreGenericParameters(unwrappedLocal.LocalType))
			{
				generator.Emit(OpCodes.Ldloc, unwrappedLocal);
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Call, TextSerializer.SerializeListToWriterMethod.MakeGenericMethod(unwrappedLocal.LocalType.GetSequenceElementType()));
			}
			else if (typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(unwrappedLocal.LocalType))
			{
				generator.Emit(OpCodes.Ldloc, unwrappedLocal);
				generator.Emit(OpCodes.Ldarg_1);
				var idictionaryi = unwrappedLocal.LocalType.GetInterfaces().FirstOrDefault(c => typeof(IDictionary<,>).IsAssignableFromIgnoreGenericParameters(c));
				generator.Emit(OpCodes.Call, TextSerializer.SerializeDictionaryToWriterMethod.MakeGenericMethod(idictionaryi.GetGenericArguments()[0], idictionaryi.GetGenericArguments()[1]));
			}
			else if (unwrappedLocal.LocalType == typeof(string))
			{
				generator.Emit(OpCodes.Ldloc, unwrappedLocal);
				generator.Emit(OpCodes.Ldarg_1);

				var attrib = memberInfo.GetFirstCustomAttribute<TextLiteralAttribute>(true);

				if (attrib == null || !attrib.IsLiteral || (memberInfo.MemberType == MemberTypes.Property || memberInfo.MemberType == MemberTypes.Field))
				{
					//generator.Emit(OpCodes.Call, TextSerializer.WriteStringValueMethod);
					generator.Emit(OpCodes.Call, TextSerializer.WriteLiteralStringValueMethod);
				}
				else
				{
					generator.Emit(OpCodes.Call, TextSerializer.WriteLiteralStringValueMethod);
				}
			}
			else if (writeMethod != null)
			{
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldloc, unwrappedLocal);
				generator.Emit(OpCodes.Callvirt, writeMethod);
			}
			else
			{
				generator.Emit(OpCodes.Ldloc, unwrappedLocal);

				if (unwrappedLocal.LocalType.IsValueType)
				{
					generator.Emit(OpCodes.Box, unwrappedLocal.LocalType);
				}

				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Call, TextSerializer.WriteValueMethod);
			}
		}

		public virtual string Serialize(T value)
		{
			var writer = new StringWriter();

			Serialize(value, writer);

			return writer.ToString();
		}

		public virtual void Serialize(T value, TextWriter writer)
		{
			serializerFunction(value, writer);
		}

		public virtual T Deserialize(string stringValue)
		{
			if (deserializeFunction != null)
			{
				return deserializeFunction(stringValue);
			}

			var retval = ActivatorUtils<T>.CreateInstance();

			return PopulateExisting(retval, stringValue);
		}

		public virtual T PopulateExisting(T retval, TextReader reader)
		{
			return PopulateExisting(retval, reader.ReadToEnd());
		}

		public virtual T PopulateExisting(T retval, string stringValue)
		{
			int len = stringValue.Length;

			if (stringValue[0] != '[')
			{
				throw new InvalidOperationException("Expected '['");
			}

			if (stringValue[len - 1] != ']')
			{
				throw new InvalidOperationException("Expected ']'");
			}

			for (var i = 1; i < len - 1; i++)
			{
				var keyOffset = TextSerializer.GetNextOffset(stringValue, i, '=');
				var key = stringValue.Substring(i, keyOffset - i);
				var keyLength = key.Length;
				var valueOffset = TextSerializer.GetNextOffset(stringValue, i + keyLength, ';');
				var value = stringValue.Substring(i + keyLength + 1, valueOffset - (i + keyLength + 1));

				i = valueOffset;

				var setter = deserializerSetters[key];

				retval = setter(retval, value);
			}

			return retval;
		}

		public virtual T Deserialize(TextReader reader)
		{
			return Deserialize(reader.ReadToEnd());
		}
	}
}
