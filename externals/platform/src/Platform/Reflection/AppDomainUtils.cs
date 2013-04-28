using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Platform.Reflection
{
	/// <summary>
	/// Provides extension methods and static utility methods for AppDomains.
	/// </summary>
	public static class AppDomainUtils
	{
		/// <summary>
		/// Returns an enumeration of all loaded types in the given AppDomain
		/// </summary>
		/// <param name="appDomain">The AppDomain</param>
		/// <returns>
		/// An enumeration of types
		/// </returns>
		public static IEnumerable<Type> GetAllLoadedTypes(this AppDomain appDomain)
		{
			return appDomain.GetAllLoadedTypes(".*");
		}

		/// <summary>
		/// Returns an enumeration of all loaded types in the given AppDomain whose
		/// assembly full name matches the given regex
		/// </summary>
		/// <param name="appDomain">The AppDomain</param>
		/// <param name="assemblyRegex">The regular expression to match full assembly names</param>
		/// <returns>
		/// An enumeration of types
		/// </returns>
		/// <seealso cref="Assembly.FullName"/>
		public static IEnumerable<Type> GetAllLoadedTypes(this AppDomain appDomain, string assemblyRegex)
		{
		    var regex = new Regex(assemblyRegex, RegexOptions.IgnoreCase);

			Predicate<Assembly> predicate = (assembly => assembly.FullName == null ? false : regex.IsMatch(assembly.FullName));

			return appDomain.GetAllLoadedTypes(predicate, PredicateUtils<Type>.AlwaysTrue);
		}

		/// <summary>
		/// Returns an enumeration of all loaded types in the given AppDomain whose
		/// assembly matches the given predicate
		/// </summary>
		/// <param name="appDomain">The AppDomain</param>
		/// <param name="acceptAssembly">The predicate that will match the given assembly</param>
		/// <returns>
		/// An enumeration of types
		/// </returns>
		public static IEnumerable<Type> GetAllLoadedTypes(this AppDomain appDomain, Predicate<Assembly> acceptAssembly)
		{
			return appDomain.GetAllLoadedTypes(acceptAssembly, PredicateUtils<Type>.AlwaysTrue);
		}

		/// <summary>
		/// Returns an enumeration of all loaded types in the given AppDomain which match the given predicates
		/// </summary>
		/// <param name="appDomain">The AppDomain</param>
		/// <param name="acceptAssembly">A predicate that will validate if the type assembly is ok</param>
		/// <param name="acceptType">A predicate that will validate if the type is ok</param>
		/// <returns> An enumeration of types</returns>
		public static IEnumerable<Type> GetAllLoadedTypes(this AppDomain appDomain, Predicate<Assembly> acceptAssembly, Predicate<Type> acceptType)
		{
			foreach (Assembly assembly in appDomain.GetAssemblies())
			{
				if (!acceptAssembly(assembly))
				{
					continue;
				}

				Type[] types;
								
				try
				{
					types = assembly.GetExportedTypes();
				}
				catch (ReflectionTypeLoadException)
				{
					continue;
				}
				catch (TypeInitializationException)
				{
					continue;
				}

				foreach (Type type in types)
				{
					if (acceptType(type))
					{
						yield return type;
					}
				}
			}
		}
	}
}
