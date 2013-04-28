using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Provides extension methods and static utility methods for <c>ITasks</c> objects.
	/// </summary>
	public static class TaskUtils
	{
		public static void WaitForFinish(this ITask task)
		{
			task.WaitForAnyTaskState(TaskState.Finished);
		}

		/// <summary>
		/// Waits for a task to reach one or more given states
		/// </summary>
		/// <param name="task">The task to wait on</param>
		/// <param name="taskStates">The task states to wait for</param>
		public static void WaitForAnyTaskState(this ITask task, params TaskState[] taskStates)
		{
			WaitForAnyTaskState(task, value => Array.IndexOf(taskStates, value) >= 0);
		}

		/// <summary>
		/// Waits for a task to reach one or more given states
		/// </summary>
		/// <param name="task">The task to wait on</param>
		/// <param name="timeout">A timeout</param>
		/// <param name="taskStates">The task states to wait for</param>
		/// <returns>True if the task reached any of the given states or false if the timeout period occured first</returns>
		public static bool WaitForAnyTaskState(this ITask task, TimeSpan timeout, params TaskState[] taskStates)
		{
			return WaitForAnyTaskState(task, timeout, value => Array.IndexOf(taskStates, value) >= 0);
		}

		/// <summary>
		/// Waits for a task to reach a given state
		/// </summary>
		/// <param name="task">The task to wait on</param>
		/// <param name="acceptState">A predicate that validates for a given state</param>
		public static void WaitForAnyTaskState(this ITask task, Predicate<TaskState> acceptState)
		{
			WaitForAnyTaskState(task, TimeSpan.FromMilliseconds(-1), acceptState);
		}

		/// <summary>
		/// Waits for a task to reach a given state
		/// </summary>
		/// <param name="task">The task to wait on</param>
		/// <param name="acceptState">A predicate that validates for a given state</param>
		/// <param name="timeout">A timeout</param>
		public static bool WaitForAnyTaskState(this ITask task, TimeSpan timeout, Predicate<TaskState> acceptState)
		{
			object obj;

			if (acceptState(task.TaskState))
			{
				return true;
			}

			obj = new object();

			task.TaskStateChanged += delegate
			{
				lock (obj)
				{
					Monitor.PulseAll(obj);
				}
			};

			for (;;)
			{
				lock (obj)
				{
					if (acceptState(task.TaskState))
					{
						return true;
					}
					else
					{
						if (!Monitor.Wait(obj, timeout))
						{
							return false;
						}
					}
				}
			}
		}
	}
}
