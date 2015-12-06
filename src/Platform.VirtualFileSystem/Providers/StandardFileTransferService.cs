using System;
using System.IO;
using System.Threading;
using Platform.IO;

namespace Platform.VirtualFileSystem.Providers
{
	public enum TransferState
	{
		NotStarted,
		Preparing,
		Comparing,
		Transferring,
		Copying,
		Tidying,
		Finished,
		Stopped
	}

	public class StandardFileTransferService
		: AbstractRunnableService, IFileTransferService
	{
		private readonly IFile source;
		private readonly IFile destination;
		private readonly FileTransferServiceType serviceType;

		public virtual event FileTransferServiceEventHandler TransferStateChanged;

		public virtual void OnTransferStateChanged(FileTransferServiceEventArgs eventArgs)
		{
			if (TransferStateChanged != null)
			{
				TransferStateChanged(this, eventArgs);
			}
		}
	
		public virtual INode OperatingNode
		{
			get
			{
				return this.source;
			}
		}

		public virtual INode TargetNode
		{
			get
			{
				return this.destination;
			}
		}

		public virtual string HashAlgorithmName { get; set; }

		public StandardFileTransferService(IFile source, IFile destination)
			: this(source, new FileTransferServiceType(destination, true))
		{
		}

		public StandardFileTransferService(IFile source, FileTransferServiceType serviceType)
		{
			this.serviceType = serviceType;
			this.source = source;
			this.destination = this.serviceType.Destination;
			this.HashAlgorithmName = serviceType.HashAlgorithmName;

			this.progress = new TransferProgress(this);			
		}

		public override IMeter Progress
		{
			get
			{
				return this.progress;
			}
		}
		private readonly TransferProgress progress;

		private sealed class TransferProgress
			: AbstractMeter
		{
			private readonly StandardFileTransferService service;

			public TransferProgress(StandardFileTransferService service)
			{
				this.service = service;
			}

			public override object Owner
			{
				get
				{
					return this.service;
				}
			}

			public override object MaximumValue
			{
				get
				{
					return this.service.GetBytesToTransfer();
				}
			}

			public override object MinimumValue
			{
				get
				{
					return 0;
				}
			}

			public override object CurrentValue
			{
				get
				{
					return this.service.GetBytesTransferred();
				}
			}

			public override string Units
			{
				get
				{
					return "bytes";
				}
			}

			public override string ToString()
			{
				switch (this.service.transferState)
				{
					case TransferState.Finished:
						return String.Format("Finished {0}/{1} bytes ({2:0}%)", CurrentValue, MaximumValue, Convert.ToDouble(CurrentValue) / Convert.ToDouble(MaximumValue) * 100.0);
					case TransferState.Transferring:
						return String.Format("Transferring {0}/{1} bytes ({2:0.##}%)", CurrentValue, MaximumValue, Convert.ToDouble(CurrentValue) / Convert.ToDouble(MaximumValue) * 100.0);
					default:
						return Enum.GetName(typeof(TransferState), this.service.transferState);
				}
			}

			public void RaiseValueChanged(object oldValue, object newValue)
			{
				OnValueChanged(oldValue, newValue);
			}

			public void RaiseMajorChange()
			{
				OnMajorChange();
			}

			private bool pumpRegistered = false;

			public void RaiseStateChanged()
			{
				if (this.service.transferState == TransferState.Transferring)
				{
					lock (this)
					{
						if (!this.pumpRegistered && this.service.copier != null)
						{
							this.service.copier.Progress.ValueChanged += PumpProgress_ValueChanged;
							this.service.copier.Progress.MajorChange += new EventHandler(PumpProgress_MajorChange);

							this.pumpRegistered = true;
						}
					}
				}
			}

			private void PumpProgress_ValueChanged(object sender, MeterEventArgs eventArgs)
			{
				OnValueChanged((long)eventArgs.OldValue + this.service.offset, (long)eventArgs.NewValue + this.service.offset);
			}

			private void PumpProgress_MajorChange(object sender, EventArgs e)
			{
				OnMajorChange();
			}
		}

		protected static TaskState ToTaskState(TransferState transferState)
		{
			switch (transferState)
			{
				case TransferState.NotStarted:
					return TaskState.NotStarted;
				case TransferState.Preparing:
				case TransferState.Comparing:
				case TransferState.Transferring:
				case TransferState.Copying:
				case TransferState.Tidying:
					return TaskState.Running;
				case TransferState.Finished:
					return TaskState.Finished;
				case TransferState.Stopped:
					return TaskState.Stopped;
				default:
					return TaskState.Unknown;
			}
		}
				
		public TransferState TransferState
		{
			get
			{
				return this.transferState;
			}
		}

		private TransferState transferState = TransferState.NotStarted;

		private void SetTransferState(TransferState value)
		{
			lock (this)
			{
				var oldValue = this.transferState;

				if (this.transferState != value)
				{
					this.transferState = value;

					OnTransferStateChanged(new FileTransferServiceEventArgs((TransferState)oldValue, this.transferState));

					SetTaskState(ToTaskState(this.transferState));

					Monitor.PulseAll(this);

					this.progress.RaiseStateChanged();
				}
			}
		}

		private StreamCopier copier;
		private long bytesTransferred = 0L;

		private long GetBytesTransferred()
		{
			if (this.copier != null)
			{
				return Convert.ToInt64(this.copier.Progress.CurrentValue) + this.offset;
			}
			else
			{
				return this.bytesTransferred + this.offset;
			}
		}

		private long GetBytesToTransfer()
		{
			if (this.copier != null)
			{
				return Convert.ToInt64(this.copier.Progress.MaximumValue) + this.offset;
			}
			else
			{
				return this.source.Length ?? 0;
			}
		}
		private long offset = 0;

		public override void DoRun()
		{
			IFile destinationTemp;
			Stream destinationTempStream;
			string sourceHash;

			try
			{
				lock (this)
				{
					SetTransferState(TransferState.Preparing);
				}

				Action<IFile> transferAttributes = delegate(IFile dest)
				{
					using (dest.Attributes.AquireUpdateContext())
					{
						foreach (string s in this.serviceType.AttributesToTransfer)
						{
							dest.Attributes[s] = this.source.Attributes[s];
						}
					}
				};

				Stream sourceStream = null;

				for (var i = 0; i < 4; i++)
				{
					try
					{
						sourceStream = this.OperatingNode.GetContent().GetInputStream(FileMode.Open, FileShare.Read);

						break;
					}
					catch (NodeNotFoundException)
					{
						throw;
					}
					catch (Exception)
					{
						if (i == 3)
						{
							throw;
						}
					}

					ProcessTaskStateRequest();
				}

				using (sourceStream)
				{
					var sourceHashingService = (IHashingService)this.OperatingNode.GetService(new StreamHashingServiceType(sourceStream, this.HashAlgorithmName));

					// Compute the hash of the source file

					SetTransferState(TransferState.Comparing);
					ProcessTaskStateRequest();

					sourceHash = sourceHashingService.ComputeHash().TextValue;

					// Try to open the destination file

					ProcessTaskStateRequest();

					var destinationHashingService = (IHashingService)this.TargetNode.GetService(new FileHashingServiceType(this.HashAlgorithmName));

					string destinationHash;

					try
					{
						destinationHash = destinationHashingService.ComputeHash().TextValue;
					}
					catch (DirectoryNodeNotFoundException)
					{
						this.TargetNode.ParentDirectory.Create(true);

						try
						{
							destinationHash = destinationHashingService.ComputeHash().TextValue;
						}
						catch (NodeNotFoundException)
						{
							destinationHash = null;
						}
					}
					catch (NodeNotFoundException)
					{
						destinationHash = null;
					}

					ProcessTaskStateRequest();

					// Source and destination are identical

					if (sourceHash == destinationHash)
					{
						SetTransferState(TransferState.Transferring);

						this.progress.RaiseValueChanged(0, GetBytesToTransfer());

						SetTransferState(TransferState.Tidying);

						// Transfer attributes

						try
						{
							transferAttributes((IFile) this.TargetNode);
						}
						catch (FileNotFoundException)
						{
						}

						// Done

						SetTransferState(TransferState.Finished);
						ProcessTaskStateRequest();

						return;
					}

					// Get a temp file for the destination based on the source's hash

					destinationTemp = ((ITempIdentityFileService)this.destination.GetService(new TempIdentityFileServiceType(sourceHash))).GetTempFile();

					// Get the stream for the destination temp file

					try
					{
						if (!destinationTemp.ParentDirectory.Exists)
						{
							destinationTemp.ParentDirectory.Create(true);
						}
					}
					catch (IOException)
					{
					}

					using (destinationTempStream = destinationTemp.GetContent().OpenStream(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
					{
						Action finishUp = delegate
						{
							SetTransferState(TransferState.Tidying);

							destinationTempStream.Close();

							for (int i = 0; i < 4; i++)
							{
								try
								{
									// Save hash value
									StandardFileHashingService.SaveHashToCache((IFile) destinationTemp, this.HashAlgorithmName,
						                   										sourceHash, (IFile) this.TargetNode);

									try
									{
										// Transfer attributes
										transferAttributes(destinationTemp);
									}
									catch (FileNotFoundException e)
									{
										Console.WriteLine(e);
									}

									// Move destination temp to destination
									destinationTemp.MoveTo(this.TargetNode, true);

									break;
								}
								catch (Exception)
								{
									if (i == 3)
									{
										throw;
									}
								}

								ProcessTaskStateRequest();
							}

							// Done

							SetTransferState(TransferState.Finished);
							ProcessTaskStateRequest();
						};

						// Get the hash for the destination temp file

						var destinationTempHashingService = (IHashingService) destinationTemp.GetService(new StreamHashingServiceType(destinationTempStream));

						// If the destination temp and the source aren't the same
						// then complete the destination temp

						string destinationTempHash;

						if (destinationTempStream.Length >= sourceStream.Length)
						{
							// Destination is longer than source but starts source (unlikely)

							destinationTempHash = destinationTempHashingService.ComputeHash(0, sourceStream.Length).TextValue;

							if (destinationTempHash == sourceHash)
							{
								if (destinationTempStream.Length != sourceStream.Length)
								{
									destinationTempStream.SetLength(sourceStream.Length);
								}

								finishUp();

								return;
							}

							destinationTempStream.SetLength(0);
						}

						if (destinationTempStream.Length > 0)
						{
							destinationTempHash = destinationTempHashingService.ComputeHash().TextValue;

							// Destination shorter than the source but is a partial copy of source

							sourceHash = sourceHashingService.ComputeHash(0, destinationTempStream.Length).TextValue;

							if (sourceHash == destinationTempHash)
							{
								this.offset = destinationTempStream.Length;
							}
							else
							{
								this.offset = 0;
								destinationTempStream.SetLength(0);
							}
						}
						else
						{
							this.offset = 0;
						}

						this.progress.RaiseValueChanged(0, this.offset);

						// Transfer over the remaining part needed (or everything if offset is 0)

						this.offset = destinationTempStream.Length;

						Stream sourcePartialStream = new PartialStream(sourceStream, destinationTempStream.Length);
						Stream destinationTempPartialStream = new PartialStream(destinationTempStream, destinationTempStream.Length);

						this.copier =
							new StreamCopier(new BufferedStream(sourcePartialStream, this.serviceType.BufferSize), destinationTempPartialStream,
							               false, false, this.serviceType.ChunkSize);

						this.copier.TaskStateChanged += delegate(object sender, TaskEventArgs eventArgs)
						                           {
						                           	if (eventArgs.TaskState == TaskState.Running
						                           	    || eventArgs.TaskState == TaskState.Paused
						                           	    || eventArgs.TaskState == TaskState.Stopped)
						                           	{
						                           		SetTaskState(eventArgs.TaskState);
						                           	}
						                           };

						SetTransferState(TransferState.Transferring);
						ProcessTaskStateRequest();

						this.copier.Run();

						if (this.copier.TaskState == TaskState.Stopped)
						{
							throw new StopRequestedException();
						}

						finishUp();
					}
				}
			}
			catch (StopRequestedException)
			{
			}
			finally
			{
				if (this.TransferState != TransferState.Finished)
				{
					SetTransferState(TransferState.Stopped);
				}
			}
		}

		private void Pump_TaskStateChanged(object sender, TaskEventArgs eventArgs)
		{
			if (eventArgs.TaskState == TaskState.Running || eventArgs.TaskState == TaskState.Paused)
			{
				SetTaskState(eventArgs.TaskState);
			}
		}

		public override bool RequestTaskState(TaskState taskState, TimeSpan timeout)
		{
			if (this.copier != null && this.copier.TaskState != TaskState.NotStarted
				&& this.copier.TaskState != TaskState.Finished && this.copier.TaskState != TaskState.Stopped)
			{
				this.copier.RequestTaskState(taskState, timeout);
			}
			
			return base.RequestTaskState(taskState, timeout);
		}
	}
}
