using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public static class NodeAttributes
	{
		public const string CreationTime = "CreationTime";
		public const string LastAccessTime = "LastAccessTime";
		public const string LastWriteTime = "LastWriteTime";
		public const string Exists = "Exists";

		public static void Copy(INodeAttributes source, INodeAttributes destination)
		{
			foreach (Pair<string, object> attributePair in source)
			{
				if (attributePair.Value != null)
				{
					destination[attributePair.Name] = attributePair.Value;
				}
			}
		}
	}
}
