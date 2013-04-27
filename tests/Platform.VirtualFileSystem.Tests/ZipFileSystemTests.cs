using System;
using System.Linq;
using NUnit.Framework;
using Platform.VirtualFileSystem.Providers.Zip;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class ZipFileSystemTests
		: TestsBase
	{
		[Test]
		public void Test_Create_Zip_FileSystem()
		{
			using (var fileSystem = ZipFileSystem.CreateZipFile(FileSystemManager.Default.ResolveFile("temp:///TestZipFile.zip"), this.WorkingDirectory.ResolveDirectory("Directory1")))
			{
				var names = fileSystem.RootDirectory.GetChildNames().ToList();

				Assert.AreEqual(4, names.Count);
				Assert.AreEqual(2, fileSystem.RootDirectory.GetChildren(NodeType.File).Count());
				Assert.AreEqual(2, fileSystem.RootDirectory.GetChildren(NodeType.Directory).Count());

				Assert.AreEqual("zip://[temp:///TestZipFile.zip]/", fileSystem.RootDirectory.Address.ToString());
				var fileContents = fileSystem.ResolveFile("SubDirectory1/A.csv").GetContent().GetReader().ReadToEnd();

				Assert.AreEqual("A.csv", fileContents);
			}

			using (var fileSystem = FileSystemManager.Default.ResolveDirectory("zip://[temp:///TestZipFile.zip]/").FileSystem)
			{
				var file = fileSystem.ResolveFile("SubDirectory1/A.csv");

				Assert.AreEqual(typeof(ZipFile), file.GetType());

				var fileContents = fileSystem.ResolveFile("SubDirectory1/A.csv").GetContent().GetReader().ReadToEnd();

				Assert.AreEqual("A.csv", fileContents);
			}
		}
	}
}
