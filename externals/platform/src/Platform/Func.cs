using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform
{
	public delegate R Func<T1, T2, T3, T4, T5, R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5);
	public delegate R Func<T1, T2, T3, T4, T5, T6, R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6);
	public delegate R Func<T1, T2, T3, T4, T5, T6, T7, R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7);
	public delegate R Func<T1, T2, T3, T4, T5, T6, T7, T8, R>(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5, T6 v6, T7 v7, T8 t8);
}
