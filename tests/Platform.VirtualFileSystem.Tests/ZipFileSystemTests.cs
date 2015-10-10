using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Platform.IO;
using Platform.VirtualFileSystem.Providers.Zip;
using System.Collections.Generic;

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

				Assert.IsFalse(newFile2.ParentDirectory.Exists);
				newFile2.ParentDirectory.Create();
				Assert.IsTrue(newFile2.ParentDirectory.Exists);

				Assert.IsFalse(newFile2.Exists);

				using (var writer = newFile2.GetContent().GetWriter())
				{
					writer.Write("A");
				}

				newFile2.Refresh();
				Assert.IsTrue(newFile2.Exists);

				fileSystem.ResolveDirectory("/NewDirectory").Create();

				var z = fileSystem.ResolveDirectory("/X/Y/Z");

				Assert.IsFalse(z.Exists);

				Assert.Catch<DirectoryNodeNotFoundException>(() => z.Create());

				z.Create(true);

				Assert.IsTrue(z.Exists);
				Assert.IsTrue(fileSystem.ResolveDirectory("/X/Y").Exists);
				Assert.IsTrue(fileSystem.ResolveDirectory("/X").Exists);
			}

			using (var fileSystem = FileSystemManager.Default.ResolveDirectory("zip://[temp:///TestReadWriteZipFile.zip]/").FileSystem)
			{
				var testFile = fileSystem.ResolveFile("Test.txt");

				Assert.AreEqual("Test", testFile.GetContent().GetReader().ReadToEndThenClose());

				using (var writer = testFile.GetContent().GetWriter())
				{
					writer.Write("Hello");
				}

				Assert.IsTrue(testFile.Exists);
				
				Assert.AreEqual("Hello", testFile.GetContent().GetReader().ReadToEndThenClose());

				Assert.IsTrue(fileSystem.ResolveDirectory("/NewDirectory").Exists);
				Assert.IsTrue(fileSystem.ResolveDirectory("/X/Y/Z").Exists);
				Assert.IsTrue(fileSystem.ResolveDirectory("/X/Y").Exists);
				Assert.IsTrue(fileSystem.ResolveDirectory("/X").Exists);
			}

			using (var fileSystem = FileSystemManager.Default.ResolveDirectory("zip://[temp:///TestReadWriteZipFile.zip]/").FileSystem)
			{
				var testFile = fileSystem.ResolveFile("Test.txt");

				Assert.AreEqual("Hello", testFile.GetContent().GetReader().ReadToEndThenClose());

				testFile.Delete();
			}

			using (var fileSystem = FileSystemManager.Default.ResolveDirectory("zip://[temp:///TestReadWriteZipFile.zip]/").FileSystem)
			{
				var testFile = fileSystem.ResolveFile("Test.txt");

				Assert.IsFalse(testFile.Exists);
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

				newFile.ParentDirectory.Create();

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

		[Test]
		public void Test_Walk_Zip_Directory()
		{
			using (var zfs = new ZipFileSystem(this.WorkingDirectory.ResolveFile("TestWalkZipDir.zip")))
			{
				const int limit = 100;
				int count = 0;
				var entries = new List<INode>();
				foreach (var entry in zfs.RootDirectory.Walk())
				{
					entries.Add(entry);
					if (++count > limit)
						Assert.Fail("Infinite loop when walking zip directory");
				}
				Assert.AreEqual(5, entries.Count);
				Assert.AreEqual(1, entries.Count(x =>
					x.Name == "sample" &&
					x.Address.AbsolutePath == "/sample" &&
					x.NodeType == NodeType.Directory));
				Assert.AreEqual(1, entries.Count(x =>
					x.Name == "TextFile2.txt" &&
					x.Address.AbsolutePath == "/sample/TextFile2.txt" &&
					x.NodeType == NodeType.File));
				Assert.AreEqual(1, entries.Count(x =>
					x.Name == "data" &&
					x.Address.AbsolutePath == "/sample/data" &&
					x.NodeType == NodeType.Directory));
				Assert.AreEqual(1, entries.Count(x =>
					x.Name == "DataFile1.xml" &&
					x.Address.AbsolutePath == "/sample/data/DataFile1.xml" &&
					x.NodeType == NodeType.File));
				Assert.AreEqual(1, entries.Count(x =>
					x.Name == "TextFile1.txt" &&
					x.Address.AbsolutePath == "/sample/data/TextFile1.txt" &&
					x.NodeType == NodeType.File));
			}
		}
	}
}
