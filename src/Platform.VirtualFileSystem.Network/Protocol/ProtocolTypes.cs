using System;

namespace Platform.VirtualFileSystem.Network.Protocol
{
	/// <summary>
	/// Summary description for Types.
	/// </summary>
	public class ProtocolTypes
	{
		private object[] m_TypeNames = 
		{
			typeof(bool),
			"b",
			typeof(byte),
			typeof(int),
			"i32",
			typeof(Int64),
			"i64"
		};

		public string GetTypeName(Type type)
		{
			return "";
		}
	}
}
