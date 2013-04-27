namespace Platform.Network.ExtensibleServer
{
	/// <summary>
	/// Summary description for AnyRunLevel.
	/// </summary>
	public class AnyRunLevel
		: RunLevel
	{
		public const string RunLevelName = "ANY";

		public static readonly AnyRunLevel Default = new AnyRunLevel();

		public AnyRunLevel()
			: base(RunLevelName)
		{
		}
	}
}
