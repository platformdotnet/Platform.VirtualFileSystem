using System;
using System.Net.Sockets;

namespace Platform.Network.ExtensibleServer
{
	/// <summary>
	/// Summary description for ConnectionBuilder.
	/// </summary>
	public class ConnectionFactory
	{
		private readonly object[] extraArgs;
		private readonly Type connectionType;

		protected ConnectionFactory()
		{
		}

		public ConnectionFactory(Type connectionType, params object[] extraArgs)
		{
			if (!typeof(Connection).IsAssignableFrom(connectionType))
			{
				throw new ArgumentException("Must extend Connection", "connectionType");
			}

			this.extraArgs = extraArgs;
			this.connectionType = connectionType;
		}

		public virtual Connection NewConnection(NetworkServer networkServer, Socket socket)
		{
			var args = new object[] { networkServer, socket }.Combine(extraArgs);

			try
			{
				return (Connection)Activator.CreateInstance(connectionType, args);
			}
			catch (MissingMethodException)
			{
			}

			return (Connection)Activator.CreateInstance(connectionType, new object[0]);
		}
	}
}
