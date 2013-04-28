using System;
using System.Threading;
using Platform;
using System.ComponentModel;

namespace Platform
{
	/// <summary>
	/// Base class for all <see cref="IRunnable"/> implementations
	/// </summary>
	public abstract class AbstractRunnable
		: MarshalByRefObject, IRunnable
	{
		/// <summary>
		/// Performs the <c>Run</c> operation
		/// </summary>
		public abstract void Run();
	}
}