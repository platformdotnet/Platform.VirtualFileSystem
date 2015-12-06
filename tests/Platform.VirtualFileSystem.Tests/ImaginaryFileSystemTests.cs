using System.Linq;
using NUnit.Framework;
using Platform.VirtualFileSystem.Providers.Imaginary;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class ImaginaryFileSystemTests
		: TestsBase
	{
		[Test]
		public void Test_Create_ImaginaryFileSystem_And_ImaginaryDirectory_And_Resolve()
		{
			var fileSystem = new ImaginaryFileSystem("imaginary");

			var fakeDirectory = fileSystem.ResolveDirectory("/Fake");

			Assert.IsFalse(fakeDirectory.Exists);

			fileSystem.ResolveDirectory("Fake").Create();

			fakeDirectory = fileSystem.ResolveDirectory("/Fake");

			Assert.IsTrue(fakeDirectory.Exists);

			fakeDirectory.AddJumpPoint("A", this.WorkingDirectory.ResolveDirectory("Directory1"));

			Assert.AreEqual(typeof(ImaginaryDirectory), fakeDirectory.GetType());

			var dir =  fileSystem.ResolveDirectory("/Fake/A");

			Assert.IsTrue(dir.Exists);
			Assert.Greater(dir.GetChildNames().Count(), 0);
		}
	}
}
