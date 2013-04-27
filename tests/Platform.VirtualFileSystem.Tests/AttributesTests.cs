using System;
using System.Text;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class AttributesTests
		: TestsBase
	{
		[Test]
		public void TestAdvancedAttributeAccess()
		{
			var file = this.WorkingDirectory.ResolveFile("TextFile1.txt");

			Assert.IsTrue(file.Exists);

			var date1 = (DateTime)file.Attributes["LastWriteTime"];
			var date2 = (DateTime)file.Attributes["LastWriteTime|secondsresolution"];

			Assert.IsTrue((long)TimeSpan.FromTicks(date1.Ticks).TotalSeconds == (long)TimeSpan.FromTicks(date2.Ticks).TotalSeconds);
		}

		[Test]
		public virtual void TestExtendedAttributes()
		{
			const string attrName = "extended:testxattr";
			const string attrValue = "testxattrvalue";
			var file = this.WorkingDirectory.ResolveFile("TextFile1.txt");

			file.Attributes[attrName] = attrValue;

			foreach (var keyValue in file.Attributes)
			{
				if (keyValue.Key == attrName)
				{
					Assert.AreEqual(Encoding.UTF8.GetString((byte[]) keyValue.Value), attrValue);
				}
			}

			Assert.AreEqual(Encoding.UTF8.GetString((byte[])file.Attributes[attrName]), attrValue);

			Assert.AreEqual(file.Attributes[attrName + "|string"], attrValue);

			file.Attributes[attrName] = null;
			Assert.IsNull(file.Attributes[attrName]);
		}
	}
}
