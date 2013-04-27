using System;
using System.Text;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public class StandardNodeAddress
		: AbstractNodeAddressWithRootPart
	{
		private readonly bool includeRootPartInUri;

		public virtual string UserName { get; private set; }
		public virtual string HostName { get; private set; }
		public virtual int Port { get; private set; }
		public virtual string Password { get; private set; }
		public virtual int DefaultPort { get; private set; }

		public StandardNodeAddress(string scheme, string hostName, int defaultPort, int port,
			string userName, string password, string path, string query)
			: this(scheme, hostName, defaultPort, port, userName, password, "", true, path, query)
		{
		}

		public StandardNodeAddress(string scheme, string hostName, int defaultPort, int port,
			string userName, string password, string rootPart, bool includeRootPartInUri, string path, string query)
			: base(scheme, rootPart, path, query)
		{
			this.HostName = hostName;
			this.Port = port;
			this.DefaultPort = defaultPort;

			this.UserName = userName;
			this.Password = password;			

			if (this.UserName == null)
			{
				this.UserName = "";
			}

			if (this.Password == null)
			{
				this.Password = "";
			}

			this.includeRootPartInUri = includeRootPartInUri;
		}

		public static StandardNodeAddress Parse(string uri)
		{
			return Parse(uri, -1);
		}

		/// <summary>
		/// Parses a <c>GenericFileName</c>.
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="defaultPort">
		/// The default port if no port is specified in the URI and the URI scheme is unknown.
		/// If the URI scheme is known, the default port for that scheme is used.
		/// </param>
		/// <returns></returns>
		public static StandardNodeAddress Parse(string uri, int defaultPort)
		{
			Uri sysUri;

			try
			{
				// For the moment use the Uri class to do parsing...

				sysUri = new Uri(uri);
			}
			catch (Exception e)
			{
				throw new MalformedUriException(uri, e.Message, e);
			}

			int x;
			string username = sysUri.UserInfo, password = "";

			x = username.IndexOf(':');

			if (x > 0)
			{
				username = username.Substring(0, x);
				password = username.Substring(x + 1);
			}

			if (defaultPort == -1)
			{
				if (sysUri.IsDefaultPort)
				{
					defaultPort = sysUri.Port;
				}
			}

			return new StandardNodeAddress(sysUri.Scheme, sysUri.Host, defaultPort,
				sysUri.Port, username, password,
				TextConversion.FromEscapedHexString(sysUri.AbsolutePath), TextConversion.FromEscapedHexString(StringUtils.Right(sysUri.Query, sysUri.Query.Length - 1)));
		}

		protected override INodeAddress CreateAddress(string absolutePath, string query)
		{
			return new StandardNodeAddress(this.Scheme, this.HostName, this.DefaultPort,
				this.Port, this.UserName, this.Password, this.RootPart, this.includeRootPartInUri, absolutePath, query);
		}
				
		public override INodeAddress CreateAsRoot(string scheme)
		{
			if (this.AbsolutePath == FileSystemManager.RootPath)
			{
				return new StandardNodeAddress(scheme, this.HostName, this.DefaultPort,
					this.Port, this.UserName, this.Password, this.RootPart, false, "/", "");
			}
			else
			{
				return new StandardNodeAddress(scheme, this.HostName, this.DefaultPort,
					this.Port, this.UserName, this.Password, this.RootPart + this.AbsolutePath, false, "/", "");
			}
		}

		protected override string GetRootUri()
		{
			var builder = new StringBuilder(64);

			builder.Append(this.Scheme).Append("://");
			
			if (this.UserName.Length > 0)
			{
				builder.Append(this.UserName);

				if (this.Password.Length > 0)
				{
					builder.Append(':').Append(this.Password);
				}

				builder.Append('@');
			}

			builder.Append(this.HostName);

			if (this.Port != this.DefaultPort && this.Port >= 0)
			{
				builder.Append(':');
				builder.Append(this.Port);
			}

			if (this.includeRootPartInUri)
			{
				if (this.RootPart != "")
				{
					builder.Append(this.RootPart);
				}
			}

			return builder.ToString();
		}
	}
}
