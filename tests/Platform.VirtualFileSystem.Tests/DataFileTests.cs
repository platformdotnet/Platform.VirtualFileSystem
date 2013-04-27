using System;
using NUnit.Framework;
using Platform.VirtualFileSystem.DataFile;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class DataFileTests
		: TestsBase
	{
		[XmlElement]
		public class Foo
		{
			[XmlElement]
			public string Name { get; set; }

			public Foo()
			{
			}

			public Foo(string name)
			{
				this.Name = name;
			}
		}

		[Test]
		public void TestDataFile1()
		{
			var dataFile = (DataFile<Foo>)this.WorkingDirectory.Resolve
			(
				"DataFile1.xml",
				new DataFileNodeType<Foo>(new XmlObjectDataFileLoaderSaver<Foo>())
			);

			dataFile.Refresh();

			Assert.IsTrue(dataFile.Value.Name == "Walter White" || dataFile.Value.Name == "Bob Marley");

			dataFile.Value.Name = "Bob Marley";

			Assert.AreEqual("Bob Marley", dataFile.Value.Name);

			dataFile.Save();
			dataFile.Load();

			Assert.AreEqual("Bob Marley", dataFile.Value.Name);
		}
	}
}
