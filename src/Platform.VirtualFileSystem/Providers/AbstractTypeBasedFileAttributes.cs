namespace Platform.VirtualFileSystem.Providers
{
	public class AbstractTypeBasedFileAttributes
		: AbstractTypeBasedNodeAttributes, IFileAttributes
	{
		private INode node;

		public AbstractTypeBasedFileAttributes(INode node)
			: base(node)
		{
			this.node = node;
		}
				
		[NodeAttribute]
		public virtual long? Length
		{
			get
			{
				return null;
			}
		}

		IFileAttributes IFileAttributes.Refresh()
		{
			return (IFileAttributes)this.Refresh();
		}

		public override object this[string item]
		{
			get
			{
				if (item.EqualsIgnoreCase("length"))
				{
					return this.Length;
				}

				return base[item];
			}
			set
			{
				if (item.EqualsIgnoreCase("length"))
				{
					return;
				}

				base[item] = value;
			}
		}
	}
}
