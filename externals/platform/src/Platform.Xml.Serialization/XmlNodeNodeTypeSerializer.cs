using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

namespace Platform.Xml.Serialization
{
	public class XmlNodeNodeTypeSerializer
		: TypeSerializer
	{
		public static readonly XmlNodeNodeTypeSerializer Default = new XmlNodeNodeTypeSerializer();

		public override Type SupportedType
		{
			get
			{
				return typeof(XmlNode);
			}
		}

		public override bool MemberBound
		{
			get
			{
				return false;
			}
		}

		private static void WriteNode(XmlReader reader, XmlWriter writer)
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
				case XmlNodeType.Element:
					writer.WriteStartElement(reader.Name);
					break;
				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;
				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;
				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;
				case XmlNodeType.XmlDeclaration:
					break;
				case XmlNodeType.Document:
					break;
				case XmlNodeType.DocumentType:
					break;
				case XmlNodeType.EntityReference:
					break;
				case XmlNodeType.EndElement:
					writer.WriteEndElement();
					break;
				}
			}
		}

		public override void Serialize(object obj, System.Xml.XmlWriter writer, SerializationContext state)
		{
			foreach (XmlNode child in ((XmlNode)obj).ChildNodes)
			{
				var reader = new XmlNodeReader(child);

				WriteNode(reader, writer);
			}
		}

		public override object Deserialize(System.Xml.XmlReader reader, SerializationContext state)
		{
			var document = new XmlDocument();

			document.LoadXml(reader.ReadOuterXml());

			return document.FirstChild;
		}
	}
}
