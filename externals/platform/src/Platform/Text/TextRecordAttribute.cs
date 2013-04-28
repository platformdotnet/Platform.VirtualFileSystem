using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Text
{
	[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
	public class TextRecordAttribute
		: Attribute
	{
	}
}
