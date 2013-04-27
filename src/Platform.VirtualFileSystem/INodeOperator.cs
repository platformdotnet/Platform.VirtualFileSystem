namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for objects that perform operations on a FileSystemNode.
	/// </summary>
	public interface INodeOperator
	{
		/// <summary>
		/// Gets the name of the operation type this operator supports.
		/// </summary>
		string OperationTypeName
		{
			get;
		}

		/// <summary>
		/// Tells the operator to perform the node operation.
		/// </summary>
		void PerformOperation();
	}
}
