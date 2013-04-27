using System;
using System.Collections.Generic;
using Platform;

namespace Platform.VirtualFileSystem.Providers.Shadow
{
	public class ShadowNodeResolutionFilter
		: INodeResolutionFilter
	{
		public virtual string QueryKey { get; set; }

		public ShadowNodeResolutionFilter()
		{
			QueryKey = "shadow";
		}

		public virtual INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache)
		{
			string value;

			canCache = false;
			
			if (this.QueryKey == null)
			{
				value = "true";
			}
			else
			{
				try
				{
					if (address.QueryValues[QueryKey] == null)
					{
						return null;
					}

					value = (string)address.QueryValues[this.QueryKey];
				}
				catch (KeyNotFoundException)
				{
					value = null;
				}
			}

			if (nodeType.Equals(NodeType.File) && value != null && value.ToLower() == "true")
			{
				canCache = true;

				var innerNodeType = nodeType.InnerType;

				if (innerNodeType == null)
				{
					innerNodeType = NodeType.File;
				}

				var query = StringUriUtils.BuildQuery
				(
					address.QueryValues,
					QueryFilter
				);

				var uri = address.AbsolutePath + "?" + query;

				return new ShadowFile((IFile)resolver.Resolve(uri, innerNodeType), address);
			}				
			
			return null;
		}

		internal static bool QueryFilter(Pair<string, string> pair)
		{
			return pair.Key.ToString().ToLower() != "shadow";
		}
	}
}
