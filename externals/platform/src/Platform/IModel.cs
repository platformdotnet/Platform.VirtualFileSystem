using System;

namespace Platform
{
	/// <summary>
	/// A base interface for model classes (classes that contain state).
	/// </summary>
	public interface IModel
		: IOwned
	{
		/// <summary>
		/// An event that is raised when a major change occurs in the model.
		/// </summary>
		event EventHandler MajorChange;
	}
}
