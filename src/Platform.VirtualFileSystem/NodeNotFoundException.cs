using System;
using System.IO;

namespace Platform.VirtualFileSystem
{
	public class NodeNotFoundException
		: IOException
	{
		public virtual INodeAddress NodeAddress { get; set; }

		public virtual NodeType NodeType { get; set; }

		public virtual string Uri { get; set; }

		public NodeNotFoundException(string uri)
			: this(null, null)
		{
			this.Uri = uri;
		}

		public NodeNotFoundException()
			: this(null, null)
		{
		}

		public NodeNotFoundException(INodeAddress nodeAddress)
			: this(nodeAddress, null)
		{
		}

		public NodeNotFoundException(INodeAddress nodeAddress, NodeType nodeType)
			: this(nodeAddress, nodeType, null)
		{
		}

		public NodeNotFoundException(INodeAddress nodeAddress, NodeType nodeType, Exception innerException)
			: base(nodeAddress != null ? "VirtualFileSystem item not found: " + nodeAddress.ToString() : "", innerException)
		{
			NodeType = nodeType;
			NodeAddress = nodeAddress;

			if (NodeAddress != null)
			{
				Uri = NodeAddress.Uri;
			}
		}

		public override string ToString()
		{
			return "NodeNotFound: " + Uri + Environment.NewLine + base.ToString();
		}
	}
}
