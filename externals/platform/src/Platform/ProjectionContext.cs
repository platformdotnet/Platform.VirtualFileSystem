using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Platform
{
	public class ProjectionContext
	{
		public static readonly ProjectionContext Default = new ProjectionContext();
		public static readonly MethodInfo ConvertToArrayMethod = typeof(ProjectionContext).GetMethod("ConvertToArray");
		public static readonly MethodInfo ConvertToListMethod = typeof(ProjectionContext).GetMethod("ConvertToList");
		public static readonly MethodInfo ConvertToEnumerationMethod = typeof(ProjectionContext).GetMethod("ConvertToEnumeration");
		public static readonly MethodInfo ConvertValueMethod1 = typeof(ProjectionContext).GetMethods().First(c => c.Name == "ConvertValue" && c.GetParameters().Length == 1);
		public static readonly MethodInfo ConvertValueMethod2 = typeof(ProjectionContext).GetMethods().First(c => c.Name == "ConvertValue" && c.GetParameters().Length == 2);
        
		protected class ProjectIntoExistingCache<T, U>
		{
			private static volatile Action<ProjectionContext, T, U> cachedFunction;

			public static Action<ProjectionContext, T, U> GetCachedFunction()
			{
				Action<ProjectionContext, T, U> retval;

				if (cachedFunction == null)
				{
					retval = ObjectToObjectProjector<T, U>.Default.BuildProjectIntoExisting();

					cachedFunction = retval;
				}
				else
				{
					retval = cachedFunction;
				}

				return retval;
			}
		}

		public static void ProjectInto<T, U>(ProjectionContext projectionContext, T target, U source)
		{
			var action = ProjectIntoExistingCache<U, T>.GetCachedFunction();

			action(projectionContext, source, target);
		}

		public virtual T CreateInstance<T>(object context)
		{
			return Activator.CreateInstance<T>();
		}

		public virtual U ConvertValue<T, U>(T value)
		{
			return ConvertValue<T, U>(null, value);
		}
        
		public virtual U ConvertValue<T, U>(object context, T value)
		{
			Type typeofT = typeof(T), typeofU = typeof(U);

			if (value == null || (value != null && value.Equals(typeofU.GetDefaultValue())))
			{
				return default(U);
			}

			if (typeofT == typeofU)
			{
				return (U)(object)value;
			}
			else if (typeofT.IsPrimitive && typeofU.IsPrimitive)
			{
				return (U)Convert.ChangeType(value, typeofU);
			}
			else if (typeofT == typeof(string) && typeofU.IsEnum)
			{
				return (U)Enum.Parse(typeofU, value.ToString());
			}
			else if (typeofT.IsEnum && typeofU == typeof(string))
			{
				return (U)(object)value.ToString();
			}
			else if (typeofT == typeof(Guid) && typeofU == typeof(string))
			{
				return (U)(object)value.ToString();
			}
			else if (typeofT == typeof(string) && typeofU == typeof(Guid))
			{
				if (String.IsNullOrEmpty((string)(object)value))
				{
					return (U)(object)new Guid();
				}

				return (U)(object)new Guid((string)(object)value);
			}
			else
			{
				var newInstance = CreateInstance<U>(context);

				ProjectInto<U, T>(this, newInstance, value);

				return newInstance;
			}
		}

		public virtual List<U> ConvertToList<T, U>(IEnumerable<T> enumerable)
		{
			var retval = new List<U>();

			foreach (var value in enumerable)
			{
				retval.Add(ConvertValue<T, U>(enumerable, value));
			}

			return retval;
		}

		public virtual IEnumerable<U> ConvertToEnumeration<T, U>(IEnumerable<T> enumerable)
		{
			foreach (var value in enumerable)
			{
				yield return ConvertValue<T, U>(enumerable, value);
			}
		}

		public virtual U[] ConvertToArray<T, U>(IEnumerable<T> enumerable)
		{
			var collection = enumerable as ICollection;

			if (collection != null)
			{
				var i = 0;
				var retval = new U[collection.Count];

				foreach (var value in enumerable)
				{
					retval[i++] = ConvertValue<T, U>(enumerable, value);
				}

				return retval;
			}
			else
			{
				return ConvertToEnumeration<T, U>(enumerable).ToArray();
			}
		}
	}
}
