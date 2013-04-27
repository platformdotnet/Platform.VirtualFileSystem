using System;
using System.Collections.Generic;
using System.Text;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers.View
{
	public class ViewNodeAddress
		: AbstractNodeAddress
	{
		public ViewNodeAddress(string scheme, string absolutePath, string query)
			: base(scheme, absolutePath, query)
		{
		}

		public static ViewNodeAddress Parse(string uri)
		{
			int x;
			string scheme, absolutePath, query;

			x = uri.IndexOf("://");

			scheme = uri.Substring(0, x);
			absolutePath = uri.Substring(x + 3);

			if (absolutePath.Length > 0 && absolutePath[0] != '/')
			{
				throw new MalformedUriException(uri, "Path must be absolute");
			}

			if ((x = absolutePath.IndexOf('?')) > 0)
			{
				query = absolutePath.Substring(x + 1);
				absolutePath = absolutePath.Substring(0, x);
			}
			else
			{
				query = "";
			}

			absolutePath = TextConversion.FromEscapedHexString(absolutePath);

			return new ViewNodeAddress(scheme, absolutePath, query);
		}

		protected override INodeAddress CreateAddress(string absolutePath, string query)
		{
			if (this.GetType() == typeof(ViewNodeAddress))
			{
				return new ViewNodeAddress(this.Scheme, absolutePath, query);
			}
			else
			{
				return (ViewNodeAddress)Activator.CreateInstance(GetType(), this.Scheme, absolutePath, query);
			}
		}

		protected override string GetRootUri()
		{
			return this.Scheme + "://";
		}
	}
}
