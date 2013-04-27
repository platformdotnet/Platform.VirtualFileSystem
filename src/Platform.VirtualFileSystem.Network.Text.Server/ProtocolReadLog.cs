using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class ProtocolReadLog
	{
		private string connectionId;

		public ProtocolReadLog(string connectionId)
		{
			this.connectionId = connectionId;
		}
				
		public virtual bool IsEnabled
		{
			get
			{
				return false;
			}
		}

		private StringBuilder partialWriteBlock = new StringBuilder();

		public virtual void LogRead(string text)
		{
		}
	}
}