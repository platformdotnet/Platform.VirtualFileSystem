using System;

namespace Platform
{
	public class DateTimeClock
		: Clock
	{
		public static DateTimeClock Default = new DateTimeClock();

		public override double CurrentMilliseconds
		{
			get
			{
				return DateTime.Now.Ticks / 10000; 
			}
		} 
	}
}
