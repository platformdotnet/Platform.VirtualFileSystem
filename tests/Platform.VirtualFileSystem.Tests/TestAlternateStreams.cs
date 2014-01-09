using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	[Category("RequiresAlternateStreams")]
	public class TestAlternateStreams
		: TestsBase
	{
		[Test]
		public void Test_Add_AlternateStream()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				return;
			}

			var file = FileSystemManager.Default.ResolveFile("temp:///Temp.txt");

			Assert.AreEqual(1, file.GetContentNames().Count());

			using (var writer = file.GetContent("AlternateStream").GetWriter())
			{
				writer.Write("AlternateData");
			}

			Assert.AreEqual(2, file.GetContentNames().Count());

			Assert.IsTrue(file.GetContentNames().Contains("AlternateStream"));

			using (var reader = file.GetContent("AlternateStream").GetReader())
			{
				Assert.AreEqual("AlternateData", reader.ReadToEnd());
			}
		}
	}
}
