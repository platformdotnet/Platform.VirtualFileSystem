using System;
using System.Text;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public class GenericNodeAddress
		: AbstractNodeAddress
	{
		/// <summary>
		/// Sets/Gets UserName
		/// </summary>
		public virtual string UserName
		{
			get
			{
				return m_UserName;
			}
		}
		/// <remarks>
		/// <see cref="UserName"/>
		/// </remarks>
		private string m_UserName;

		/// <summary>
		/// Sets/Gets HostName
		/// </summary>
		public virtual string HostName
		{
			get
			{
				return m_HostName;
			}
		}
		/// <remarks>
		/// <see cref="HostName"/>
		/// </remarks>
		private string m_HostName;

		/// <summary>
		/// Sets/Gets Port
		/// </summary>
		public virtual int Port
		{
			get
			{
				return m_Port;
			}
		}
		/// <remarks>
		/// <see cref="Port"/>
		/// </remarks>
		private int m_Port;

		/// <summary>
		/// Sets/Gets Password
		/// </summary>
		public virtual string Password
		{
			get
			{
				return m_Password;
			}
		}
		/// <remarks>
		/// <see cref="Password"/>
		/// </remarks>
		private string m_Password;

		/// <summary>
		/// Sets/Gets DefaultPort
		/// </summary>
		public virtual int DefaultPort
		{
			get
			{
				return m_DefaultPort;
			}
		}
		/// <remarks>
		/// <see cref="DefaultPort"/>
		/// </remarks>
		private int m_DefaultPort;

		private string m_RootPath;

		public string RootPath
		{
			get
			{
				return m_RootPath;
			}
		}

		public GenericNodeAddress(string scheme, string hostName, int defaultPort, int port,
			string userName, string password, string path)
			: this(scheme, hostName, defaultPort, port, userName, password, "", path)
		{
		}

			/// <summary>
		/// Initialises a new <c>GenericFileName</c>.
		/// </summary>
		/// <param name="scheme"></param>
		/// <param name="hostName"></param>
		/// <param name="port"></param>
		/// <param name="userName"></param>
		/// <param name="password"></param>
		/// <param name="path"></param>
		public GenericNodeAddress(string scheme, string hostName, int defaultPort, int port,
			string userName, string password, string rootPath, string path)
			: base(scheme, path)
		{
			m_HostName = hostName;
			m_Port = port;
			m_DefaultPort = defaultPort;
			m_RootPath = rootPath;

			m_UserName = userName;
			m_Password = password;			

			if (m_UserName == null)
			{
				m_UserName = "";
			}

			if (m_Password == null)
			{
				m_Password = "";
			}
		}

		public static GenericNodeAddress Parse(string uri)
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
		public static GenericNodeAddress Parse(string uri, int defaultPort)
		{
			Uri sysUri;

			try
			{
				// For the moment use the Uri class to do parsing...

				sysUri = new Uri(uri);
			}
			catch (Exception e)
			{
				throw new MalformedUriException(e.Message, e);
			}

			int x;
			string username = sysUri.UserInfo, password = "";

			x = username.IndexOf('@');

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

			return new GenericNodeAddress(sysUri.Scheme, sysUri.Host, defaultPort,
				sysUri.Port, username, password,
				sysUri.PathAndQuery);
		}

		/// <summary>
		/// <see cref="AbstractFileName.CreateName()"/>
		/// </summary>
		protected override INodeAddress CreateName(string absolutePath)
		{
			return new GenericNodeAddress(this.Scheme, this.HostName, this.DefaultPort,
				this.Port, this.UserName, this.Password, this.m_RootPath, absolutePath);
		}

		/// <summary>
		/// <see cref="AbstractFileName.GetRootUri()"/>
		/// </summary>
		protected override string GetRootUri()
		{
			StringBuilder builder = new StringBuilder(64);

			builder.Append(this.Scheme).Append("://");
			
			if (m_UserName.Length > 0)
			{
				builder.Append(m_UserName);

				if (m_Password.Length > 0)
				{
					builder.Append(':').Append(m_Password);
				}

				builder.Append('@');
			}

			builder.Append(m_HostName);

			if (this.Port != this.DefaultPort && m_Port >= 0)
			{
				builder.Append(':');
				builder.Append(m_Port);
			}

			if (m_RootPath != "")
			{
				builder.Append(m_RootPath);
			}

			return builder.ToString();
		}
	}
}
