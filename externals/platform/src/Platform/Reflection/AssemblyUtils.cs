using System;
using System.Reflection;

namespace Platform.Reflection
{
	/// <summary>
	/// Provides extension methods and static utility methods for Assembky and derived classes.
	/// </summary>
	public static class AssemblyUtils
	{
		/// <summary>
		/// Gets and returns the first custom attribute of the given type.
		/// </summary>
		/// <typeparam name="T">The type of attribute to get an return</typeparam>
		/// <param name="assembly"></param>
		/// <param name="inherit"></param>
		/// <returns>The custom attribute or null if one was not found</returns>
		public static T GetFirstCustomAttribute<T>(this Assembly assembly, bool inherit)
			where T : Attribute
		{
		    var values = assembly.GetCustomAttributes(typeof(T), true);

		    return values.Length > 0 ? (T)values[0] : null;
		}
	}
}
