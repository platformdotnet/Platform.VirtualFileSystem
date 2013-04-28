using System;

namespace Platform
{
	/// <summary>
	/// Attribute that allows plugin assemblies to declare that they provide specific type.
	/// </summary>
	/// <remarks>
	/// Application developers can use this attribute to speed up scanning of available
	/// plugin types.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class AssemblyProvidesTypeAttribute
		: Attribute
	{
		public virtual Type ProvidedType
		{
			get;
			set;
		}

		public AssemblyProvidesTypeAttribute(Type providedTypes)
		{
			this.ProvidedType = providedTypes;
		}
	}
}
