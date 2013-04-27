using System.Linq;
using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class JumpPointTests
		: TestsBase
	{
		[Test]
		public void Test_Make_And_Navigate_Jumppoint()
		{
			var dir = this.WorkingDirectory.ResolveDirectory("Directory1");

			this.WorkingDirectory.AddJumpPoint("JumpPoint", dir);

			Assert.IsTrue(this.WorkingDirectory.ResolveDirectory("JumpPoint").Exists);

			Assert.AreEqual(2, this.WorkingDirectory.ResolveDirectory("JumpPoint").GetChildNames(NodeType.File).Count());
		}
	}
}
