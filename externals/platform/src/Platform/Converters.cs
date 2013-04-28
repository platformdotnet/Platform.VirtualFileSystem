using System;

namespace Platform
{
	public class Converters<I, O>
	{
		public static Converter<I, O> NoConvert
		{
			get
			{
				return noConvert;
			}
		}
		private static readonly Converter<I, O> noConvert = value => (O) (object) value;
	}
}