using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Exception used to unravel the stack when a task or operation
	/// is cancelled or stopped.
	/// </summary>
	public class StopRequestedException
		: Exception
	{
		public StopRequestedException()
		{
		}
	}
}
