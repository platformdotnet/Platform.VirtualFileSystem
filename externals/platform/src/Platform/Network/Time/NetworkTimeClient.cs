using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Network.Time
{
	/// <summary>
	/// Base class for any network time client.
	/// </summary>
	public abstract class NetworkTimeClient
		: AbstractTask, IValued<TimeSpan?>
	{
		/// <summary>
		/// The name of the time server
		/// </summary>
		public virtual string ServerName
		{
			get;
			set;
		}

		/// <summary>
		/// The port the time server is listening on
		/// </summary>
		public virtual int Port
		{
			get;
			set;
		}

		/// <summary>
		/// Constructs a new <see cref="NetworkTimeClient"/>
		/// </summary>
		/// <param name="serverName">The name of the time server</param>
		/// <param name="port">The port the time server is listening on</param>
		protected NetworkTimeClient(string serverName, int port)
		{
			this.Port = port;
			this.ServerName = serverName;
		}

		/// <summary>
		/// The current value from the time server (or null if no value)
		/// </summary>
		public abstract TimeSpan? Value
		{
			get;
		}

		/// <summary>
		/// The current value from the time server (or null if no value).  Value will
		/// be returned as a nullable <see cref="TimeSpan"/>.
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.Value;
			}
		}
	}
}
