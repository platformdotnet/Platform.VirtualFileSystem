using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public class LayeredNodeAddress
		: AbstractNodeAddressWithRootPart
	{	
		public override string InnerUri
		{
			get
			{
				return this.innerUri;
			}
		}
		private readonly string innerUri;

		public virtual string UserName { get; set; }

		public virtual string Password { get; set; }

		public virtual int? Port { get; set; }

		public virtual string ServerName { get; set; }

		private static readonly Regex networkPartRegex;

		static LayeredNodeAddress()
		{
			networkPartRegex = new Regex
			(
				@"(((?<UserName>\w*) (\:(?<Password>\w*))?)\@)? ((?<ServerName>([a-zA-Z0-9\.]*)) (\:(?<Port>\d*))?)? (?<Rest>.*)",
				RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
			);
		}

		private readonly bool exposeAsNonComplexAddress = false;

		public LayeredNodeAddress(string scheme, string userName, string password, string serverName, int? port, string upperLayerUri, string rootPart, string path, string query, bool exposeAsNonComplexAddress)
			: base(scheme, rootPart, path, query)
		{
			this.innerUri = upperLayerUri;
			this.UserName = userName;
			this.Password = password;
			this.ServerName = serverName;
			this.Port = port;
			this.exposeAsNonComplexAddress = exposeAsNonComplexAddress;
		}

		public static int ParseParentUri(string uri, int startIndex)
		{
			if (uri[startIndex] != '[')
			{
				throw new MalformedUriException();
			}

			startIndex++;

			while (true)
			{
				if (startIndex >= uri.Length)
				{
					throw new MalformedUriException();
				}

				if (uri[startIndex] == '[')
				{
					startIndex = ParseParentUri(uri, startIndex);
				}
				else if (uri[startIndex] == ']')
				{
					return startIndex;
				}

				startIndex++;
			}
		}

		public static T Parse<T>(string uri)
		{
			int? port;
			string scheme, userName, password, serverName, parentUri, path, query;

			Parse(uri, out scheme, out userName, out password, out serverName, out port, out parentUri, out path, out query);

			return (T)CreateAddressDynamically(typeof(T), scheme, userName, password, serverName, port, parentUri, "", path, query, false);
		}

		private static INodeAddress CreateAddressDynamically(Type type, string scheme, string userName, string password, string serverName, int? port, string upperLayerUri, string rootPart, string path, string query, bool exposeAsNonComplexAddress)
		{
			if (type == typeof(LayeredNodeAddress))
			{
				return new LayeredNodeAddress(scheme, userName, password, serverName, port, upperLayerUri, "", path, query, exposeAsNonComplexAddress);
			}
			else
			{
				var args = new object[] { scheme, userName, password, serverName, port, upperLayerUri, "", path, query, exposeAsNonComplexAddress };

				return (INodeAddress)Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
			}
		}

		public static LayeredNodeAddress Parse(string uri)
		{
			int? port;
			string scheme, userName, password, serverName, parentUri, path, query;

			Parse(uri, out scheme, out userName, out password, out serverName, out port, out parentUri, out path, out query);

			return new LayeredNodeAddress(scheme, userName, password, serverName, port, parentUri, "", path, query, false);
		}

		public static int ParseNetworkPart(string uri, int startIndex, out string userName, out string password, out string serverName, out int? port)
		{
			var match = networkPartRegex.Match(uri, startIndex);

			userName = match.Groups["UserName"].Value;
			password = match.Groups["Password"].Value;
			serverName = match.Groups["ServerName"].Value;
			var s = match.Groups["Port"].Value;

			if (s.Length > 0)
			{
				try
				{
					port = Convert.ToInt32(s);
				}
				catch (FormatException)
				{
					throw new MalformedUriException();
				}
			}
			else
			{
				port = null;
			}

			return match.Groups["Rest"].Index;
		}

		public static void Parse(string uri, out string scheme, out string userName,
			out string password, out string serverName, out int? port, out string parentUri, out string path, out string query)
		{
			int y;
			var x = uri.IndexOf("://", StringComparison.Ordinal);
			
			if (x < 0)
			{
				throw new MalformedUriException(uri);
			}

			if (x == uri.Length - 1)
			{
				throw new MalformedUriException(uri);
			}

			scheme = uri.Substring(0, x);

			x = ParseNetworkPart(uri, x + 3, out userName, out password, out serverName, out port);

			if (uri[x] == '/')
			{
				x++;
			}

			if (x >= uri.Length || (x < uri.Length && uri[x] != '['))
			{
				y = x;
				parentUri = "";
			}
			else
			{
				y = ParseParentUri(uri, x);

				parentUri = uri.Substring(x + 1, y - x - 1);

				if (Local.LocalNodeAddress.CanParse(parentUri))
				{
					if (parentUri.IndexOf("://") < 0)
					{
						parentUri = "file://" + TextConversion.ToEscapedHexString(parentUri, TextConversion.IsStandardUrlEscapedChar);
					}
					else
					{
						parentUri = TextConversion.ToReEscapedHexString(parentUri, TextConversion.IsStandardUrlEscapedChar);
					}
				}
                else
				{
				    parentUri = TextConversion.ToReEscapedHexString(parentUri, TextConversion.IsStandardUrlEscapedChar);
				}
			}

			if (y + 1 >= uri.Length)
			{
				path = "/";
				query = "";
			}
			else
			{
				if (uri[y + 1] != FileSystemManager.SeperatorChar)
				{
					throw new MalformedUriException();
				}

				path = uri.Substring(y + 1);

				if ((x = path.IndexOf('?')) >= 0)
				{					
					query = path.Substring(x + 1);
					path = path.Substring(0, x);
				}
				else
				{
					query = "";
				}
			}
		}

		protected virtual INodeAddress CreateAddress(string scheme, string userName, string password, string serverName, int? port, string upperLayerUri, string rootPart, string path, string query, bool exposeAsNonComplexAddress)
		{
			return CreateAddressDynamically(GetType(), scheme, userName, password, serverName, port, upperLayerUri, rootPart, path, query, exposeAsNonComplexAddress);
		}

		protected override INodeAddress CreateAddress(string absolutePath, string query)
		{
			return CreateAddress(this.Scheme, UserName, Password, ServerName, Port, this.InnerUri, this.RootPart, absolutePath, query, this.exposeAsNonComplexAddress);
		}

		public override INodeAddress CreateAsRoot(string scheme)
		{
			if (this.AbsolutePath == FileSystemManager.RootPath)
			{
				return CreateAddress(scheme, UserName, Password, ServerName, Port, this.InnerUri, this.RootPart, "/", "", this.exposeAsNonComplexAddress);
			}
			else
			{
				return CreateAddress(scheme, UserName, Password, ServerName, Port, this.InnerUri, this.RootPart + this.AbsolutePath, "/", "", this.exposeAsNonComplexAddress);
			}			
		}

		public virtual bool HasNetworkPart
		{
			get
			{
				return this.UserName != ""
					|| this.Password != ""
					|| this.ServerName != ""
					|| this.Port >= 0;
			}
		}

		protected override string GetRootUri()
		{
			var buffer = new StringBuilder();

			buffer.Append(this.Scheme).Append("://");

			if (this.exposeAsNonComplexAddress)
			{
				buffer.Append(this.RootPart);
			}
			else
			{
				if (UserName != "")
				{
					buffer.Append(UserName);
				}

				if (Password != "")
				{
					buffer.Append(':').Append(Password);
				}

				if (UserName != "" || Password != "")
				{
					buffer.Append('@');
				}

				if (ServerName != "")
				{
					buffer.Append(ServerName);
				}

				if (Port >= 0)
				{
					buffer.Append(':').Append(Port);
				}

				if (!this.InnerUri.IsNullOrEmpty())
				{
					buffer.Append('[').Append(this.InnerUri).Append("]");
				}

				buffer.Append(this.RootPart);
			}

			return buffer.ToString();
		}
	}
}
