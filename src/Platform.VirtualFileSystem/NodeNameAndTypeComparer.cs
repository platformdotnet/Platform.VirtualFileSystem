using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem
{
	public class NodeNameAndTypeComparer
		: IComparer<INode>
	{
		public static NodeNameAndTypeComparer Default
		{
			get
			{
				return defaultComparer;
			}
		}
		private static readonly NodeNameAndTypeComparer defaultComparer = new NodeNameAndTypeComparer(StringComparer.CurrentCulture);

		public static NodeNameAndTypeComparer DefaultCaseInsensitive
		{
			get
			{
				return defaultCaseInsensitiveComparer;
			}
		}
		private static readonly NodeNameAndTypeComparer defaultCaseInsensitiveComparer = new NodeNameAndTypeComparer(StringComparer.CurrentCultureIgnoreCase);

		private readonly StringComparer nameComparer;

		private NodeNameAndTypeComparer(StringComparer nameComparer)
		{
			this.nameComparer = nameComparer;
		}

		public virtual int Compare(INode x, INode y)
		{
			if (x.NodeType == y.NodeType)
			{
				return nameComparer.Compare(x.Name, y.Name);
			}
			else if (x.NodeType == NodeType.Directory)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		}
	}
}
