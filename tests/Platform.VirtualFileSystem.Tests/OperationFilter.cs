using System;

namespace Platform.VirtualFileSystem.Tests
{
	public class OperationFilter
		: DefaultNodeOperationFilter
	{
		public static int numberOfTimesDeleteCalled;
		public static int numberOfTimesCreateCalled;

		public override INode Delete(INode thisNode, ref bool operationPerformed, Func<INode> defaultOperator)
		{
			numberOfTimesDeleteCalled++;

			return base.Delete(thisNode, ref operationPerformed, defaultOperator);
		}

		public override INode Create(INode thisNode, bool createParent, ref bool operationPerformed, Func<bool, INode> defaultOperator)
		{
			numberOfTimesCreateCalled++;

			return base.Create(thisNode, createParent, ref operationPerformed, defaultOperator);
		}
	}
}
