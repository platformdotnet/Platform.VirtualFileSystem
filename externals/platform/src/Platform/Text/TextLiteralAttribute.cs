using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Text
{
	public class TextLiteralAttribute : Attribute
	{
		public bool IsLiteral
		{
			get; set;
		}
	}
}
