using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Platform.Threading
{
	/// <summary>
	/// Class for temporarily changing a thread's priority using the C# using statement
	/// </summary>
	/// <remarks>
	/// The current thread's priority is changed to the provided priority until the
	/// object is disposed.
	/// </remarks>
	public class ThreadPriorityContext
		: IDisposable
	{
		/// <summary>
		/// The original thread priority
		/// </summary>
		private readonly ThreadPriority originalPriority;

		/// <summary>
		/// Constructs a new <see cref="ThreadPriorityContext"/> with the given priority
		/// </summary>
		/// <param name="priority">The priority to change the current thread to</param>
		public ThreadPriorityContext(ThreadPriority priority)
		{
			originalPriority = priority;

			Thread.CurrentThread.Priority = priority;
		}

		/// <summary>
		/// Changes the current thread's priority back to the original value
		/// </summary>
		void IDisposable.Dispose()
		{
			Thread.CurrentThread.Priority = originalPriority;
		}
	}
}
