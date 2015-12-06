using System;

namespace Platform.VirtualFileSystem
{
	public class DefaultNodeOperationFilter
		: INodeOperationFilter
	{	
		public virtual INode CopyTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator)
		{
			operationPerformed = true;

			return defaultOperator(target, overwrite);
		}

		public virtual INode MoveTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator)
		{
			operationPerformed = true;

			return defaultOperator(target, overwrite);
		}

		public virtual INode Create(INode thisNode, bool createParent, ref bool operationPerformed, Func<bool, INode> defaultOperator)
		{
			operationPerformed = true;

			return defaultOperator(createParent);
		}

		public virtual INode Delete(INode thisNode, ref bool operationPerformed, Func<INode> defaultOperator)
		{
			operationPerformed = true;

			return defaultOperator();
		}

		public virtual INode RenameTo(INode thisNode, string name, bool overwrite, ref bool operationPerformed, Func<string, bool, INode> defaultOperator)
		{
			operationPerformed = true;

			return defaultOperator(name, overwrite);
		}
	}
}
