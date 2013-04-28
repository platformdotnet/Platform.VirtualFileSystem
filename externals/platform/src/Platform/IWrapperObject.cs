using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An interface implemented by objects that are wrappers.
	/// </summary>
	/// <typeparam name="T">The type of object being wrapped</typeparam>
	public interface IWrapperObject<T>
	{
		T Wrappee
		{
			get;
		}
	}
}
