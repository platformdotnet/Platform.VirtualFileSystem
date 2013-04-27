#region Using directives



#endregion

namespace Platform.Network.ExtensibleServer.CommandServer
{
	public interface ITextConnection
	{
		void Flush();

		string ReadTextBlock();
		void WriteTextBlock(string block);
		void WriteTextBlock(string format, params object[] args);
	}
}
