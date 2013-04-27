using System;
using System.Reflection;

namespace Platform.VirtualFileSystem
{
	public class FileSystemManager
		: MarshalByRefObject
	{
		public const char SeperatorChar = '/';
		public const string SeperatorString = "/";
		public const char RootChar = '/';
		public const string RootPath = "/";

		private static readonly IFileSystemManager manager;

		static FileSystemManager()
		{
			try
			{
				manager = (IFileSystemManager)Activator.CreateInstance(Type.GetType(ConfigurationSection.Default.DefaultManager));
			}
			catch (TargetInvocationException e)
			{
				Console.Error.WriteLine("There was an error initializing the {0}", typeof(FileSystemManager).Name);
				Console.Error.WriteLine(e.InnerException);
			}
		}

		public static IFileSystemManager Default
		{
			get
			{
				return GetManager();
			}
		}

		public static IFileSystemManager GetManager()
		{
			if (manager == null)
			{
				throw new InvalidOperationException("No default manager");
			}

			return manager;
		}
	}
}