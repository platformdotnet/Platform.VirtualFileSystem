namespace Platform.Network.ExtensibleServer
{
	/// <summary>
	/// Summary description for AnyRunLevel.
	/// </summary>
	public class DisconnectedRunLevel
		: RunLevel
	{
		public const string RunLevelName = "DISCONNECTED";

		public static readonly DisconnectedRunLevel Default = new DisconnectedRunLevel();

		public DisconnectedRunLevel()
			: base(RunLevelName)
		{
		}
	}
}
