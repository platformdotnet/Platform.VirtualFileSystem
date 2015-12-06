using System.Collections;

namespace Platform.VirtualFileSystem
{
	public class AttributeChangeDeterminer
	{
		private readonly INode node;
		private readonly string[] attributeNames;
		private readonly IDictionary attributes;
		
		public AttributeChangeDeterminer(INode node, params string[] attributeNames)
		{
			this.node = node;

			this.attributeNames = attributeNames;
			attributes = System.Collections.Specialized.CollectionsUtil.CreateCaseInsensitiveHashtable();

			MakeUnchanged();
		}

		public virtual bool IsUnchanged()
		{
			return IsUnchanged(attributeNames);
		}

		public virtual bool IsUnchanged(params string[] attributeNames)
		{
			node.Refresh();

			foreach (var name in attributeNames)
			{
				if (!node.Attributes[name].Equals(attributes[name]))
				{
					return false;
				}
			}

			return true;
		}

		public virtual void MakeUnchanged()
		{
			MakeUnchanged(attributeNames);
		}

		public virtual void MakeUnchanged(params string[] attributeNames)
		{
			foreach (var name in attributeNames)
			{
				attributes[name] = node.Attributes[name];
			}
		}
	}
}
