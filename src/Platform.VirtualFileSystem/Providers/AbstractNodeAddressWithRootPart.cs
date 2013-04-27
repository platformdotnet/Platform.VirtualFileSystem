using System;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public abstract class AbstractNodeAddressWithRootPart
		: AbstractNodeAddress
	{
		public virtual string RootPart
		{
			get
			{
				return this.rootPart;
			}
		}
		private readonly string rootPart;

		protected AbstractNodeAddressWithRootPart(string scheme, string rootPart, string absolutePath, string query)
			: base(scheme, absolutePath, query)
		{
			this.rootPart = rootPart;
		}

		public virtual string AbsolutePathIncludingRootPart
		{
			get
			{
				if (this.absolutePathIncludingRootPart == null)
				{
					lock (this)
					{
						if (this.absolutePathIncludingRootPart == null)
						{
							this.absolutePathIncludingRootPart = this.rootPart + this.AbsolutePath;
						}
					}
				}

				return this.absolutePathIncludingRootPart;
			}
		}
		private volatile string absolutePathIncludingRootPart;

		public virtual INodeAddress CreateAsRoot()
		{
			return CreateAsRoot(this.Scheme);
		}

		public abstract INodeAddress CreateAsRoot(string scheme);
	}
}
