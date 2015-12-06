using System;
using System.Collections.Generic;
using Platform.Text;

namespace Platform.VirtualFileSystem.Network.Text.Protocol
{
	public static class TextNetworkProtocol
	{
		public static readonly int DefaultPort = 6021;

		private class AttributesEnumerable
			: IEnumerable<Pair<string, object>>
		{
			private bool m_Finished;
			private Func<string> m_ReadNextLine;
			private ValueBox<string> m_CurrentLine;
			private Predicate<string> m_IsAttributeLine;

			public AttributesEnumerable(Func<string> readNextLine, Predicate<string> isAttributeLine, ValueBox<string> currentLine)
			{
				m_Finished = false;
				m_CurrentLine = currentLine;
				m_ReadNextLine = readNextLine;
				m_IsAttributeLine = isAttributeLine;
			}

			public IEnumerator<Pair<string, object>> GetEnumerator()
			{
				try
				{
					for (; ; )
					{
						if (m_Finished)
						{
							throw new InvalidOperationException();
						}

						m_CurrentLine.Value = m_ReadNextLine();

						if (m_IsAttributeLine(m_CurrentLine.Value))
						{
							string s;
							Pair<string, string> split;
							Pair<string, object> attributeValue;

							attributeValue = new Pair<string, object>();

							split = this.m_CurrentLine.Value.SplitAroundFirstCharFromLeft('=');

							s = split.Right.Trim().Trim('\"');

							attributeValue.Name = split.Left.Trim();
							attributeValue.Value = ProtocolTypes.FromEscapedString(s);

							yield return attributeValue;
						}
						else
						{
							m_Finished = true;

							yield break;
						}
					}
				}
				finally
				{
					Finish();
				}
			}

			protected internal virtual void Finish()
			{
				if (!m_Finished)
				{
					m_Finished = true;

					ConsumeAttributes(m_ReadNextLine, m_CurrentLine, m_IsAttributeLine);
				}
			}

			public static void ConsumeAttributes(Func<string> readNextLine, ValueBox<string> currentLine, Predicate<string> isAttributeLine)
			{
				for (;;)
				{
					currentLine.Value = readNextLine();

					if (!isAttributeLine(currentLine.Value))
					{
						break;
					}
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}

		public static NodeType GetNodeType(string name)
		{
			switch (name)
			{
				case "f":
					return NodeType.File;
				case "d":
					return NodeType.Directory;
				default:
					return NodeType.FromName(name);
			}
		}

		public static string GetNodeTypeName(NodeType nodeType)
		{
			if (nodeType == NodeType.File)
			{
				return "f";
			}
			else if (nodeType == NodeType.Directory)
			{
				return "d";
			}
			else
			{
				return null;
			}
		}

		public static IEnumerable<Pair<string, object>> ReadAttributes(Func<string> readNextLine, ValueBox<string> lastReadLine)
		{
			return new AttributesEnumerable
			(
				readNextLine,
				delegate(string s)
				{
					return !s.StartsWith(ResponseCodes.RESPONSE_MARKER);
				},
				lastReadLine
			);
		}

		public static IEnumerable<Pair<NetworkFileSystemEntry, string>> ReadEntries(Func<string> readNextLine)
		{
			string currentFile = null;
			NodeType currentNodeType = null;
			AttributesEnumerable attributesEnumerable;
			ValueBox<string> currentLine = new ValueBox<string>();

			Predicate<string> isAttributeLine = delegate(string s)
				{
					return s[0] == ' ';
				};

			currentLine.Value = readNextLine();

			try
			{
				for (; ; )
				{
					currentFile = null;
					currentNodeType = null;

					if (currentLine.Value.StartsWith(ResponseCodes.RESPONSE_MARKER, StringComparison.CurrentCultureIgnoreCase))
					{
						if (currentLine.Value.EqualsIgnoreCase(ResponseCodes.READY))
						{
							break;
						}
						else if (currentLine.Value.StartsWith(ResponseCodes.ERROR, StringComparison.CurrentCultureIgnoreCase))
						{
							yield return new Pair<NetworkFileSystemEntry, string>
							(
								new NetworkFileSystemEntry(),
								currentLine.Value
							);

							yield break;
						}
						else if (currentLine.Value.StartsWith(ResponseCodes.ENCODING))
						{
							string value = null, encoding = "url";

							currentLine.Value = currentLine.Value.Substring(ResponseCodes.ENCODING.Length);

							foreach (KeyValuePair<string, string> keyValuePair in CommandResponse.ParseTupleString(currentLine.Value))
							{
								if (keyValuePair.Key == "value")
								{
									value = keyValuePair.Value;
								}
								else if (keyValuePair.Key == "encoding")
								{
									encoding = keyValuePair.Key;
								}
								else if (keyValuePair.Key == "type")
								{
									currentNodeType = GetNodeType(keyValuePair.Value);
								}
							}

							if (value == null || currentNodeType == null)
							{
								AttributesEnumerable.ConsumeAttributes(readNextLine, currentLine, isAttributeLine);

								continue;
							}

							try
							{
								currentFile = ProtocolTypes.DecodeString(value, encoding);
							}
							catch (NotSupportedException)
							{
								AttributesEnumerable.ConsumeAttributes(readNextLine, currentLine, isAttributeLine);

								continue;
							}
						}
						else
						{
							// Major error

							throw new TextNetworkProtocolException();
						}
					}

					if (currentLine.Value.Length == 0)
					{
						continue;
					}

					if (currentFile == null)
					{
						Pair<string, string> result;

						result = currentLine.Value.SplitAroundFirstCharFromLeft(':');
						currentFile = TextConversion.FromEscapedHexString(result.Right);
						currentNodeType = GetNodeType(result.Left);
					}

					if (currentNodeType == null)
					{
						AttributesEnumerable.ConsumeAttributes(readNextLine, currentLine, isAttributeLine);

						continue;
					}

					attributesEnumerable = new AttributesEnumerable
					(
						readNextLine,
						isAttributeLine,
						currentLine
					);

					try
					{
						yield return new Pair<NetworkFileSystemEntry, string>
						(
							new NetworkFileSystemEntry(currentFile, currentNodeType, attributesEnumerable),
							null
						);
					}
					finally
					{
						attributesEnumerable.Finish();
					}
				}
			}
			finally
			{
				while (!currentLine.Value.StartsWith(ResponseCodes.READY))
				{
					currentLine.Value = readNextLine();					
				}
			}
		}	
	}
}
