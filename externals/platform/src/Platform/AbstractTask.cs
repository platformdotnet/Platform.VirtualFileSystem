using System;
using System.ComponentModel;
using System.Threading;

namespace Platform
{
    /// <summary>
    /// Provides a base class for objects that can be started, stopped and paused.
    /// The class provides support for accepting and processing messages to perform
    /// task operations such as <see cref="Start()"/> and <see cref="Stop()"/>
    /// </summary>
    public abstract partial class AbstractTask
        : ITask
    {
        #region TaskAsyncStateAndImplementationHelper

        /// <summary>
        /// Helper struct for implementing <see cref="ITask"/>
        /// </summary>
        /// <remarks>
        /// This class is useful for implementers of <see cref="ITask"/> which don't or can't
        /// extend <see cref="AbstractTask"/>.  The class stores state and provides algorithms
        /// for safely transitioning between task states.
        /// </remarks>
        public class TaskAsyncStateAndImplementationHelper
        {
            /// <summary>
            /// <see cref="RequestedTaskState"/>
            /// </summary>
            private TaskState requestedTaskState;

            /// <summary>
            /// 
            /// </summary>
            private object startLock = new object();

            private ITask task;

            /// <summary>
            /// <see cref="TaskState"/>
            /// </summary>
            private TaskState taskState;

            /// <summary>
            /// <see cref="TaskStateLock"/>
            /// </summary>
            private object taskStateLock;

            /// <summary>
            /// <see cref="TaskThread"/>
            /// </summary>
            private Thread taskThread;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="task"></param>
            public TaskAsyncStateAndImplementationHelper(ITask task)
            {
                this.task = task;
                taskState = TaskState.NotStarted;

                requestedTaskState = TaskState.Unknown;
                taskThread = null;
                taskStateLock = new object();

                TaskStateChanged = null;
                RequestedTaskStateChanged = null;
            }

            /// <summary>
            /// TaskState
            /// </summary>
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public TaskState TaskState
            {
                get
                {
                    return taskState;
                }
            }

            /// <summary>
            /// RequestedTaskState
            /// </summary>
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public TaskState RequestedTaskState
            {
                get
                {
                    return requestedTaskState;
                }
            }

            /// <summary>
            /// TaskStateLock
            /// </summary>
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public object TaskStateLock
            {
                get
                {
                    return taskStateLock;
                }
                set
                {
                    taskStateLock = value;
                }
            }

            /// <summary>
            /// TaskThread
            /// </summary>
            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Thread TaskThread
            {
                get
                {
                    return taskThread;
                }
                set
                {
                    taskThread = value;
                }
            }

            /// <summary>
            /// TaskStateChanged Event.
            /// </summary>
            public event TaskEventHandler TaskStateChanged;

            /// <summary>
            /// Raises the TaskStateChanged event.
            /// </summary>
            /// <param name="eventArgs">The <see cref="TaskEventArgs"/> that contains the event data.</param>
            private void OnTaskStateChanged(TaskEventArgs eventArgs)
            {
                if (TaskStateChanged != null)
                {
                    TaskStateChanged(task, eventArgs);
                }
            }

            /// <summary>
            /// RequestedTaskStateChanged Event.
            /// </summary>
            public event TaskEventHandler RequestedTaskStateChanged;

            /// <summary>
            /// Raises the RequestedTaskStateChanged event.
            /// </summary>
            /// <param name="eventArgs">The <see cref="TaskEventArgs"/> that contains the event data.</param>
            private void OnRequestedTaskStateChanged(TaskEventArgs eventArgs)
            {
                if (RequestedTaskStateChanged != null)
                {
                    RequestedTaskStateChanged(task, eventArgs);
                }
            }

            protected internal virtual void SetRequestedTaskState(TaskState value)
            {
                TaskState oldValue;

                oldValue = requestedTaskState;

                if (oldValue != value)
                {
                    requestedTaskState = value;

                    OnRequestedTaskStateChanged(new TaskEventArgs(oldValue, value));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newState"></param>
            public void SetTaskState(TaskState newState)
            {
                TaskState oldState;

                lock (TaskStateLock)
                {
                    oldState = TaskState;

                    taskState = newState;

                    if (requestedTaskState != TaskState.Unknown
                        && newState == requestedTaskState)
                    {
                        SetRequestedTaskState(TaskState.Unknown);
                    }

                    Monitor.PulseAll(TaskStateLock);
                }

                OnTaskStateChanged(new TaskEventArgs(oldState, newState));
            }

            private void Start(TimeSpan waitTimeOut)
            {
                Thread thread;
                TaskState originalState;

                lock (TaskStateLock)
                {
                    if (TaskState != TaskState.NotStarted && TaskState != TaskState.Stopped && TaskState != TaskState.Finished)
                    {
                        throw new InvalidOperationException("Task in invalidate state (" + TaskState + ")");
                    }

                    originalState = TaskState;

                    switch (task.TaskAsynchronisity)
                    {
                        case TaskAsynchronisity.AsyncWithSystemPoolThread:
                            Action routine;

                            routine = delegate
                                      {
                                          taskThread = Thread.CurrentThread;

                                          task.Run();
                                      };

                            routine.BeginInvoke(null, null);

                            break;
                        case TaskAsynchronisity.AsyncWithForegroundThread:
                            thread = new Thread(task.Run);

                            taskThread = thread;

                            thread.SetApartmentState(task.ApartmentState);
                            thread.IsBackground = false;
                            thread.Start();

                            break;
                        case TaskAsynchronisity.AsyncWithBackgroundThread:
                            thread = new Thread(task.Run);

                            taskThread = thread;
                            thread.SetApartmentState(task.ApartmentState);
                            thread.IsBackground = true;
                            thread.Start();

                            break;
                    }
                }

                if (waitTimeOut != TimeSpan.Zero)
                {
                    while (true)
                    {
                        lock (TaskStateLock)
                        {
                            if (TaskState != originalState)
                            {
                                break;
                            }

                            Monitor.Wait(taskStateLock, waitTimeOut);
                        }
                    }
                }
            }

            public bool RequestTaskState(TaskState state)
            {
                return RequestTaskState(state, TimeSpan.FromMilliseconds(-1));
            }

            public bool RequestTaskState(TaskState state, TimeSpan timeout)
            {
                TaskState currentState;
                bool waitForStart = false;

                currentState = TaskState.Unknown;

                try
                {
                    lock (TaskStateLock)
                    {
                        currentState = TaskState;

                        if (!task.CanRequestTaskState(state))
                        {
                            throw new InvalidOperationException("Can't enter TaskState" + state);
                        }

                        if ((state == TaskState.Starting || state == TaskState.Running)
                            && TaskState != TaskState.Starting && TaskState != TaskState.Running
                            && TaskState != TaskState.Paused && TaskState != TaskState.Waiting)
                        {
                            Start(timeout);

                            waitForStart = true;

                            return true;
                        }

                        if (Thread.CurrentThread == TaskThread)
                        {
                            if (TaskState == state)
                            {
                                return true;
                            }

                            SetRequestedTaskState(state);

                            ProcessTaskStateRequest();

                            return true;
                        }

                        if (timeout != TimeSpan.Zero)
                        {
                            if (TaskState != state)
                            {
                                while (true)
                                {
                                    SetRequestedTaskState(state);

                                    Monitor.PulseAll(TaskStateLock);

                                    if (HasAquiredTaskState(state))
                                    {
                                        SetRequestedTaskState(TaskState.Unknown);

                                        break;
                                    }

                                    if (!Monitor.Wait(TaskStateLock, timeout))
                                    {
                                        return false;
                                    }
                                }
                            }

                            return true;
                        }
                        else
                        {
                            if (TaskState != state)
                            {
                                SetRequestedTaskState(state);
                            }

                            return true;
                        }
                    }
                }
                finally
                {
                    if (waitForStart && timeout != TimeSpan.Zero)
                    {
                        TaskUtils.WaitForAnyTaskState
                            (
                            task,
                            timeout,
                            PredicateUtils.ObjectNotEquals(currentState)
                            );
                    }
                }
            }

            public virtual void InitializeRun(string defaultThreadName)
            {
                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = defaultThreadName;
                }

                if (TaskThread == null)
                {
                    TaskThread = Thread.CurrentThread;
                }

                if (TaskState != TaskState.Running)
                {
                    SetTaskState(TaskState.Running);
                }
            }

            public virtual void ProcessTaskStateRequest()
            {
                lock (TaskStateLock)
                {
                    switch (RequestedTaskState)
                    {
                        case TaskState.Paused:

                            SetRequestedTaskState(TaskState.Unknown);

                            SetTaskState(TaskState.Paused);

                            while (true)
                            {
                                Monitor.Wait(TaskStateLock);

                                if (RequestedTaskState == TaskState.Running)
                                {
                                    SetTaskState(TaskState.Running);

                                    break;
                                }
                                else if (RequestedTaskState == TaskState.Stopped)
                                {
                                    throw new StopRequestedException();
                                }
                            }

                            return;

                        case TaskState.Running:

                            SetRequestedTaskState(TaskState.Unknown);

                            SetTaskState(TaskState.Running);

                            return;

                        case TaskState.Stopped:

                            SetRequestedTaskState(TaskState.Unknown);

                            throw new StopRequestedException();

                        default:

                            SetRequestedTaskState(TaskState.Unknown);

                            return;
                    }
                }
            }

            private bool HasAquiredTaskState(TaskState state)
            {
                return TaskState == state
                       || TaskState == TaskState.Finished;
            }
        }

        #endregion

        private TaskAsyncStateAndImplementationHelper implementationHelper;

        /// <summary>
        /// <see cref="InnerRunAction"/>
        /// </summary>
		private Action innerRunAction;

        private TaskAsynchronisity taskAsynchronisity;

        /// <summary>
        /// Create a new AbstractTask
        /// </summary>
        public AbstractTask()
        {
            ApartmentState = ApartmentState.Unknown;
            implementationHelper = new TaskAsyncStateAndImplementationHelper(this);

            innerRunAction = DoRun;
        }

        /// <summary>
        /// InnerRunAction
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual Action InnerRunAction
        {
            get
            {
                return innerRunAction;
            }
            set
            {
                innerRunAction = value;
            }
        }

        #region ITask Members

        public virtual event TaskEventHandler TaskStateChanged
        {
            add
            {
                implementationHelper.TaskStateChanged += value;
            }
            remove
            {
                implementationHelper.TaskStateChanged -= value;
            }
        }

        public virtual event TaskEventHandler RequestedTaskStateChanged
        {
            add
            {
                implementationHelper.RequestedTaskStateChanged += value;
            }
            remove
            {
                implementationHelper.RequestedTaskStateChanged -= value;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual IMeter Progress
        {
            get
            {
                return AbstractMeter.Null;
            }
        }

        public virtual ApartmentState ApartmentState
        {
            get;
            set;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TaskAsynchronisity TaskAsynchronisity
        {
            get
            {
                return taskAsynchronisity;
            }
            set
            {
                taskAsynchronisity = value;
            }
        }

        public virtual void Start()
        {
            RequestTaskState(TaskState.Running);
        }

        public virtual void Pause()
        {
            RequestTaskState(TaskState.Paused);
        }

        public virtual void Resume()
        {
            RequestTaskState(TaskState.Running);
        }

        public virtual void Stop()
        {
            RequestTaskState(TaskState.Stopped);
        }

        public virtual void Run()
        {
            try
            {
                InitializeRun();

                InnerRunAction();
            }
            catch (StopRequestedException)
            {
                SetTaskState(TaskState.Stopped);
            }
            catch (Exception e)
            {
                try
                {
                    if (!(TaskState == TaskState.Stopped || TaskState == TaskState.Finished))
                    {
                        SetTaskState(TaskState.Stopped);
                    }

                    throw;
                }
                finally
                {
                    OnTaskException(new TaskExceptionEventArgs(e));
                }
            }
            finally
            {
                if (!(TaskState == TaskState.Stopped || TaskState == TaskState.Finished))
                {
                    SetTaskState(TaskState.Finished);
                }
            }
        }

        public virtual Thread GetTaskThread()
        {
            return implementationHelper.TaskThread;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TaskState TaskState
        {
            get
            {
                return implementationHelper.TaskState;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual TaskState RequestedTaskState
        {
            get
            {
                return implementationHelper.RequestedTaskState;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool SupportsStart
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool SupportsPause
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool SupportsResume
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual bool SupportsStop
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual object TaskStateLock
        {
            get
            {
                return implementationHelper.TaskStateLock;
            }
        }

        public virtual bool CanRequestTaskState(TaskState taskState)
        {
            return true;
        }

        public virtual void RequestTaskState(TaskState taskState)
        {
            implementationHelper.RequestTaskState(taskState, TimeSpan.FromMilliseconds(-1));
        }

        public virtual bool RequestTaskState(TaskState taskState, TimeSpan timeout)
        {
            return implementationHelper.RequestTaskState(taskState, timeout);
        }

        #endregion

        #region Protected Methods

        protected virtual void ProcessTaskStateRequest()
        {
            implementationHelper.ProcessTaskStateRequest();
        }

        protected virtual void SetTaskState(TaskState value)
        {
            implementationHelper.SetTaskState(value);
        }

        protected virtual void InitializeRun()
        {
            implementationHelper.InitializeRun(GetType().Name);
        }

        #endregion

        /// <summary>
        /// TaskException Event.
        /// </summary>
        public virtual event EventHandler<TaskExceptionEventArgs> TaskException;

        /// <summary>
        /// Raises the TaskException event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="EventHandler<TaskExceptionEventArgs>"/> that contains the event data.</param>
        protected virtual void OnTaskException(TaskExceptionEventArgs eventArgs)
        {
            if (TaskException != null)
            {
                TaskException(this, eventArgs);
            }
        }

        public abstract void DoRun();

        public virtual void SetTaskThread(Thread value)
        {
            implementationHelper.TaskThread = value;
        }

        protected virtual void SetRequestedTaskState(TaskState taskState)
        {
            implementationHelper.SetRequestedTaskState(taskState);
        }

        protected virtual void Sleep(int timeout)
        {
            lock (TaskStateLock)
            {
                Monitor.Wait(TaskStateLock, timeout);
                Monitor.PulseAll(TaskStateLock);
            }
        }

        protected virtual void Sleep(TimeSpan timeout)
        {
            lock (TaskStateLock)
            {
                Monitor.Wait(TaskStateLock, timeout);
                Monitor.PulseAll(TaskStateLock);
            }
        }
    }

    /// <summary>
    /// Class that holds event information for task events.
    /// </summary>
    /// <seealso cref="ITask"/>
    /// <seealso cref="AbstractTask"/>
    public class TaskExceptionEventArgs
        : EventArgs
    {
        /// <summary>
        /// <see cref="Exception"/>
        /// </summary>
        private Exception exception;

        public TaskExceptionEventArgs(Exception exception)
        {
            this.exception = exception;
        }

        /// <summary>
        /// Exception
        /// </summary>
        public virtual Exception Exception
        {
            get
            {
                return exception;
            }
            set
            {
                exception = value;
            }
        }
    }
}