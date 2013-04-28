using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// A class that contains a value and allows it to be set and get.
	/// This class is useful for holding structures or immutable values (such as strings)
	/// so that they can be passed to methods that can change them.
	/// </summary>
	/// <typeparam name="T">
	/// The type of value to contain.
	/// </typeparam>
	public class ValueBox<T>
		: IValued<T>
	{
		/// <summary>
		/// Gets or sets the contained value.
		/// </summary>
		public virtual T Value
		{
			get; set;
		}

		/// <summary>
		/// Gets the contained value.
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.Value;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="ValueBox{T}"/> with the default
		/// value for <see cref="T"/>.
		/// </summary>
		public ValueBox()
			: this(default(T))
		{
		}

		/// <summary>
		/// Constructs a new <see cref="ValueBox{T}"/> with the given value.
		/// </summary>
		/// <param name="value">The value the <see cref="ValueBox{T}"/> should contain.</param>
		public ValueBox(T value)
		{
			this.Value = value;
		}

		/// <summary>
		/// Returns the <see cref="Value"/> as a string by calling <see cref="Convert.ToString(object)"/>
		/// </summary>
		/// <returns>A string representation of <see cref="Value"/></returns>
		public override string ToString()
		{
			return Convert.ToString(this.Value);
		}
	}
}
