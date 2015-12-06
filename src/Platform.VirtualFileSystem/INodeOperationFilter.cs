using System;

namespace Platform.VirtualFileSystem
{	
	public interface INodeOperationFilter
	{
		INode CopyTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator);
		INode MoveTo(INode thisNode, INode target, bool overwrite, ref bool operationPerformed, Func<INode, bool, INode> defaultOperator);
		INode Create(INode thisNode, bool createParent, ref bool operationPerformed, Func<bool, INode> defaultOperator);
		INode Delete(INode thisNode, ref bool operationPerformed, Func<INode> defaultOperator);
		INode RenameTo(INode thisNode, string name, bool overwrite, ref bool operationPerformed, Func<string, bool, INode> defaultOperator);		
	}
}
