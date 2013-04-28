using System;
using System.Threading;

namespace Platform
{
	/// <summary>
	/// Represents the state of an <see cref="ITask"/>.
	/// </summary>
	public enum TaskState
	{
		/// <summary>
		/// The task is in an unknown state.
		/// </summary>
		Unknown,

		/// <summary>
		/// The task hasn't been started.
		/// </summary>
		NotStarted,

		/// <summary>
		/// The task is in the process of starting.
		/// </summary>
		/// <remarks>
		/// This state indicates that start has been requested but
		/// that the task has (for whatever reason) not entered a running state.
		/// Tasks running from a thread pool will remain in a starting
		/// state until a free thread is available.
		/// </remarks>
		Starting,

		/// <summary>
		/// The task has finished.
		/// </summary>
		Finished,

		/// <summary>
		/// The task was forcefully stopped.
		/// </summary>
		Stopped,

		/// <summary>
		/// The task is running.
		/// </summary>
		Running,

		/// <summary>
		/// The task is paused.
		/// </summary>
		Paused,

		/// <summary>
		/// The task is waiting for some condition
		/// </summary>
		Waiting
	}

	/// <summary>
	/// Specifies how an <see cref="ITask"/> <see cref="Starts"/> asynchronously.
	/// </summary>
	public enum TaskAsynchronisity
	{
		/// <summary>
		/// Specifies that a task will start in a background thread (or equivalent).
		/// The task will not prevent the process from exiting.
		/// </summary>
		/// <remarks>
		/// Most tasks will create a new background thread.  Some tasks may appear
		/// asynchronous without needing to create a background thread and will
		/// only support this form of asynchronisity.  Tasks that send messages
		/// to an external process or tasks that send messages to a network server 
		/// are an example of tasks that may not require a seperate thread in order
		/// to work asynchronously.
		/// </remarks>
		AsyncWithBackgroundThread,
		
		/// <summary>
		/// Specifies the task will start in a foreground thread.
		/// The task will prevent the process from exiting.
		/// </summary>
		AsyncWithForegroundThread,

		/// <summary>
		/// Specifies the task will start in a <see cref="System.Threading.ThreadPool"/> thread.
		/// The task will not prevent a process from exiting because ThreadPool threads
		/// are background threads.
		/// </summary>
		AsyncWithSystemPoolThread
	}
	/// <summary>
	/// EventHandler delegatge for <see cref="ITask"/>.
	/// </summary>
	public delegate void TaskEventHandler(object sender, TaskEventArgs eventArgs);

	/// <summary>
	/// Stores event information for an <see cref="ITask"/>
	/// </summary>
	public class TaskEventArgs
		 : EventArgs
	{
		/// <summary>
		///  TaskState
		/// </summary>
		public virtual TaskState TaskState
		{
			get
			{
				return this.NewState;
			}
			set
			{
				this.NewState = value;
			}
		}

		public virtual TaskState OldState
		{
			get;
			set;
		}

		public virtual TaskState NewState
		{
			get;
			set;
		}

		public TaskEventArgs(TaskState oldState, TaskState newState)
		{
			this.OldState = oldState;
			this.NewState = newState;
		}
	}

