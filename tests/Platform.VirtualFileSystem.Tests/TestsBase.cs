using System;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	public class TestsBase
	{
		protected IDirectory WorkingDirectory { get; set; }

		[SetUp]
		public virtual void SetUp()
		{
			this.WorkingDirectory = FileSystemManager.GetManager().ResolveDirectory(Environment.CurrentDirectory).ResolveDirectory("TestFiles");
		}
	}
}
