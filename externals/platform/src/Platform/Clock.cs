using System;

namespace Platform
{
	public abstract class Clock
	{
		public abstract double CurrentMilliseconds
		{
			get;
		}
	}
}
