using System;

namespace Platform
{
	/// <summary>
	/// Interface implemented by classes which support a readonly <c>Value</c> property.
	/// </summary>
	public interface IValued
	{
		object Value
		{
			get;
		}
	}

	/// <summary>
	/// Interface implemented by classes which support a readonly <c>Value</c> property.
	/// </summary>
	public interface IValued<T>
		: IValued
	{
		new T Value
		{
			get;
		}
	}
}
