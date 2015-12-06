using Platform.VirtualFileSystem.Network.Text.Protocol;

namespace Platform.VirtualFileSystem.Network.Text.Server
{
	public static class AttributesCommands
	{
		public static void PrintAttribute(FileSystemCommandConnection connection, string name, object value)
		{
			var typeName = ProtocolTypes.GetTypeName(value.GetType());

			if (typeName != null)
			{
				connection.WriteTextBlock(" {0}=\"{1}:{2}\"", name,
					typeName, ProtocolTypes.ToEscapedString(value));
			}
		}

		public static void PrintAttributes(FileSystemCommandConnection connection, INode node)
		{
			foreach (var keyValuePair in node.Attributes)
			{
				if (!keyValuePair.Key.EqualsIgnoreCase("exists"))
				{
					if (keyValuePair.Value != null)
					{
						PrintAttribute(connection, keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
		}
	}
}
