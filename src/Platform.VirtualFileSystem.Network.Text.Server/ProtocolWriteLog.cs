using System.Text;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public class ProtocolWriteLog
	{
		private string connectionId;

		public ProtocolWriteLog(string connectionId)
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

		private readonly StringBuilder partialWriteBlock = new StringBuilder();

		public virtual void LogWrite(string text)
		{
		}

		public virtual void LogWrite(string format, params object[] args)
		{
			LogWrite(string.Format(format, args));
		}

		public virtual void LogWritePartial(char[] buffer, int offset, int length)
		{
			partialWriteBlock.Append(buffer, offset, length);
		}

		public virtual void LogWritePartial(string text)
		{
			partialWriteBlock.Append(text);
		}
	}
}
