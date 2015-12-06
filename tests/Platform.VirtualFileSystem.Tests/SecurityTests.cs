using NUnit.Framework;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class SecurityTests
		: TestsBase
	{
		[Test]
		public void Test()
		{
			IDirectoryHashingService service;

			var dir = this.WorkingDirectory.ResolveDirectory("Directory1/SubDirectory2");

			using (dir.FileSystem.SecurityManager.AcquireSecurityContext(true))
			{
				dir.FileSystem.SecurityManager.CurrentContext.AddPermissionVerifier
				(
					AccessPermissionVerifier.FromPredicate((context) =>
					{
						if (context.Operation == FileSystemSecuredOperation.View)
						{
							if (context.Node.Name.StartsWith("A"))
							{
								return AccessVerificationResult.Granted;
							}

							return AccessVerificationResult.Denied;
						}
						else
						{
							return AccessVerificationResult.Granted;
						}
					})
				);

				service = (IDirectoryHashingService)dir.GetService(new DirectoryHashingServiceType(true));

				Assert.AreEqual("00000000010000000000000001000000360bd011c4b8e0aa9e712bdcc6d1f7e0e1053256c73b62c48c2c3cdcd9309c3f77e67c34f19a6dcfc055d613573060bb81bb98ac6575bf0242c10e002ed45d3d6512e76e42690d7ab8cb09de5fe2e820f4bf4e8e15694a2e1f3f0ad6480a4cba528d204a799c60fcf2e5d27e4e565a3289d46f53e022d3662ab413b7733981af", service.ComputeHash().TextValue);
			}

			service = (IDirectoryHashingService)dir.GetService(new DirectoryHashingServiceType(true));

			Assert.AreEqual("0000000002000000000000000200000028f558771fb2706554cc5937088a687fdfc0a1d4de55eae855665d5b5408ca49e96e719bc459ac853cfbe372dbf86a84170399854f2748e6e7f16b67765b940d7091a32b6f10538476c7729af3ababd2e2ed0b103c7d2075365232387133c300545507df3ef2239133c978b9b946e7dfa3d807d5aa44ec988f79fb4017fdceb3", service.ComputeHash().TextValue);
		}
	}
}
