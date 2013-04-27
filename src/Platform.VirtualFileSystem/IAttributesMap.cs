using System;

namespace Platform.VirtualFileSystem
{
	public interface IAttributesMap
	{
		void SetAttribute(string name, object value);
		object GetAttribute(string name);

		object this[string name]
		{
			get;
			set;
		}
	}
}
