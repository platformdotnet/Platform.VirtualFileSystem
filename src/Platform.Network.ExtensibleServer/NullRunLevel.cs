namespace Platform.Network.ExtensibleServer
{
	/// <summary>
	/// Summary description for NullRunLevel.
	/// </summary>
	public class NullRunLevel
		: RunLevel
	{
		public const string RunLevelName = "NULL";

		public static readonly NullRunLevel Default = new NullRunLevel();

		public NullRunLevel()
			: base(RunLevelName)
		{
		}

		public override string ToString()
		{
			return Name;
		}

	}
}
