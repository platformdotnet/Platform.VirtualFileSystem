using System;

namespace Platform.VirtualFileSystem.DataFile
{
	public class DataFileResolutionFilter
		: INodeResolutionFilter
	{
		public DataFileResolutionFilter()
		{
			
		}

		public INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache)
		{
			if (nodeType.GetType().IsGenericType
				&& nodeType.GetType().GetGenericTypeDefinition() == typeof(DataFileNodeType<>))
			{
				var innerNodeType = nodeType.InnerType ?? NodeType.File;
				
				var file = (IFile)resolver.Resolve(address.PathAndQuery, innerNodeType);

				var dataFileType = typeof(DataFile<>);

				dataFileType = dataFileType.MakeGenericType(nodeType.GetType().GetGenericArguments());

				var dataFile = (INode)Activator.CreateInstance(dataFileType, file, nodeType);

				canCache = true;

				return dataFile;
			}
			else
			{
				canCache = false;

				return null;
			}
		}
	}
}
