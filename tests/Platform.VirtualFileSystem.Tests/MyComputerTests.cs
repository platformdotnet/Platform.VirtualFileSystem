using System;
using System.Linq;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class MyComputerTests
		: TestsBase
	{
		[Test]
		public void Test_My_Computer()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				return;
			}

			var dirs = FileSystemManager.Default.ResolveDirectory("mycomputer:///");

			Assert.Greater(dirs.GetChildren().Count(), 0);

			foreach (var child in dirs.GetChildren())
			{
				Console.WriteLine(child);
			}
		}
	}
}
