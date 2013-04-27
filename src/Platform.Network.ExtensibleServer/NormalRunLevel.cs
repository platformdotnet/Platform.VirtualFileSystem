namespace Platform.Network.ExtensibleServer
{
	/// <summary>
	/// Summary description for NormalRunLevel.
	/// </summary>
	public class NormalRunLevel
		: RunLevel
	{
		public const string RunLevelName = "NORMAL";

		public static readonly NormalRunLevel Default = new NormalRunLevel();

		public NormalRunLevel()
			: base(RunLevelName)
		{
		}
	}
}
