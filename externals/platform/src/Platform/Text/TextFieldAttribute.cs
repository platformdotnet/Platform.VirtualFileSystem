using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Text
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class TextFieldAttribute
		: Attribute
	{
		public string Name
		{
			get;
			set;
		}

		public TextFieldAttribute()
		{
		}

		public TextFieldAttribute(string name)
		{
			this.Name = name;
		}
	}
}
