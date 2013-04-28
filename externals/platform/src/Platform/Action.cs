namespace Platform
{
	/// <summary>
	/// A subroutine with five parameters
	/// </summary>
	public delegate void Action<in T1, in T2, in T3, in T4, in T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

	/// <summary>
	/// A subroutine with six parameters
	/// </summary>
	public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

	/// <summary>
	/// A subroutine with seven parameters
	/// </summary>
	public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);

	/// <summary>
	/// A subroutine with eight parameters
	/// </summary>
	public delegate void Action<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
}