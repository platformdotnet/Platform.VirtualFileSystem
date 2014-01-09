using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Platform.VirtualFileSystem
{
	public class FileSystemVariablesCollection
		: NameValueCollection
	{
		public FileSystemVariablesCollection()
		{
			this.IsReadOnly = true;
		}

		public FileSystemVariablesCollection(NameValueCollection existingValues)
			: base(existingValues)
		{
			this.IsReadOnly = true;
		}

		public bool CollectionEquals(FileSystemVariablesCollection nameValueCollection)
		{
			return this.ToKeyValue().SequenceEqual(nameValueCollection.ToKeyValue());
		}

		private IEnumerable<KeyValuePair<string, string>> ToKeyValue()
		{
			return this.AllKeys.OrderBy(x => x).Select(x => new KeyValuePair<string, string>(x, this[x]));
		}
	}
}
