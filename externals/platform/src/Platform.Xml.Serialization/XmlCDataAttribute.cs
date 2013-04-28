using System;

namespace Platform.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
	public class XmlCDataAttribute
		: XmlSerializationAttribute
	{
		public bool Enabled
		{
			get;
			set;
		}

		public XmlCDataAttribute()
		{
			this.Enabled = true;
		}

		public XmlCDataAttribute(bool enabled)
		{
			this.Enabled = enabled;
		}
	}
}