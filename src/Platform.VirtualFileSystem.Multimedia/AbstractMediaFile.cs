using System;
using System.Threading;
using Platform;
using Platform.Threading;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Multimedia
{
	public abstract class AbstractMediaFile
		: FileWrapper, IMediaFile
	{
		public AbstractMediaFile(IFile file)
			: base(file)
		{
			this.ApartmentState = ApartmentState.Unknown;
		}

		~AbstractMediaFile()
		{
			Close();	
		}

		public virtual void FadeToLevel(VolumeLevel targetLevel, TimeSpan time)
		{
			const int steps = 50;

			var startLevel = this.VolumeLevel.Average;
			var sleep = new TimeSpan(time.Ticks / steps);
			var delta = (targetLevel.Average - startLevel) / steps;
			var newLevel = startLevel + delta;

			if (newLevel < 0)
			{
				newLevel = 0;
			}

			try
			{
				for (var i = 0; i < steps; i++)
				{
					this.VolumeLevel = new VolumeLevel((int) newLevel, (int) newLevel);

					try
					{
						checked
						{
							newLevel += delta;
						}
					}
					catch (OverflowException)
					{
						newLevel = targetLevel.Average;
					}

					Thread.Sleep(sleep);
				}

				this.VolumeLevel = targetLevel;
			}
			catch (Exception)
			{
			}
		}

		#region IMediaFile Members

		public virtual event EventHandler Opened;

		public virtual event EventHandler Closed;

		protected virtual void OnClosed()
		{
			if (Closed != null)
			{
				Closed(this, EventArgs.Empty);
			}
		}

		protected virtual void OnOpened()
		{
			if (Opened != null)
			{
				Opened(this, EventArgs.Empty);
			}
		}

		public virtual IMeter PlaybackMeter
		{
			get
			{
				return this.Progress;
			}
		}

		public virtual TimeSpan PlaybackPosition
		{
			get
			{
				return (TimeSpan)this.Progress.CurrentValue;
			}
			set
			{
				Seek(value);
			}
		}

		public virtual TimeSpan PlaybackLength
		{
			get
			{
				return (TimeSpan)this.Progress.MaximumValue;
			}
		}

		public abstract void Seek(TimeSpan position);

		public override NodeType NodeType
		{
			get
			{
				return new MediaFileNodeType(MediaFileType.Unknown);
			}
		}


		public virtual void Play()
		{
			Start();
		}

		#endregion
		
		#region ITask Members

		public virtual bool SupportsStart
		{
			get
			{
				return true;
			}
		}

		public abstract object TaskStateLock
		{
			get;
		}
		
		public virtual void Start()
		{
			RequestTaskState(TaskState.Running);
		}

		public virtual void RequestTaskState(TaskState taskState)
		{
			RequestTaskState(taskState, new TimeSpan(-1));
		}

		public abstract bool RequestTaskState(TaskState taskState, TimeSpan timeout);
		
		public virtual void Resume()
		{
			RequestTaskState(TaskState.Running);
		}

		public virtual void Pause()
		{
			RequestTaskState(TaskState.Paused);
		}

		public abstract TaskState TaskState
		{
			get;
		}

		public virtual bool SupportsPause
		{
			get
			{
				return true;
			}
		}

		public virtual bool SupportsStop
		{
			get
			{
				return true;
			}
		}

		public virtual bool SupportsResume
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// TaskStateChanged Event.
		/// </summary>
		public virtual event TaskEventHandler TaskStateChanged;

		/// <summary>
		/// Raises the TaskStateChanged event.
		/// </summary>
		/// <param name="eventArgs">The <see cref="TaskEventArgs"/> that contains the event data.</param>
		protected virtual void OnTaskStateChanged(TaskEventArgs eventArgs)
		{
			if (TaskStateChanged != null)
			{
				TaskStateChanged(this, eventArgs);
			}
		}

		public virtual void Stop()
		{
			RequestTaskState(TaskState.Stopped);
		}

		public abstract IMeter Progress
		{
			get;
		}

		#endregion

		public virtual void Run()
		{
			Play();

			TaskUtils.WaitForAnyTaskState(this, PredicateUtils.ObjectEqualsAny(TaskState.Finished, TaskState.Stopped));
		}

		public abstract void Open();

		public abstract void Close();

		public virtual void WaitForFinish()
		{
			WaitForFinish(Timeout.Infinite);
		}
		
		public virtual bool WaitForFinish(int timeout)
		{
			return WaitForFinish(TimeSpan.FromMilliseconds(timeout));
		}

		public virtual bool WaitForFinish(TimeSpan timeout)
		{
			lock (this.TaskStateLock)
			{
				TaskState taskState = this.TaskState;

				if (taskState == TaskState.Unknown
					|| taskState == TaskState.NotStarted
					|| taskState == TaskState.Stopped)
				{
					return true;
				}
			}

			return this.WaitForAnyTaskState(timeout, PredicateUtils.ObjectEqualsAny(TaskState.Finished, TaskState.Stopped));
		}

		public virtual void WaitForPlaybackPosition(TimeSpan position)
		{
			WaitForPlaybackPosition(position, Timeout.Infinite);
		}

		public virtual bool WaitForPlaybackPosition(TimeSpan position, int timeout)
		{
			return WaitForPlaybackPosition(position, TimeSpan.FromMilliseconds(timeout));
		}

		public virtual bool WaitForPlaybackPosition(TimeSpan position, TimeSpan timeout)
		{
			return GetWaitHandleForPlaybackPosition(position).WaitOne(timeout, false);
		}

		protected virtual WaitHandle GetWaitHandleForPlaybackPosition(TimeSpan position)
		{
			return new MeterValueEvent<TimeSpan>(this.Progress,
				delegate(TimeSpan value) { return value >= position; }).WaitHandle;
		}

		public abstract VolumeLevel VolumeLevel
		{
			get;
			set;
		}

		public virtual bool CanRequestTaskState(TaskState taskState)
		{
			return true;
		}

		public abstract Thread GetTaskThread();

		/// <summary>
		/// TaskAsynchronisity 
		/// </summary>
		public virtual TaskAsynchronisity  TaskAsynchronisity 
		{
			get
			{
				return m_TaskAsynchronisity ;
			}
			set
			{
				m_TaskAsynchronisity  = value;
			}
		}
		/// <summary>
		/// <see cref="TaskAsynchronisity "/>
		/// </summary>
		private TaskAsynchronisity  m_TaskAsynchronisity ;

		/// <summary>
		/// RequestedTaskState
		/// </summary>
		public abstract TaskState RequestedTaskState
		{
			get;
		}

		#region ITask Members


		/// <summary>
		/// RequestedTaskStateChanged Event.
		/// </summary>
		public virtual event TaskEventHandler RequestedTaskStateChanged;

		/// <summary>
		/// Raises the RequestedTaskStateChanged event.
		/// </summary>
		/// <param name="eventArgs">The <see cref="TaskEventArgs"/> that contains the event data.</param>
		protected virtual void OnRequestedTaskStateChanged(TaskEventArgs eventArgs)
		{
			if (RequestedTaskStateChanged != null)
			{
				RequestedTaskStateChanged(this, eventArgs);
			}
		}
	
		#endregion

		public ApartmentState ApartmentState
		{
			get;
			set;
		}
	}
}
