#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Platform
{
	public interface IKeyedValued
		: IKeyed, IValued
	{
	}

	public interface IKeyedValued<K, V>
		: IKeyed<K>, IValued<V>
	{
	}
}
