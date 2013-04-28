using System;
using System.Collections;
using System.Collections.Specialized;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlSerializerNamespaces.
	/// </summary>
	public class XmlSerializerNamespaces
	{
		private readonly IDictionary prefixes;
		private readonly IDictionary namespaces;

		public XmlSerializerNamespaces()
		{
			namespaces = CollectionsUtil.CreateCaseInsensitiveHashtable();
			prefixes = CollectionsUtil.CreateCaseInsensitiveHashtable();
		}

		internal IEnumerable GetPrefixes()
		{
			return prefixes.Values;
		}

		public void Add(string prefix, string ns)
		{
			namespaces[prefix] = ns;
			prefixes[ns] = prefix;
		}

		public string GetNamespace(string prefix)
		{
			return (string)namespaces[prefix];
		}

		public string GetPrefix(string ns)
		{
			return (string)prefixes[ns];
		}
	}
}
