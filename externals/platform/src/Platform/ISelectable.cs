using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// An interface for objects that can be selected.
	/// </summary>
	public interface ISelectable
	{
		/// <summary>
		/// Gets or sets whether or not the object is selected.
		/// </summary>
		bool Selected
		{
			get;
			set;
		}

		/// <summary>
		/// Selects the current object.
		/// </summary>
		void Select();

		/// <summary>
		/// Deselects the current object.
		/// </summary>
		void Deselect();
	}
}
