using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform
{
	public static class TypeUtils
	{
		public static readonly Type ListType = typeof(List<>);
		public static readonly Type IListType = typeof(IList<>);
		public static readonly Type IEnumerableType = typeof(IEnumerable<>);
		public static readonly Type IQueryableType = typeof(IQueryable<>);

		public static Type GetSequenceType(this Type elementType)
		{
			return typeof(IEnumerable<>).MakeGenericType(elementType);
		}

		public static Type GetSequenceElementType(this Type sequenceType)
		{
			var retval = FindSequenceElementType(sequenceType);

			return retval ?? sequenceType;
		}

		private static Type FindSequenceElementType(this Type sequenceType)
		{
			if (sequenceType == null || sequenceType == typeof(string))
			{
				return null;
			}

			if (sequenceType.IsArray)
			{
				return sequenceType.GetElementType();
			}

			if (sequenceType.IsGenericType)
			{
				var genericType = sequenceType.GetGenericTypeDefinition();

				if (genericType == ListType || genericType == IListType)
				{
					return sequenceType.GetGenericArguments()[0];
				}

				foreach (var genericArgument in sequenceType.GetGenericArguments())
				{
					var iEnumerable = typeof(IEnumerable<>).MakeGenericType(genericArgument);

					if (iEnumerable.IsAssignableFrom(sequenceType))
					{
						return genericArgument;
					}
				}
			}

			var interfaces = sequenceType.GetInterfaces();

			if (interfaces != null && interfaces.Length > 0)
			{
				foreach (var interfaceType in interfaces)
				{
					var element = FindSequenceElementType(interfaceType);

					if (element != null)
					{
						return element;
					}
				}
			}

			if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
			{
				return FindSequenceElementType(sequenceType.BaseType);
			}

			return null;
		}

		public static IEnumerable<Type> WalkHierarchy(this Type type, bool includeInterfaces, bool convertGenericsToGenericTypeDefinition)
		{
			var currentType = type;

			if (convertGenericsToGenericTypeDefinition)
			{
				while (currentType != null)
				{
					if (currentType.IsGenericType)
					{
						yield return currentType.GetGenericTypeDefinition();
					}

					currentType = currentType.BaseType;
				}
			}
			else
			{
				while (currentType != null)
				{
					yield return currentType;

					currentType = currentType.BaseType;
				}
			}

			if (includeInterfaces)
			{
				if (convertGenericsToGenericTypeDefinition)
				{
					foreach (var interfaceType in type.GetInterfaces())
					{
						if (interfaceType.IsGenericType)
						{
							yield return interfaceType.GetGenericTypeDefinition();
						}
					}
				}
				else
				{
					foreach (var interfaceType in type.GetInterfaces())
					{
						yield return interfaceType;
					}
				}
			}
		}

		/// <summary>
		/// Returns true if the type is a numeric type (by default does not
		/// return true if it is a nullable numeric type).
		/// </summary>
		public static bool IsNumericType(this Type type)
		{
			return IsIntegerType(type) && IsRealType(type);
		}

		/// <summary>
		/// Returns true if the type is a real type (by default does not
		/// return true if it is a nullable real type).
		/// </summary>
		public static bool IsRealType(this Type type)
		{
			return IsRealType(type, false);
		}

		public static bool IsRealType(this Type type, bool checkNullable)
		{
			Type underlyingType;

			if (checkNullable && (underlyingType = Nullable.GetUnderlyingType(type)) != null)
			{
				type = underlyingType;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Single:
				case TypeCode.Double:
				case TypeCode.Decimal:
					return true;
			}

			return false;
		}

		public static bool IsIntegerType(this Type type)
		{
			return IsIntegerType(type, false);
		}

		public static bool IsIntegerType(this Type type, bool checkNullable)
		{
			Type underlyingType;

			if (checkNullable && (underlyingType = Nullable.GetUnderlyingType(type)) != null)
			{
				type = underlyingType;
			}

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.SByte:
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.Int16:
				case TypeCode.UInt32:
				case TypeCode.Int32:
				case TypeCode.UInt64:
				case TypeCode.Int64:
					return true;
			}

			return false;
		}
		
		public static object GetDefaultValue(this Type type)
		{
			return GetDefaultValue(type, true);
		}

		public static object GetDefaultValue(this Type type, bool nullablesAreNull)
		{
			if (type.IsValueType)
			{
				var underlying = Nullable.GetUnderlyingType(type);

				if (underlying != null)
				{
					if (nullablesAreNull)
					{
						return null;
					}
					else
					{
						type = underlying;
					}
				}

				if (type.IsEnum)
				{
					return Enum.ToObject(type, Activator.CreateInstance(type));
				}

				switch (Type.GetTypeCode(type))
				{
					case TypeCode.Int16:
						return (short)0;
					case TypeCode.Int32:
						return 0;
					case TypeCode.Int64:
						return (long)0;
					case TypeCode.UInt16:
						return (ushort)0;
					case TypeCode.UInt32:
						return (uint)0;
					case TypeCode.UInt64:
						return (ulong)0;
					default:
						return Activator.CreateInstance(type);
				}
			}
			else
			{
				return null;
			}
		}

		public static bool IsAssignableFromIgnoreGenericParameters(this Type type, Type compareToType)
		{
			if (type.IsGenericType)
			{
				type = type.GetGenericTypeDefinition();
			}

			foreach (var walkedType in compareToType.WalkHierarchy(type.IsInterface, true))
			{
				if (walkedType == type || type.IsAssignableFrom(compareToType))
				{
					return true;
				}
			}

			return false;
		}
	}
}
