using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and utility methods for object classes
	/// </summary>
	public static class ObjectUtils
	{
		/// <summary>
		/// A function that takes a value and converts it to a string.  This is a useful method to pass to
		/// methods that expect a delegate of the form: T => String
		/// </summary>
		/// <typeparam name="T">The type of the object to convert</typeparam>
		/// <param name="value">The object to convert</param>
		/// <returns>A string representation of <see cref="value"/></returns>
		public static string ToString<T>(T value)
		{
			return Convert.ToString(value);
		}
/*
		public static T CastTo<T>(this object value)
			where T : class
		{
			return (T)value;
		}

		public static T CastAs<T>(this object value)
			where T : class
		{
			return value as T;
		}*/

		/*public static T Apply<T>(this T objectValue, Expression<Func<T>> action)
			where T : new()
		{
			var memberInitExpression = action.Body as MemberInitExpression;

			if (memberInitExpression == null)
			{
				throw new InvalidOperationException("Lambda must be a member initialisation");
			}

			foreach (var binding in memberInitExpression.Bindings)
			{
				switch (binding.BindingType)
				{
					case MemberBindingType.Assignment:
						break;
					case MemberBindingType.ListBinding:
						break;
					case MemberBindingType.MemberBinding:
						break;
					default:
						break;
				}
			}

			return new T();
		}*/
	}
}
