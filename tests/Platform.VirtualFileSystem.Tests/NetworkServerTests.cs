using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Platform.IO;
using Platform.VirtualFileSystem.Network.Server;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class NetworkServerTests
		: TestsBase
	{
		private INetworkFileSystemServer server;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			this.server = TextBasedServer.CreateServer();

			this.server.Start();
			this.server.WaitForAnyTaskState(TaskState.Running, TaskState.Finished, TaskState.Stopped);
		}

		[TearDown]
		public virtual void TearDown()
		{
			this.server.Stop();
		}

		[Test]
		public void Test_Connect_To_Server_And_Read_File()
		{
			var dir = FileSystemManager.Default.ResolveDirectory("netvfs://localhost[testfiles:///]/");

			dir.Refresh();

			Assert.IsTrue(dir.Exists);

			var file = dir.ResolveFile("TextFile1.txt");

			file.Refresh();

			Assert.IsTrue(file.Exists);

			Assert.AreEqual("TextFile1.txt", file.GetContent().GetReader().ReadToEndThenClose());
		}

		[Test]
		public void Test_Connect_To_Server_And_Use_Remote_Hash_Service()
		{
			var file = FileSystemManager.Default.ResolveFile("netvfs://localhost[testfiles:///]/TextFile1.txt");
			
			file.Refresh();

			Assert.IsTrue(file.Exists);
			Assert.AreEqual("TextFile1.txt", file.GetContent().GetReader().ReadToEndThenClose());
			
			var service = file.GetService<IFileHashingService>(new FileHashingServiceType());
			Assert.AreEqual("NetworkFileHashingService", service.GetType().Name);
			Assert.AreEqual("bec084d430670e66976d5abc24627a54", service.ComputeHash().TextValue);
		}
	}
}
