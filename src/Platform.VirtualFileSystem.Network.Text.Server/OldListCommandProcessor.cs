using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Platform.Network.ExtensibleServer;
using Platform.Network.ExtensibleServer.CommandServer;
using Platform.Utilities;
using Platform.VirtualFileSystem.Network.Protocol;

namespace Platform.VirtualFileSystem.Network.Server
{
	/*
	[TextCommandSpecification("LIST", "NORMAL")]
	public class ListCommandProcessor
		: FileSystemTextCommandProcessorWithOptions
	{
		public class CommandOptions
		{
			[CommandLineOption(0, Required = true)]
			public string Uri = "";

			[CommandLineOption(Required = false)]
			public string Name = "";

			[CommandLineOption(Required = false)]
			public string Types = "D F";

			[CommandLineOption(Required = false)]
			public string Format = @"$(T) $(NAME)";
		}

		private static Regex m_Regex;

		static ListCommandProcessor()
		{
			m_Regex = new Regex(@"\$\(#?[a-zA-Z0-9\-]+\)");
		}

		public ListCommandProcessor(Connection connection)
			: base(connection)
		{
		}

		#region WriteTabbedFormatEvaluator

		private class WriteTabbedFormatEvaluator
		{
			private INode m_Node;
			private string m_Name;
			private NodeType m_NodeType;
			private IDirectory m_ParentDirectory;

			public WriteTabbedFormatEvaluator(INode node)
			{
				m_Node = node;
			}

			public WriteTabbedFormatEvaluator(IDirectory parentDirectory, string name, NodeType nodeType)
			{
				m_Name = name;
				m_NodeType = nodeType;
				m_ParentDirectory = parentDirectory;
			}

			private string EncodeString(string s, bool encode)
			{
				if (encode)
				{
					return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
				}
				else
				{
					return s;
				}
			}

			public string Evaluator(Match match)
			{
				bool encode;
				string value;

				encode = match.Value.IndexOf('#') >= 0;

				if (encode)
				{
					value = match.Value.Replace("#", "");
				}
				else
				{
					value = match.Value;
				}

				switch (value.ToUpper())
				{
					case "$(T)":
						if (m_Node != null)
						{
							return EncodeString(m_Node.NodeType.ToString().ToUpper().Substring(0, 1), encode);
						}
						else
						{
							return EncodeString(m_NodeType.ToString().ToUpper().Substring(0, 1), encode);
						}
					case "$(NAME)":
						if (m_Node != null)
						{
							return EncodeString(m_Node.Address.NameAndQuery, encode);
						}
						else
						{
							return EncodeString(m_Name, encode);
						}
					case "$(TYPE)":
						if (m_Node != null)
						{
							return EncodeString(m_Node.NodeType.ToString().ToUpper(), encode);
						}
						else
						{
							return EncodeString(m_NodeType.ToString().ToUpper(), encode);
						}
					case "$(ATTRIBUTE-NAMES)":

						StringBuilder builder = new StringBuilder();

						foreach (string s in m_Node.Attributes.Names)
						{
							builder.Append(s);
							builder.Append(' ');
						}

						if (builder.Length > 0)
						{
							builder.Length--;
						}

						return builder.ToString();

					default:
						if (m_Node == null)
						{
							m_Node = m_ParentDirectory.Resolve(m_Name, m_NodeType);
						}

						try
						{
							string s;
							object attribute;
							
							attribute = m_Node.Attributes[value.Substring(2, value.Length - 3)];

							if (attribute is DateTime)
							{
								DateTime dt;

								dt = (DateTime)attribute;

								s = dt.ToUniversalTime().ToString("yyyy-MM-dd HH-mm-ss.fffffff");
							}
							else
							{
								s = Convert.ToString(attribute.ToString());
							}

							return s;
						}
						catch (NotSupportedException)
						{
							return "";
						}
				}
			}
		}

		#endregion

		private void WriteTabbedFormat(string format, INode node)
		{
			string s;

			s = m_Regex.Replace(format, new MatchEvaluator(new WriteTabbedFormatEvaluator(node).Evaluator));

			this.Connection.WriteLine(s);
		}

		private void WriteTabbedFormat(string format, IDirectory parentDirectory, string name, NodeType nodeType)
		{
			string s;

			s = m_Regex.Replace(format, new MatchEvaluator(new WriteTabbedFormatEvaluator(parentDirectory, name, nodeType).Evaluator));
			
			this.Connection.WriteLine(s);
		}

		public override void Process(Command command)
		{
			string[] types;			
			NodeType nodeType;
			IDirectory directory;
			CommandOptions options;

			options = (CommandOptions)LoadOptions((TextCommand)command);

			types = options.Types.Split(' ');

			if (options.Name != "")
			{
				INode node;

				directory = Connection.FileSystemManager.ResolveDirectory(options.Uri);

				Connection.Writer.WriteLine("OK");

				foreach (string s in types)
				{
					nodeType = NodeType.FromName(s);

					node = directory.Resolve(options.Name, nodeType);

					if (node.Exists)
					{
						WriteTabbedFormat(options.Format, node);
					}
				}
			}
			else
			{
				directory = Connection.FileSystemManager.ResolveDirectory(options.Uri);

				foreach (string s in types)
				{
					nodeType = NodeType.FromName(s);

					foreach (INode node in directory.GetChildren(nodeType))
					{
						node.Refresh();

						if (node.Exists)
						{
							WriteTabbedFormat(options.Format, node);
						}
					}
				}
			}

			Connection.Writer.Flush();
		}
	}
	*/
}
