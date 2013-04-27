using System;
using System.IO;
using System.Threading;
using Platform.IO;
using Platform.Text;
using Platform.Models;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public class OldStandardFileTransferService
		: AbstractRunnableService, IFileTransferService
	{
		private IFile m_Source;
		private IFile m_Destination;
		private FileTransferServiceType m_ServiceType;

		public virtual INode OperatingNode
		{
			get
			{
				return m_Source;
			}
		}

		public virtual INode TargetNode
		{
			get
			{
				return m_Destination;
			}
		}

		public OldStandardFileTransferService(IFile source, IFile destination)
			: this(source, new FileTransferServiceType(destination, true))
		{
		}

		public OldStandardFileTransferService(IFile source, FileTransferServiceType serviceType)
		{
			m_ServiceType = serviceType;
			m_Source = source;
			m_Destination = m_ServiceType.Destination;

			m_Progress = new TransferProgress(this);
		}

		public override Platform.Models.IMeter Progress
		{
			get
			{
				return m_Progress;
			}
		}
		private TransferProgress m_Progress;

		#region Types

		private class TransferProgress
			: AbstractMeter
		{
			private OldStandardFileTransferService m_Service;

			public TransferProgress(OldStandardFileTransferService service)
			{
				m_Service = service;
			}

			public override object Owner
			{
				get
				{
					return m_Service;
				}
			}

			public override object MaximumValue
			{
				get
				{
					return m_Service.GetBytesToTransfer();
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
					return m_Service.GetBytesTransferred();
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
				switch (m_Service.m_TransferState)
				{
					case TransferState.Finished:
						return String.Format("Finished {0}/{1} bytes ({2:0}%)", CurrentValue, MaximumValue, Convert.ToDouble(CurrentValue) / Convert.ToDouble(MaximumValue) * 100.0);
					case TransferState.Transferring:
						return String.Format("Transferring {0}/{1} bytes ({2:0.##}%)", CurrentValue, MaximumValue, Convert.ToDouble(CurrentValue) / Convert.ToDouble(MaximumValue) * 100.0);
					default:
						return Enum.GetName(typeof(TransferState), m_Service.m_TransferState);
				}
			}

			public virtual void RaiseValueChanged(object oldValue, object newValue)
			{
				OnValueChanged(oldValue, newValue);
			}

			public virtual void RaiseMajorChange()
			{
				OnMajorChange();
			}

			private bool m_PumpRegistered = false;

			public virtual void RaiseStateChanged()
			{
				if (m_Service.m_TransferState == TransferState.Transferring)
				{
					lock (this)
					{
						if (!m_PumpRegistered)
						{
							m_Service.m_Pump.Progress.ValueChanged += PumpProgress_ValueChanged;
							m_Service.m_Pump.Progress.MajorChange += new EventHandler(PumpProgress_MajorChange);

							m_PumpRegistered = true;
						}
					}
				}
			}

			private void PumpProgress_ValueChanged(object sender, MeterEventArgs eventArgs)
			{
				OnValueChanged((long)eventArgs.OldValue + m_Service.m_Offset,  (long)eventArgs.NewValue + m_Service.m_Offset);
			}

			private void PumpProgress_MajorChange(object sender, EventArgs e)
			{
				OnMajorChange();
			}
		}

		#endregion

		protected static TaskState ToTaskState(TransferState transferState)
		{
			switch (transferState)
			{
				case TransferState.NotStarted:
					return Platform.Models.TaskState.NotStarted;
				case TransferState.Preparing:
				case TransferState.Comparing:
				case TransferState.Transferring:
				case TransferState.Copying:
				case TransferState.Tidying:
					return Platform.Models.TaskState.Running;
				case TransferState.Finished:
					return Platform.Models.TaskState.Finished;
				case TransferState.Stopped:
					return Platform.Models.TaskState.Stopped;
				default:
					return Platform.Models.TaskState.Unknown;
			}
		}

		protected enum TransferState
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

		private TransferState m_TransferState = TransferState.NotStarted;

		private void SetTransferState(TransferState value)
		{
			lock (this)
			{
				object oldValue;

				oldValue = m_TransferState;

				m_TransferState = value;

				SetTaskState(ToTaskState(m_TransferState));

				Monitor.PulseAll(this);
				
				m_Progress.RaiseStateChanged();
			}
		}
		
		private StreamPump m_Pump;
		private Thread m_TaskThread;

		private long m_BytesTransferred = 0L;

		private long GetBytesTransferred()
		{
			if (m_Pump != null)
			{
				return Convert.ToInt64(m_Pump.Progress.CurrentValue) + m_Offset;
			}
			else
			{
				return m_BytesTransferred + m_Offset;
			}
		}

		private long GetBytesToTransfer()
		{
			if (m_Pump != null)
			{
				return Convert.ToInt64(m_Pump.Progress.MaximumValue) + m_Offset;
			}
			else
			{
				return m_Source.Length;
			}
		}

		protected override Thread GetTaskThread()
		{
			return m_TaskThread;
		}

		private long m_Offset = 0;

		public override void Run()
		{
			long x;
			string id;
			IFile destTemp;
			ITempIdentityFileService destTempService;
			IFile dest;
			Stream srcStream = null, destStream = null;
			IFileHashingService hasher;
			string sourceHash, destTempHash;

			try
			{
				lock (this.SyncLock)
				{
					m_TaskThread = Thread.CurrentThread;

					if (m_TransferState != TransferState.NotStarted)
					{
						throw new InvalidOperationException();
					}

					SetTransferState(TransferState.Preparing);
					ProcessTaskStateRequest();
				}

				id = m_Source.Address.Uri + m_Source.Length.ToString()
					+ (m_Source.Attributes.CreationTime ?? DateTime.MinValue).ToBinary().ToString();

				destTempService = (ITempIdentityFileService)m_Destination.GetService(new TempIdentityFileServiceType(id));

				destTemp = destTempService.GetTempFile();

				for (;;)
				{
					try
					{
						x = destTemp.Length;
					}
					catch (FileNotFoundException)
					{
						x = 0;	
					}

					dest = m_Destination.ParentDirectory.ResolveFile("$TMP_" + m_Destination.Address.Name + "_" + Guid.NewGuid().ToString("N"));

					try
					{
						if (x == m_Source.Length)
						{
							try
							{
								if (m_Source.IdenticalTo(destTemp, FileComparingFlags.CompareContents))
								{
									SetTransferState(TransferState.Copying);
									ProcessTaskStateRequest();

									m_Progress.RaiseValueChanged(m_Progress.CurrentValue, 0);
								
									destTemp.MoveTo(dest, true);							

									if (!m_Source.IdenticalTo(dest, FileComparingFlags.CompareContents))
									{
										continue;
									}

									dest.RenameTo(m_Destination.Address.NameAndQuery, true);

									m_BytesTransferred = m_Destination.Length;

									m_Progress.RaiseValueChanged(m_Progress.CurrentValue, m_BytesTransferred);

									SetTransferState(TransferState.Finished);
									ProcessTaskStateRequest();

									return;
								}
							}
							catch (IOException)
							{
						
							}
						}

						srcStream = m_Source.GetContent().GetInputStream(FileShare.Read);

						if (!srcStream.CanSeek)
						{
							destStream = destTemp.GetContent().GetOutputStream(FileMode.Create, FileShare.Read);	
						}
						else
						{
							destStream = destTemp.GetContent().GetOutputStream(FileMode.Append, FileShare.Read);

							SetTransferState(TransferState.Comparing);
							ProcessTaskStateRequest();

							hasher = (IFileHashingService)m_Source.GetService(new FileHashingServiceType("md5"));

							sourceHash = hasher.ComputeHash(0, destStream.Length).TextValue;

							hasher = (IFileHashingService)destTemp.GetService(new FileHashingServiceType("md5"));

							destTempHash = hasher.ComputeHash().TextValue;

							if (sourceHash != destTempHash)
							{
								destStream.Close();
								destStream = destTemp.GetContent().GetOutputStream(FileMode.Create, FileShare.Read);
							}
							else
							{
								m_Offset = destStream.Length;

								if (m_Offset > 0)
								{
									srcStream = new PartialStream(srcStream, m_Offset);
								}
							}
						}
		
						m_Progress.RaiseValueChanged(0, m_Offset);
						ProcessTaskStateRequest();

						m_Pump = new StreamPump(srcStream, destStream, true, false, m_ServiceType.BufferSize);

						m_Pump.TaskStateChanged += new TaskEventHandler(Pump_TaskStateChanged);

						SetTransferState(TransferState.Transferring);
						ProcessTaskStateRequest();

						m_Pump.Run();

						if (m_Pump.TaskState == TaskState.Stopped)
						{
							throw new StopRequestedException();	
						}

						SetTransferState(TransferState.Copying);
						ProcessTaskStateRequest();
					}
					finally
					{
						if (srcStream != null)
						{
							Routines.IgnoreExceptions(delegate
							{
								srcStream.Close();
							});
						}

						if (destStream != null)
						{
							Routines.IgnoreExceptions(delegate
							{
								destStream.Close();
							});
						}
					}

					break;
				}

				SetTransferState(TransferState.Tidying);

				destTemp.MoveTo(dest, true);

				///
				/// Aquire an UpdateContext for the attributes
				/// so that all updates to the attributes are
				/// commited in a single operation
				///

				using (dest.Attributes.AquireUpdateContext())
				{
					foreach (string s in this.m_ServiceType.AttributesToTransfer)
					{
						dest.Attributes[s] = m_Source.Attributes[s];
					}
				}

				dest.RenameTo(m_Destination.Address.Name, true);

				SetTransferState(TransferState.Finished);
			}
			finally
			{
				if (m_TransferState != TransferState.Stopped)
				{
					SetTransferState(TransferState.Finished);
				}
			}
		}

		private void Pump_TaskStateChanged(object sender, TaskEventArgs eventArgs)
		{
			lock (this)
			{
				if (eventArgs.TaskState == TaskState.Running
					|| eventArgs.TaskState == TaskState.Paused)
				{
					SetTaskState(eventArgs.TaskState);
				}
			}
		}

		public override void Stop()
		{
			lock (this)
			{
				if (m_Pump != null && m_Pump.TaskState == TaskState.Running)
				{
					m_Pump.Stop();
				}
				else
				{
					base.Stop();
				}
			}
		}

		public override void Pause()
		{
			lock (this)
			{
				if (m_Pump != null && m_Pump.TaskState == TaskState.Running)
				{
					m_Pump.Pause();
				}
				else
				{
					base.Pause();
				}
			}
		}

		public override void Resume()
		{
			lock (this)
			{
				if (m_Pump != null && m_Pump.TaskState == TaskState.Running)
				{
					m_Pump.Resume();
				}
				else
				{
					base.Resume();
				}
			}
		}
	}
}
