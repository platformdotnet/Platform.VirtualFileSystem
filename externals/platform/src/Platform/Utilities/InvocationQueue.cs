using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using Platform.Collections;

namespace Platform.Utilities
{
	/// <summary>
	/// A task class that is also a blocking queue of <see cref="Action"/> delegates.
	/// The InvocationQueue will dequeue routines as they are enqueued and execute the
	/// routines on a seperate individual thread.  This class can be used as a simple
	/// volatile MessageQueue that can be paused, and stopped just like any other
	/// <see cref="ITask"/>.
	/// </summary>
	public class InvocationQueue
		: BlockingQueueAdapter<Action>, ITask
	{
		/// <summary>
		/// <see cref="ITask.RequestedTaskStateChanged"/>.
		/// </summary>
		public virtual event TaskEventHandler RequestedTaskStateChanged
		{
			add
			{
				taskImplementationHelper.RequestedTaskStateChanged += value;
			}
			remove
			{
				taskImplementationHelper.RequestedTaskStateChanged -= value;
			}
		}
		private readonly AbstractTask.TaskAsyncStateAndImplementationHelper taskImplementationHelper;

		/// <summary>
		/// The used to synchronize task state changes.
		/// </summary>
		public virtual object TaskStateLock
		{
			get
			{
				return taskImplementationHelper.TaskStateLock;
			}
		}
	
		/// <summary>
		/// Creates a new <see cref="InvocationQueue"/>.
		/// </summary>
		public InvocationQueue()
			: this(typeof(LinkedListQueue<>))
		{
		}

		/// <summary>
		/// Creates a new <see cref="InvocationQueue"/> using the given <see cref="queueType"/>
		/// as a backing queue.  The <see cref="queueType"/> must implement <see cref="ILQueue{Action}"/>.
		/// </summary>
		/// <param name="queueType">The type of queue to use</param>
		/// <param name="args">Arguments to the queue's constructor.</param>
        public InvocationQueue(Type queueType, params object[] args)
            : base((ILQueue<Action>)Activator.CreateInstance(queueType.MakeGenericType(typeof(Action)), args))
        {
			this.IdleWaitTime = TimeSpan.FromMilliseconds(250);
			this.TaskAsynchronisity = TaskAsynchronisity.AsyncWithBackgroundThread;
			this.ApartmentState = ApartmentState.Unknown;

			taskImplementationHelper = new AbstractTask.TaskAsyncStateAndImplementationHelper(this);
			
            SetTaskState(TaskState.NotStarted);			
        }

		/// <summary>
		/// Sets the current <see cref="TaskState"/>.
		/// </summary>
		/// <param name="value">The new <see cref="TaskState"/></param>
		protected virtual void SetTaskState(TaskState value)
		{
			taskImplementationHelper.SetTaskState(value);
		}

		/// <summary>
		/// Dequeueing is not supported by the <see cref="InvocationQueue"/>.
		/// </summary>
		/// <returns></returns>
		public override Action Dequeue()
		{
			throw new NotSupportedException();
		}

		#region ITask Members

		/// <summary>
		/// Raised when the <see cref="TaskState"/> has changed.
		/// </summary>
		public virtual event TaskEventHandler TaskStateChanged
		{
			add
			{
				taskImplementationHelper.TaskStateChanged += value;
			}
			remove
			{
				taskImplementationHelper.TaskStateChanged -= value;
			}
		}

		/// <summary>
		/// The <see cref="InvocationQueue"/> does not support any progress indications.
		/// This property will return <see cref="AbstractMeter.Null"/>.
		/// </summary>
		public virtual IMeter Progress
		{
			get
			{
				return AbstractMeter.Null;
			}
		}

		/// <summary>
		/// Starts the <see cref="InvocationQueue"/>.
		/// </summary>
		public virtual void Start()
		{
			taskImplementationHelper.RequestTaskState(TaskState.Running);
		}

		/// <summary>
		/// Pauses the <see cref="InvocationQueue"/>.
		/// </summary>
		public virtual void Pause()
		{
			taskImplementationHelper.RequestTaskState(TaskState.Paused);
		}

		/// <summary>
		/// Unpauses the <see cref="InvocationQueue"/>.
		/// </summary>
		public virtual void Resume()
		{
			taskImplementationHelper.RequestTaskState(TaskState.Running);
		}

		/// <summary>
		/// Stops the <see cref="InvocationQueue"/>.
		/// </summary>
		public virtual void Stop()
		{
			taskImplementationHelper.RequestTaskState(TaskState.Stopped);			
		}

		/// <summary>
		/// Return True.
		/// </summary>
		public virtual bool SupportsStart
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Return True.
		/// </summary>
		public virtual bool SupportsPause
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Return True.
		/// </summary>
		public virtual bool SupportsResume
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Return True.
		/// </summary>
		public virtual bool SupportsStop
		{
			get
			{
				return true;
			}
		}

		#endregion

		/// <summary>
		/// The amount of time the <see cref="InvocationQueue"/> will wait for a new
		/// <see cref="Action"/> to be enqueued before it starts processing
		/// task state change requests (such as calls to <see cref="Pause"/>) and invocating 
		/// the <see cref="DoIdleProcessing"/> event .  Default is 250ms.
		/// </summary>
		public virtual TimeSpan IdleWaitTime
		{
			get;
			set;
		}
        
		/// <summary>
		/// Specifies whether the <see cref="InvocationQueue"/> should invoke the
		/// <see cref="DoIdleProcessing"/> event after every task rather than only
		/// when the <see cref="InvocationQueue"/> is empty.
		/// </summary>
		public bool AlwaysInvokeIdleAfterEveryTask
		{
			get;
			set;
		}

		/// <summary>
		/// An event that is raised when the <see cref="InvocationQueue"/> is not busy.
		/// </summary>
		public virtual event EventHandler DoIdleProcessing;

		/// <summary>
		/// Raises the <see cref="DoIdleProcessing"/> event.
		/// </summary>
		/// <param name="eventArgs"></param>
		protected virtual void OnDoIdleProcessing(EventArgs eventArgs)
		{
			if (this.DoIdleProcessing != null)
			{
				this.DoIdleProcessing(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Do not call this method directly.
		/// Use the <see cref="Start"/> method to start the <see cref="InvocationQueue"/>.
		/// </summary>
		public virtual void Run()
		{   
			try
			{
				taskImplementationHelper.InitializeRun(GetType().Name);

				while (true)
				{
					Action action;

					try
					{
						this.ProcessTaskStateRequest();
					}
					catch (StopRequestedException)
					{
						SetTaskState(TaskState.Stopped);

						return;
					}

					if (!base.TryDequeue(this.IdleWaitTime, out action))
					{
						try
						{
							this.ProcessTaskStateRequest();
						}
						catch (StopRequestedException)
						{
							SetTaskState(TaskState.Stopped);

							return;
						}
						
						this.OnDoIdleProcessing(EventArgs.Empty);

						continue;
					}

					try
					{
						action();
					}
					catch (StopRequestedException)
					{
						SetTaskState(TaskState.Stopped);

						return;
					}
					catch (Exception e)
					{
						Console.Error.WriteLine(e);
					}

					if (this.AlwaysInvokeIdleAfterEveryTask)
					{
						this.OnDoIdleProcessing(EventArgs.Empty);
					}
				}
			}
			catch (Exception)
			{
				SetTaskState(TaskState.Stopped);
			}
			finally
			{
				if (this.TaskState == TaskState.Running)
				{
					SetTaskState(TaskState.Finished);
				}
			}
		}

		/// <summary>
		/// Gets the current <see cref="TaskState"/>.
		/// </summary>
		public virtual TaskState TaskState
		{
			get
			{
				return taskImplementationHelper.TaskState;				
			}
		}
		
		/// <summary>
		///  Gets the currently requested <see cref="TaskState"/>.
		/// </summary>
		public virtual TaskState RequestedTaskState
		{
			get
			{
				return taskImplementationHelper.RequestedTaskState;				
			}
		}

		/// <summary>
		/// Requests the queue to change its state to <see cref="state"/> and waits
		/// for it to change state.  The method does not return until the queue has
		/// entered its new state.  The amount of time the method will take to finish
		/// is dependent on the routine the the queue is currently running.
		/// </summary>
		/// <param name="state">The newly requested <see cref="TaskState"/></param>
		public virtual void RequestTaskState(TaskState state)
		{
			taskImplementationHelper.RequestTaskState(state);
		}

		/// <summary>
		/// Requests the queue to change its state to <see cref="state"/>.
		/// </summary>
		/// <param name="state">The newly requested <see cref="TaskState"/></param>
		/// <param name="timeout">
		/// The amount of time to wait for the <see cref="InvocationQueue"/>  to acquire
		/// the new <see cref="TaskState"/>
		/// </param>
		/// <returns>
		/// True if the <see cref="InvocationQueue"/> entered the new task state before 
		/// the method returns
		/// </returns>
		public virtual bool RequestTaskState(TaskState state, TimeSpan timeout)
		{
			return taskImplementationHelper.RequestTaskState(state, timeout);
		}

		/// <summary>
		/// Call to process the next <see cref="TaskState"/> change request.
		/// </summary>
		protected virtual void ProcessTaskStateRequest()
		{
			taskImplementationHelper.ProcessTaskStateRequest();
		}

		/// <summary>
		/// Stops, Clears and Resets the <see cref="InvocationQueue"/> so that it becomes
		/// just like a new <see cref="InvocationQueue"/>.
		/// </summary>
        public virtual void Reset()
        {
            Stop();            
            Clear();
            SetTaskState(TaskState.NotStarted);
        }

		/// <summary>
		/// Gets the <see cref="Thread"/> that executes the <see cref="Action"/> objects
		/// for this <see cref="InvocationQueue"/>.
		/// </summary>
		/// <returns>A reference to the <see cref="Thread"/></returns>
		public virtual Thread GetTaskThread()
		{
			return taskImplementationHelper.TaskThread;
		}

		/// <summary>
		/// Returns True if the <see cref="InvocationQueue"/>
		/// supports changing to the given <see cref="state"/> in its current state.
		/// </summary>
		/// <param name="state">The state to check</param>
		/// <returns>True if the <see cref="InvocationQueue"/> can change to the given <see cref="state"/></returns>
		public virtual bool CanRequestTaskState(TaskState state)
		{
			return !((state == TaskState.Unknown || state == TaskState.Finished || state == TaskState.NotStarted));
		}

		/// <summary>
		/// Gets or sets how the <see cref="InvocationQueue"/> will run asynchronously.
		/// </summary>
		public virtual TaskAsynchronisity TaskAsynchronisity
		{
			get; set;
		}
		
		/// <summary>
		/// Returns the current type's name.
		/// </summary>
		public virtual string Name
		{
			get
			{
				return GetType().Name;
			}
		}

		public ApartmentState ApartmentState
		{
			get;
			set;
		}
	}
}
