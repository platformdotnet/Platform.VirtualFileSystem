using System;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebNodeAttributes
		: AbstractTypeBasedNodeAttributes
	{
		protected AbstractNode node;

		public WebNodeAttributes(AbstractNode node)
			: base(node)
		{
			this.node = node;
		}

		public override DateTime? LastAccessTime
		{
			get
			{
				return DateTime.Now;
			}
		}

		public override DateTime? LastWriteTime
		{
			get
			{
				return DateTime.Now;
			}
		}
	}
}
