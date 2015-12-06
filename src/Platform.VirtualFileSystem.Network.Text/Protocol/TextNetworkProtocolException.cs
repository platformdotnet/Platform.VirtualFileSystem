using System;

namespace Platform.VirtualFileSystem.Network.Text.Protocol
{
	public class TextNetworkProtocolException
		: Exception
	{
	}

	internal class TextNetworkProtocolErrorResponseException
		: Exception
	{
		public TextNetworkProtocolErrorResponseException(string message)
			: base(message)
		{
		}
	}
}
