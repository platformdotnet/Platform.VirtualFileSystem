using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class ProtocolTrafficLog
	{
		private string connectionId;

		public ProtocolTrafficLog(string connectionId)
		{
			this.connectionId = connectionId;
		}
				
		public bool IsEnabled
		{
			get
			{
				return false;
			}
		}

		public virtual void LogTraffic(object value)
		{
		}
	}
}
