using System;
using Platform.VirtualFileSystem.Providers.Web;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// A default implementation of a FileSystemManager.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This manager only uses the <c>Web</c> provider to resolve files.  You should extend this
	/// class and call <see cref="DefaultFileSystemManager.AddProvider(INodeProvider)"/>
	/// to add more providers.
	/// </p>
	/// <p>
	/// Usually you would use the <see cref="StandardFileSystemManager"/>.  It uses all the standard
	/// providers as well as providers configured in the application <c>config</c> file.  The 
	/// <see cref="StandardFileSystemManager"/> is based on this provider so will still rely on the 
	/// <see cref="WebNodeProvider"/> when no other providers can resolve a file.
	/// </p>
	/// </remarks>
	public class DefaultFileSystemManager
		: StandardFileSystemManager
	{
		public DefaultFileSystemManager()
		{
			if (ConfigurationSection.Default.DefaultNodeProviders.Length == 0)
			{
				AddDefaultProviders();
			}
			else
			{
				foreach (var entry in ConfigurationSection.Default.DefaultNodeProviders)
				{
					Exception e;

					if ((e = ActionUtils.IgnoreExceptions(() => AddProvider(CreateProvider(entry)))) != null)
					{
						Console.Error.WriteLine("Error Creating Node Provider: " + entry.Type);

						Console.Error.WriteLine(e);

						throw e;
					}
				}
			}

			this.ScanAssembliesAndAddProviders();
		}

		private void AddDefaultProviders()
		{
			AddProvider(new Providers.Local.LocalNodeProvider(this));
			AddProvider(new Providers.Temp.TempNodeProvider(this));
			AddProvider(new Providers.Web.WebNodeProvider(this));
			AddProvider(new Providers.Shadow.ShadowNodeProvider(this));
			AddProvider(new Providers.MyComputer.MyComputerNodeProvider(this));
		}
	}
}
