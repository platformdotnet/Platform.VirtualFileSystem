using System;
using Platform.Text;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkNodeAddress
		: LayeredNodeAddress
	{
		protected internal NetworkNodeAddress(string scheme, string userName, string password, string serverName, int? port, string upperLayerUri, string rootPart, string path, string query, bool exposeAsNonComplexAddress)
			: base(scheme, userName, password, serverName, port, upperLayerUri, rootPart, path, query, exposeAsNonComplexAddress)
		{
		}

		public virtual string RemoteUri
		{
			get
			{
				if (this.remoteUri == null)
				{
					lock (this)
					{
						this.remoteUri = FunctionUtils.VolatileAssign(() => StringUriUtils.Combine(this.InnerUri, TextConversion.ToReEscapedHexString(this.PathAndQuery, TextConversion.IsStandardUrlEscapedChar)));
					}
				}

				return this.remoteUri;
			}
		}
		private string remoteUri;
		
		protected override INodeAddress CreateAddress(string scheme, string userName, string password, string serverName, int? port, string upperLayerUri, string rootPart, string path, string query, bool exposeAsNonComplexAddress)
		{
			return new NetworkNodeAddress(scheme, userName, password, serverName, port, upperLayerUri, rootPart, path, query, exposeAsNonComplexAddress);
		}

		public new static NetworkNodeAddress Parse(string uri)
		{
			var address = Parse<NetworkNodeAddress>(uri);

			return address;
		}

		public static NetworkNodeAddress CreateAsRoot(string scheme, NetworkNodeAddress modelAddress)
		{
			return CreateAsRoot(scheme, modelAddress, false);
		}

		public static NetworkNodeAddress CreateAsRoot(string scheme, NetworkNodeAddress modelAddress, bool exposeAsNonComplexAddress)
		{
			var upperLayerUri = StringUriUtils.Combine(modelAddress.InnerUri, modelAddress.AbsolutePath);

			var address = new NetworkNodeAddress(scheme, modelAddress.UserName, modelAddress.Password,
			                                                    modelAddress.ServerName, modelAddress.Port, upperLayerUri, "", "/", "", exposeAsNonComplexAddress);

			return address;
		}

		public static NetworkNodeAddress CreateAddressFromRemoteUri(NetworkNodeAddress modelAddress, string remoteUri)
		{
			var x = remoteUri.IndexOf(modelAddress.InnerUri, StringComparison.CurrentCultureIgnoreCase);

			if (x < 0)
			{
				throw new ArgumentException();
			}

			string s;

			s = remoteUri.Substring(x + modelAddress.InnerUri.Length);

			if (s[0] != '/')
			{
				s = '/' + s;
			}

			return (NetworkNodeAddress)modelAddress.ResolveAddress(s);
		}
	}
}
