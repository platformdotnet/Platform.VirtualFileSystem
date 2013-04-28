using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.References
{
	/// <summary>
	/// An interface implemented objects that want to know when a <see cref="Reference{T}"/>
	/// target has been collected.
	/// </summary>
	/// <typeparam name="T">The <see cref="Reference{T}"/> target</typeparam>
	/// <seealso cref="ReferenceQueue{T}"/>
	public interface IReferenceQueue<T>
		where T : class
	{
		/// <summary>
		/// This method is called whenever the garbage collector has collected a <see cref="Reference{T}"/>
		/// target.
		/// </summary>
		/// <param name="reference">The reference whose target has been collected</param>
		void Enqueue(Reference<T> reference);		
	}
}
