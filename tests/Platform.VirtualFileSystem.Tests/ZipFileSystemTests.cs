using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Platform.VirtualFileSystem.Providers.Zip;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class ZipFileSystemTests
		: TestsBase
	{
		[Test]
		public void Test_Create_Empty_ZipFileSystem_And_Write_Files()
		{
			using (var fileSystem = ZipFileSystem.CreateZipFile(FileSystemManager.Default.ResolveFile("temp:///TestReadWriteZipFile.zip")))
			{
				var newFile = fileSystem.ResolveFile("Test.txt");
				
				Assert.IsFalse(newFile.Exists);

				using (var writer = newFile.GetContent().GetWriter())
				{
					writer.Write("Test");
				}

				newFile.Refresh();
				Assert.IsTrue(newFile.Exists);

				var newFile2 = fileSystem.ResolveFile("/TestDir/A.txt");

				Assert.IsFalse(newFile2.Exists);

				using (var writer = newFile2.GetContent().GetWriter())
				{
					writer.Write("A");
				}

				newFile2.Refresh();
				Assert.IsTrue(newFile2.Exists);
			}
		}

		[Test]
		public void Test_Create_ZipFileSystem_And_Write_Files()
		{
			using (var fileSystem = ZipFileSystem.CreateZipFile(FileSystemManager.Default.ResolveFile("temp:///TestZipFile.zip"), this.WorkingDirectory.ResolveDirectory("Directory1")))
			{
				var fileContents = fileSystem.ResolveFile("SubDirectory1/A.csv").GetContent().GetReader().ReadToEnd();

				Assert.AreEqual("A.csv", fileContents);

				var newFile = fileSystem.ResolveFile("SubDirectory1/B.txt");

				Assert.IsFalse(newFile.Exists);

				using (var writer = newFile.GetContent().GetWriter(Encoding.UTF8))
				{
					writer.Write("B.txt");
				}

				Assert.IsTrue(newFile.Exists);
			}
		}

		[Test]
		public void Test_Create_Zip_FileSystem()
		{
			using (var fileSystem = ZipFileSystem.CreateZipFile(FileSystemManager.Default.ResolveFile("temp:///TestZipFile.zip"), this.WorkingDirectory.ResolveDirectory("Directory1")))
			{
				var names = fileSystem.RootDirectory.GetChildNames().ToList();

				Assert.AreEqual(4, names.Count);
				Assert.AreEqual(2, fileSystem.RootDirectory.GetChildren(NodeType.File).Count());
				Assert.AreEqual(2, fileSystem.RootDirectory.GetChildren(NodeType.Directory).Count());

				var dir = fileSystem.ResolveDirectory("SubDirectory1");
				Assert.IsTrue(dir.Exists);

				dir = fileSystem.ResolveDirectory("SubDirectory2");
				Assert.IsTrue(dir.Exists);

				dir = fileSystem.ResolveDirectory("SubDirectory3");
				Assert.IsFalse(dir.Exists);

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
