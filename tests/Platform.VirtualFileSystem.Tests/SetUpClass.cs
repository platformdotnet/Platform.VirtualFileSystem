using System;
using System.IO;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[SetUpFixture]
	public class SetUpClass
	{
		[SetUp]
		public void SetUp()
		{
			Environment.SetEnvironmentVariable("TEMP", Path.Combine(Environment.CurrentDirectory, "TestFiles", "Temp"));

			var fileSystem = FileSystemManager.GetManager().ResolveDirectory(Environment.CurrentDirectory).ResolveDirectory("TestFiles").CreateView("testfiles");

			FileSystemManager.Default.AddFileSystem(fileSystem);
		}
	}
}
