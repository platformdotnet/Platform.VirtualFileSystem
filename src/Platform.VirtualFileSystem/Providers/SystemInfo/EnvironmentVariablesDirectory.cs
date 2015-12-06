using System;
using System.Collections.Generic;
using System.Text;
using Platform.VirtualFileSystem.Providers.Imaginary;

namespace Platform.VirtualFileSystem.Providers.SystemInfo
{
	public class EnvironmentVariablesDirectory
		: ImaginaryDirectory
	{
		public EnvironmentVariablesDirectory(IFileSystem fileSystem, INodeAddress address)
			: base(fileSystem, address)
		{
			Create();			
		}

		public override INode Create()
		{
			this.Attributes.SetValue<bool>("exists", true);

			OnActivity(new NodeActivityEventArgs(FileSystemActivity.Created, this));

			return this;
		}

		public override INode GetImaginaryChild(string name, NodeType nodeType)
		{
			return new ImaginaryMemoryFile
			(
				this.FileSystem, this.Address.ResolveAddress(name),
				delegate
				{
					string s;

					s = Environment.GetEnvironmentVariable(name);

					if (s == null)
					{
						s = "";
					}

					return Encoding.ASCII.GetBytes(s + Environment.NewLine);
				},
				delegate
				{
					return Environment.GetEnvironmentVariable(name) != null;
				}
			);
		}

		private IEnumerable<INode> BaseGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			return base.GetChildren(nodeType, acceptNode);
		}
		
		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			foreach (string key in Environment.GetEnvironmentVariables().Keys)
			{
				yield return GetImaginaryChild(key, NodeType.File);
			}
		}
	}
}
