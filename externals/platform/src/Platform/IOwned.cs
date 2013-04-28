using System;

namespace Platform
{
	/// <summary>
	/// An interface for objects that have an owner.
	/// </summary>
	public interface IOwned
	{
		/// <summary>
		/// Gets the owner of the current object.
		/// </summary>
		object Owner
		{
			get;
		}
	}
}
