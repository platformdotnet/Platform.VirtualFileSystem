using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Contains extended operations for the <see cref="Exception"/> class.
	/// </summary>
	public static class ExceptionUtils
	{
		/// <summary>
		/// Checks if a given <see cref="Exception"/> is a system exception
		/// (one of <see cref="ExecutionEngineException"/> or <see cref="OutOfMemoryException"/> or <see cref="StackOverflowException"/>).
		/// </summary>
		/// <param name="e">
		/// The <see cref="Exception"/> to check.
		/// </param>
		/// <remarks>
		/// A system exception is an exception that extends <see cref="ExecutionEngineException"/>
		/// <see cref="OutOfMemoryException"/> or <see cref="StackOverflowException"/>.
		/// This function can be used to prevent swallowing an exception by
		/// catching all exceptions that extend the base <see cref="Exception"/>
		/// class.
		/// </remarks>
		/// <returns>
		/// True if <paramref name="e"/> is a system exception otherwise false.
		/// </returns>
		public static bool IsSystemException(Exception e)
		{
			return e is OutOfMemoryException || e is StackOverflowException;
		}
	}
}
