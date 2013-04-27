using System.Linq;
using NUnit.Framework;
using Platform.IO;
using Platform.VirtualFileSystem.Providers.Overlayed;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class OverlayedFileSystemTests
		: TestsBase
	{
		[Test]
		public void Test_Make_Overlayed_FileSystem()
		{
			var fileSystem1 = this.WorkingDirectory.ResolveDirectory("Directory1").CreateView();
			var fileSystem2 = this.WorkingDirectory.ResolveDirectory("Directory2").CreateView();

			var overlayedFileSystem = new OverlayedFileSystem(fileSystem1, fileSystem2);

			var dir = overlayedFileSystem.RootDirectory;

			var list = dir.GetChildNames().ToList();

			Assert.AreEqual(5, list.Count);

			var file = dir.ResolveFile("C.txt");

			Assert.AreEqual(2, file.GetAlternates().Count());
			Assert.IsTrue(file.Exists);
			Assert.AreEqual("C.txt", file.GetContent().GetReader().ReadToEndThenClose());

			file = dir.ResolveFile("A.txt");
			Assert.AreEqual(2, file.GetAlternates().Count());
			Assert.IsTrue(file.Exists);
			Assert.AreEqual("A.txt", file.GetContent().GetReader().ReadToEndThenClose());

			file = dir.ResolveFile("B.txt");
			Assert.AreEqual(2, file.GetAlternates().Count());
			Assert.IsTrue(file.Exists);
			Assert.AreEqual("B.txt", file.GetContent().GetReader().ReadToEndThenClose());

			var subdir = dir.ResolveDirectory("SubDirectory1");
			Assert.AreEqual(2, subdir.GetAlternates().Count());
			Assert.IsTrue(subdir.Exists);

			file = dir.ResolveFile("SubDirectory1/A.csv");
			Assert.IsTrue(file.Exists);
			Assert.AreEqual(2, file.GetAlternates().Count());
			Assert.AreEqual("A.csv", file.GetContent().GetReader().ReadToEndThenClose());
			Assert.AreEqual("A.csv(2)", file.GetAlternates().ToList()[1].GetContent().GetReader().ReadToEndThenClose());
		}
	}
}
