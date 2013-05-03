using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Platform.IO;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class WebFileSystemTests
		: TestsBase
	{
		[Test]
		public void Test_Grab_Google_Homepage()
		{
			var file = FileSystemManager.Default.ResolveFile("http://www.google.com");

			var googleHomePage =  file.GetContent().GetReader().ReadToEndThenClose();

			Assert.IsTrue(googleHomePage.Contains("html"));
		}

		[Test]
		public void Test_Grab_Github_Homepage()
		{
			var dir1 = FileSystemManager.Default.ResolveDirectory("https://github.com/platformdotnet/Platform.VirtualFileSystem");

			var rootDir = dir1.FileSystem.RootDirectory;
			var rootDirAltMethod = dir1.ResolveDirectory("../..");

			Assert.AreSame(rootDir, rootDirAltMethod);

			var rootPage = rootDir.GetContent().GetReader().ReadToEndThenClose();

			Assert.IsTrue(rootPage.Contains("github"));

			var rootPageAsFile = rootDir.ResolveFile(".").GetContent().GetReader().ReadToEndThenClose();

			Assert.IsTrue(rootPageAsFile.Contains("github"));
		}
	}
}
