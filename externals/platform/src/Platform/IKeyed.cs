#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Platform
{
	/// <summary>
	/// Interface for objects that contain a <see cref="Key"/> property.
	/// </summary>
	public interface IKeyed
	{
		/// <summary>
		/// The <see cref="Key"/>.
		/// </summary>
		object Key
		{
			get;
		}
	}

	/// <summary>
	/// Interface for objects that contain a <see cref="Key"/> property.
	/// </summary>
	public interface IKeyed<T>
		: IKeyed
	{
		/// <summary>
		/// The <see cref="Key"/>.
		/// </summary>
		new T Key
		{
			get;
		}
	}
}
