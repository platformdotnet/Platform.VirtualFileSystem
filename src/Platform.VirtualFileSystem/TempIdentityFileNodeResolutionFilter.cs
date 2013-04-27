using System;
using System.Collections.Generic;
using Platform.VirtualFileSystem.Providers;
using Platform.VirtualFileSystem.Providers.Shadow;

namespace Platform.VirtualFileSystem
{
	public class TempIdentityFileNodeResolutionFilter
		: AbstractNodeResolutionFilter
	{
		public virtual string QueryKey { get; set; }

		public TempIdentityFileNodeResolutionFilter()
		{
			this.QueryKey = "tempidentity";
		}

		public override INode Filter(ref INodeResolver resolver, ref INodeAddress address, ref NodeType nodeType, out bool canCache)
		{
			string value;

			canCache = false;

			if (this.QueryKey == null)
			{
				return null;
			}
			else
			{
				try
				{
					if (address.QueryValues[this.QueryKey] == null)
					{
						return null;
					}

					value = (string)address.QueryValues[this.QueryKey];
				}
				catch (KeyNotFoundException)
				{
					return null;
				}
			}

			if (nodeType.Equals(NodeType.File))
			{
				canCache = true;

				var query = StringUriUtils.BuildQuery
				(
					address.QueryValues,
					pair => pair.Key.Equals(this.QueryKey, StringComparison.CurrentCultureIgnoreCase)
				);

				var uri = address.AbsolutePath + "?" + query;

				var file = resolver.ResolveFile(uri);
				var service = (ITempIdentityFileService)file.GetService(new TempIdentityFileServiceType(value));

				canCache = true;

				return service.GetTempFile();
			}
					
			return null;
		}
	}
}
