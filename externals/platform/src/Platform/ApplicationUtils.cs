using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Platform
{
	/// <summary>
	/// Provides useful utility methods for applications
	/// </summary>
	public static class ApplicationUtils
	{
		/// <summary>
		/// Loads all DLLs in the <see cref="ApplicationPath"/>
		/// </summary>
		/// <param name="regexText">A regex that matches the names of the assemblys</param>
		public static void LoadPlugins(string regexText)
		{
			LoadPlugins("", new Regex(regexText, RegexOptions.IgnoreCase));
		}

		/// <summary>
		/// Loads all DLLs in the <see cref="ApplicationPath"/>
		/// </summary>
		/// <param name="subDir">
		/// The sub directory where the plugins are stored (relative to the <see cref="ApplicationPath"/>).
		/// Can be an empty string.
		/// </param>
		/// <param name="assemblyNameRegex">A regex that matches the names of the assemblys</param>
		public static void LoadPlugins(string subDir, Regex assemblyNameRegex)
		{
			foreach (string fileName in Directory.GetFiles(Path.Combine(ApplicationUtils.ApplicationPath, subDir), "*.dll"))
			{
				try
				{
				    var name = Path.GetFileName(fileName);
					var x = name.LastIndexOf('.');

					if (x > 0)
					{
						name = name.Substring(0, x);
					}

					if (assemblyNameRegex.IsMatch(name))
					{
						Assembly.Load(name);
					}
				}
				catch (TypeLoadException)
				{
				}
			}
		}

		/// <summary>
		/// Gets the path that the current application resides in
		/// </summary>
		public static string ApplicationPath
		{
			get
			{
			    // GetEntryAssembly may return null if the application is started
				// from native code (by TestDrive.NET for example)

				var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

				var uriCodebase = new Uri(assembly.CodeBase);

				return Path.GetDirectoryName(uriCodebase.LocalPath);	
			}
		}
	}
}
