using System;
using System.Linq;
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

			var file = FileSystemManager.Default.ResolveFile($"temp:///{Guid.NewGuid().ToString("N")}.txt");

			var x = file.GetContentNames().Count();

			Assert.GreaterOrEqual(x, 1);

			using (var writer = file.GetContent("AlternateStream").GetWriter())
			{
				writer.Write("AlternateData");
			}

			Assert.AreEqual(x + 1, file.GetContentNames().Count());

			Assert.IsTrue(file.GetContentNames().Contains("AlternateStream"));

			using (var reader = file.GetContent("AlternateStream").GetReader())
			{
				Assert.AreEqual("AlternateData", reader.ReadToEnd());
			}

			file.Delete();
		}
	}
}
