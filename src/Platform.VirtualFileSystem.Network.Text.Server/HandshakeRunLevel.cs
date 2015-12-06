using Platform.Network.ExtensibleServer;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	/// <summary>
	/// Summary description for HandshakeRunLevel.
	/// </summary>
	public class HandshakeRunLevel
		: RunLevel
	{
		public const string NAME = "Handshake";

		public static readonly HandshakeRunLevel Default = new HandshakeRunLevel();
	}
}
