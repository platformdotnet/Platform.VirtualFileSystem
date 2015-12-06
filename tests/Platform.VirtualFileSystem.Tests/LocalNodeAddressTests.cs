using System;
using NUnit.Framework;
using Platform.VirtualFileSystem.Providers.Local;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class LocalNodeAddressTests
		: TestsBase
	{
		[Test]
		public void Test_Parse_Unix_Address()
		{
			var address = LocalNodeAddress.Parse("/usr/local/src");

			Assert.AreEqual("", address.RootPart);
			Assert.AreEqual("file://", address.RootUri);
			Assert.AreEqual("file:///usr/local/src", address.Uri);
		}

		[Test]
		public void Test_Parse_Unix_Root_Address()
		{
			var address = LocalNodeAddress.Parse("/");

			Assert.AreEqual("", address.RootPart);
			Assert.AreEqual(true, address.IsRoot);
			Assert.AreEqual("/", address.AbsolutePath);
		}

		[Test]
		public void Test_Parse_Windows_Address()
		{
			var address = LocalNodeAddress.Parse("c:/windows/system32");

			Assert.AreEqual("c:", address.RootPart);
			Assert.AreEqual("file://c:", address.RootUri);
			Assert.AreEqual("/windows/system32", address.AbsolutePath);
			Assert.AreEqual("file://c:/windows/system32", address.Uri);
		}

		[Test]
		public void Test_Parse_Windows_Address_Relative_And_Check_Root_Part()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				var address = LocalNodeAddress.Parse(".");

				Assert.AreNotEqual("", address.RootPart);
				Assert.IsTrue(address.RootPart.EndsWith(":"));
			}
		}

		[Test]
		public void Test_Parse_Unix_Relative_Address()
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var address = LocalNodeAddress.Parse(".");

				Assert.AreEqual("", address.RootPart);
				Assert.IsTrue(address.AbsolutePath.StartsWith("/"));
			}
		}
	}
}
