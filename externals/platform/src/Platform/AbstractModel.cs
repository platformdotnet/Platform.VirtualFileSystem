using System;

namespace Platform
{
	/// <summary>
	/// Base class for all <see cref="IModel"/> implementations
	/// </summary>
	public abstract class AbstractModel
		: MarshalByRefObject, IModel
	{
		/// <summary>
		/// An event that is raised when a major change occurs in the model.
		/// </summary>
		public virtual event EventHandler MajorChange;

		/// <summary>
		/// Raises the <see cref="MajorChange"/> event
		/// </summary>
		protected virtual void OnMajorChange()
		{
			if (this.MajorChange != null)
			{
				this.MajorChange(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets the owner of the current object
		/// </summary>
		public virtual object Owner
		{
			get
			{
				return null;
			}
		}
	}
}
