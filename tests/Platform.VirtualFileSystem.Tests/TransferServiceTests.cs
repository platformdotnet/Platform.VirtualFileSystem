using System;
using System.Collections.Generic;
using NUnit.Framework;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Tests
{
	[TestFixture]
	public class TransferServiceTests
		: TestsBase
	{
		[Test]
		public void Test_Transfer_File_And_Then_Compare()
		{
			var src = this.WorkingDirectory.ResolveFile("TextFile1.txt");
			var des = FileSystemManager.Default.ResolveFile("temp:///CopyOfTextFile1.txt");
			var transferStates = new List<TransferState>();

			var service = src.GetService<IFileTransferService>(new FileTransferServiceType(des));

			service.TransferStateChanged += (sender, arg) => transferStates.Add(arg.CurrentTransferState);
			service.Progress.ValueChanged += (sender, args) => Console.WriteLine("Progress: {0}%", Convert.ToDouble(args.NewValue) / Convert.ToDouble((service.Progress.MaximumValue)) * 100);
			

			service.Start();

			service.WaitForAnyTaskState(TaskState.Finished);

			Assert.AreEqual(new List<TransferState> { TransferState.Preparing, TransferState.Comparing, TransferState.Transferring, TransferState.Tidying, TransferState.Finished }, transferStates);

			var comparingService = src.GetService<IFileComparingService>(new FileComparingServiceType(des, FileComparingFlags.CompareAll));

			Assert.IsTrue(comparingService.Compare());
		}
	}
}
