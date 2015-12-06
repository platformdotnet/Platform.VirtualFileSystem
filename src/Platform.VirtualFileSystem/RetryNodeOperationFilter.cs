using System;

namespace Platform.VirtualFileSystem
{
	public class RetryNodeOperationFilter
		: DefaultNodeOperationFilter
	{
		private const int retryCount = 1;

		public override INode Delete(INode thisNode, ref bool operationPerformed, Func<INode> defaultOperator)
		{
			INode retval = null;

			ActionUtils.ToRetryAction<object>(value => retval = defaultOperator(), retryCount)(null);

			operationPerformed = true;

			return retval;
		}
	}
}
