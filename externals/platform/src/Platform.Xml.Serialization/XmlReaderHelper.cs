using System;
using System.Xml;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Helper methods for XmlReaders.
	/// </summary>
	internal class XmlReaderHelper
	{
		public static readonly XmlNodeType[] ElementOrEndElement = new XmlNodeType[] { XmlNodeType.Element, XmlNodeType.EndElement };		

		public static void ReadAndConsumeMatchingEndElement(XmlReader reader)
		{
			var x = 0;

			if (reader.IsEmptyElement)
			{
				reader.Read();

				return;
			}

			while (true)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					x++;
				}

				if (reader.NodeType == XmlNodeType.EndElement)
				{					
					if (x == 0)
					{
						reader.ReadEndElement();

						break;
					}

					x--;
				}

				if (reader.ReadState != ReadState.Interactive)
				{
					break;
				}

				reader.Read();
			}
		}

		public static void ReadAndApproachMatchingEndElement(XmlReader reader)
		{
			var x = 0;

			if (reader.IsEmptyElement)
			{
				reader.Read();

				return;
			}

			while (true)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					x++;
				}

				if (reader.NodeType == XmlNodeType.EndElement)
				{					
					if (x == 0)
					{
						break;
					}

					x--;
				}

				if (reader.ReadState != ReadState.Interactive)
				{
					break;
				}

				reader.Read();
			}
		}

		public static void ReadUntilTypeReached(XmlReader reader, XmlNodeType nodeType)
		{
			ReadUntilAnyTypesReached(reader, new XmlNodeType[] { nodeType });
		}

		public static void ReadUntilAnyTypesReached(XmlReader reader, XmlNodeType[] nodeTypes)
		{
			while (true)
			{
				if (Array.IndexOf(nodeTypes, reader.NodeType) >= 0)
				{
					break;
				}

				if (reader.ReadState != ReadState.Interactive)
				{
					break;
				}

				reader.Read();
			}
		}

		/// <summary>
		/// Reads the current node in the reader's value.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static string ReadCurrentNodeValue(XmlReader reader)
		{
			var fromElement = (reader.NodeType == XmlNodeType.Element);
 
			// If we're deserializing from an element,
 
			if (fromElement)
			{
				// read the start node.
 
				if (reader.IsEmptyElement)
				{					
					reader.Read();
 
					return "";
				}
 
				reader.ReadStartElement();
			}
 			
			var s = reader.Value;
 
			// If we're deserializing from an element,
 
			if (fromElement)
			{
				// read the end node.
 
				XmlReaderHelper.ReadAndConsumeMatchingEndElement(reader);
			}
             
			return s;
		}
	}
}
