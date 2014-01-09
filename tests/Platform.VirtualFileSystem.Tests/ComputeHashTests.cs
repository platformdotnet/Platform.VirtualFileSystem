using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class ComputeHashTests
		: TestsBase
	{
		[Test]
		public void Test_Hash_First_Few_Bytes()
		{
			var file = this.WorkingDirectory.ResolveFile("TextFile1.txt");

			var service = (IHashingService)file.GetService(new FileHashingServiceType());

			Assert.AreEqual("c54ee3ab10639f2979d2fc27f6122e87", service.ComputeHash(0, 4).TextValue);
		}

		[Test]
		public void Test_Hash_Whole_File()
		{
			var file = this.WorkingDirectory.ResolveFile("TextFile1.txt");

			this.WorkingDirectory.ResolveDirectory("a").GetFiles(c => c.Address.Extension == "dll");

			var service = (IHashingService)file.GetService(new FileHashingServiceType());

			Assert.AreEqual("bec084d430670e66976d5abc24627a54", service.ComputeHash().TextValue);
		}

		[Test]
		public void Test_Hash_Directory()
		{
			var dir = this.WorkingDirectory.ResolveDirectory("Directory1/SubDirectory1");

			Assert.That(dir.GetChildNames().Count(), Is.EqualTo(1));

			var service = (IHashingService)dir.GetService(new DirectoryHashingServiceType(true));

			var hashValue = service.ComputeHash();

			Assert.AreEqual("00000000010000000000000001000000360bd011c4b8e0aa9e712bdcc6d1f7e0e1053256c73b62c48c2c3cdcd9309c3f77e67c34f19a6dcfc055d613573060bb81bb98ac6575bf0242c10e002ed45d3d6512e76e42690d7ab8cb09de5fe2e820f4bf4e8e15694a2e1f3f0ad6480a4cba528d204a799c60fcf2e5d27e4e565a3289d46f53e022d3662ab413b7733981af", hashValue.TextValue);
		}
	}
}