	/// <summary>
	/// Represents a Task that can be run, controlled and metered.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Refer to the documentation for <see cref="ITask.TaskStateLock"/> for details
	/// on how to prevent deadlocks.
	/// </para>
	/// </remarks>
	public partial interface ITask
		: IRunnable, INamed
	{
		/// <summary>
		/// An event that occurs when the task's <see cref="ITask.TaskState"/> changes.
		/// </summary>
		event TaskEventHandler TaskStateChanged;

		/// <summary>
		/// 
		/// </summary>
		event TaskEventHandler RequestedTaskStateChanged;

		/// <summary>
		/// 
		/// </summary>
		IMeter Progress
		{
			get;
		}

		ApartmentState ApartmentState
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the <see cref="TaskAsynchronisity"/> of the task.
		/// </summary>
		/// <remarks>
		/// <para>The <see cref="TaskAsynchronisity"/> determines how a task responds to the
		/// <see cref="Start()"/> method.
		/// </para>
		/// <para>
		/// Most tasks use the <c>Normal</c> <see cref="TaskAsynchronisity"/> which specifies that the
		/// task will run in a background thread that does not prevent the process from exiting.
		/// Most tasks do not allow the <see cref="TaskAsynchronisity"/> to be changed after a task
		/// has started.
		/// </para>
		/// </remarks>
		TaskAsynchronisity TaskAsynchronisity 
		{
			set;
			get;
		}

		/// <summary>
		/// Starts the current task in the background.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The task runs in the background using a thread as defined by
		/// the <see cref="TaskAsynchronisity"/> property.
		/// </para>
		/// <para>
		/// Usually this method invokes the <see cref="Run"/> method from another thread.
		/// Some tasks (especially server clients) may use a different mechanism for running 
		/// a task in the background.
		/// </para>
		/// <para>
		/// This method does not return until the task has transitioned from the 
		/// <c>NotStarted <see cref="TaskState"/></c>.  This does not mean that the task
		/// will always be in a <c>Running</c> <see cref="TaskState"/>when the call returns
		/// as some tasks may start and stop before this method returns.
		/// </para>
		/// <para>
		/// Call the <c>RequestState(TaskState.Running, false)</c> to start a task without
		/// waiting for it to transition from the <see cref="TaskState"/>.NotStarted state.
		/// </para>
		/// </remarks>
		/// <see cref="Run"/>
		/// <seealso cref="TaskAsynchronisity"/>
		void Start();
		
		/// <summary>
		/// Pause the task.
		/// </summary>
		/// <remarks>
		/// This method does not return until the task has entered the <c>Paused</c>,
		/// <c>Finished</c> or <c>Stopped</c> <see cref="TaskState"/>.
		/// Refer to the documetnation for <see cref="TaskStateLock"/> on information how to 
		/// avoiding deadlocks.
		/// </remarks>
		void Pause();

		/// <summary>
		/// Resume the task.
		/// </summary>
		/// <remarks>
		/// This method does not return until the task has entered the <c>Running</c>,
		/// <c>Finished</c> or <c>Stopped</c> <see cref="TaskState"/>.
		/// Refer to the documetnation for <see cref="TaskStateLock"/> on information how to avoiding 
		/// deadlocks.
		/// </remarks>
		void Resume();

		/// <summary>
		/// Stop the task.
		/// </summary>
		/// <remarks>
		/// This method does not return until the task has entered the <c>Stopped</c>
		/// or <c>Finished</c> TaskState.
		/// Refer to the documetnation for <see cref="TaskStateLock"/> on information how to avoiding 
		/// deadlocks.
		/// </remarks>
		void Stop();

		/// <summary>
		/// Runs the task in the current thread.
		/// </summary>
		/// <remarks>
		/// Use the <see cref="Start()"/> method in combination with the
		/// <see cref="TaskAsynchronisity"/> property if you want to run
		/// the task in the background.
		/// <para>
		/// This method does not return until the task has finished or
		/// is stopped.
		/// </para>
		/// </remarks>
		new void Run();

		Thread GetTaskThread();

		/// <summary>
		/// Get the current state of the task.
		/// </summary>
		/// <remarks>
		/// This property returns the current state of the task.
		/// </remarks>
		TaskState TaskState
		{
			get;
		}

		TaskState RequestedTaskState
		{
			get;
		}

		#region Supports Queries

		bool SupportsStart
		{
			get;
		}

		bool SupportsPause
		{
			get;
		}

		bool SupportsResume
		{
			get;
		}

		bool SupportsStop
		{
			get;
		}

		#endregion

		/// <summary>
		/// Lock for the task state.
		/// </summary>
		/// <remarks>
		/// Lock the monitor on this object in order to prevent a task from
		/// transitioning into a new state.  
		/// <para>
		/// Care must be taken to prevent deadlocks.  Some tasks may lock the
		/// monitor when raising an event.  If a UI thread is waiting on the
		/// TaskStateLock (e.g. it has requested a the task to pauseand is waiting 
		/// for the task to pause before returning) and the task fires
		/// an event whose handler tries to invoke a method on the
		/// UI thread a deadlock will result.  This can be solved by either
		/// making requests to the task non-blocking using the 
		/// <see cref="RequestTaskState(TaskState, bool)"/> method or by 
		/// asynchronously invoking on the UI thread from event handlers (i.e.
		/// using <c>BeginInvoke()</c> instead of <c>Invoke()</c> in the case of
		/// </para>
		/// </remarks>
		object TaskStateLock
		{
			get;
		}

		/// <summary>
		/// Checks if the Task can enter into the provided <paramref name="taskState"/>.
		/// </summary>
		/// <param name="taskState">
		/// The <see cref="TaskState"/> to check against.
		/// </param>
		/// <returns>
		/// True if the task can enter the provided <paramref name="taskState"/>.
		/// </returns>
		bool CanRequestTaskState(TaskState taskState);

		/// <summary>
		/// Requests that a task enter the given <typeparamref name="taskState"/>.
		/// </summary>
		/// <param name="taskState">
		/// The <see cref="TaskState"/> to enter.
		/// </param>
		void RequestTaskState(TaskState taskState);

		/// <summary>
		/// Requests that a task enter the given<typeparamref name="taskState"/>.
		/// </summary>
		/// <param name="taskState">
		/// The <see cref="TaskState"/> to enter.
		/// </param>
		/// <param name="timeout">
		/// The amount of time to wait for the task to enter the new <paramref name="taskState"/>.
		/// A total timeout value of <c>-1 milliseconds</c> represents infinite.
		/// </param>
		/// <returns>
		/// True if the task successfully entered the <see cref="taskState"/> before
		/// the <paramref name="timeout"/> otherwise False.
		/// </returns>
		bool RequestTaskState(TaskState taskState, TimeSpan timeout);
	}
}
