using System;
using System.Collections.Generic;
using System.IO;

namespace Platform.VirtualFileSystem.Providers.Overlayed
{
	public class OverlayedDirectory
		: DirectoryDelegationWrapper
	{
		private readonly INodeAddress nodeAddress;
		private readonly OverlayedFileSystem fileSystem;

		public OverlayedDirectory(OverlayedFileSystem fileSystem, INodeAddress nodeAddress, IDirectory directory)
			: base(directory)
		{
			this.fileSystem = fileSystem;
			this.nodeAddress = nodeAddress;
		}

		public override IFileSystem CreateView(string scheme, FileSystemOptions options)
		{
			return new View.ViewFileSystem(scheme, this, options);
		}

		public override INodeAddress Address
		{
			get
			{
				return nodeAddress;
			}
		}

		public override IFileSystem FileSystem
		{
			get
			{
				return fileSystem;
			}
		}

		public override IEnumerable<INode> GetAlternates()
		{
			return fileSystem.GetAlternates(this);
		}

		public override IDirectory Refresh(DirectoryRefreshMask mask)
		{
			foreach (var node in this.GetAlternates())
			{
				((IDirectory)node).Refresh(mask);
			}

			SetWrappee(fileSystem.GetOverlay(this.Wrappee.Address, this.Wrappee.NodeType));

			return this;
		}

		/// <summary>
		/// <see cref="IResolver.Resolve(string, NodeType, AddressScope)"/>
		/// </summary>
		public override INode Resolve(string name, NodeType nodeType, AddressScope scope)
		{
			var address = this.Address.ResolveAddress(name, scope);

			return this.FileSystem.Resolve(address, nodeType);
		}

		public override IEnumerable<string> GetChildNames(NodeType nodeType, Predicate<string> acceptName)
		{
			var localNodes = nodes;

			localNodes.Clear();

			try
			{
				foreach (IDirectory dir in this.GetAlternates())
				{
					if (dir == this)
					{
						continue;
					}

					var childNames = dir.GetChildNames(nodeType).GetEnumerator();

					using (childNames)
					{
						while (true)
						{
							try
							{
								if (!childNames.MoveNext())
								{
									break;
								}
							}
							catch (DirectoryNodeNotFoundException)
							{
								break;
							}

							if (!localNodes.ContainsKey(childNames.Current))
							{
								localNodes.Add(childNames.Current, true);

								yield return childNames.Current;
							}
						}
					}
				}
			}
			finally
			{
				localNodes.Clear();
			}
		}

		[ThreadStatic]
		private IDictionary<string, bool> nodes = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);

		public override IEnumerable<INode> GetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			INode node;

			foreach (string name in this.GetChildNames(nodeType))
			{
				node = fileSystem.Resolve(this.Address.ResolveAddress(name).AbsolutePath, nodeType);

				if (acceptNode(node))
				{
					yield return node;
				}
			}			
		}

		public override INode Create(bool createParent)
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation(
					(OverlayedFileSystem)this.FileSystem, FileSystemActivity.Created, this.Address, this.NodeType, out nodes))
			{
				int count = 0;

				foreach (IDirectory node in nodes)
				{
					try
					{
						node.Create(createParent);
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.Create(createParent);
			}

			return this;
		}

		public override IDirectory Delete(bool recursive)
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation(
					(OverlayedFileSystem)this.FileSystem, FileSystemActivity.Deleted, this.Address, this.NodeType, out nodes))
			{
				int count = 0;

				foreach (IDirectory node in nodes)
				{
					try
					{
						node.Delete(recursive);
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.Delete(recursive);
			}

			return this;
		}

		public override INode RenameTo(string name, bool overwrite)
		{
			INode[] nodes;

			if (((OverlayedFileSystem)this.FileSystem).OverlayedNodeSelector.SelectNodeForOperation(
					(OverlayedFileSystem)this.FileSystem, FileSystemActivity.Renamed, this.Address, this.NodeType, out nodes))
			{
				int count = 0;

				foreach (IDirectory node in nodes)
				{
					try
					{
						node.RenameTo(name, overwrite);
						count++;
					}
					catch (NodeNotFoundException)
					{
						continue;
					}
				}

				if (count == 0)
				{
					throw new NodeNotFoundException(this.Address);
				}
			}
			else
			{
				return base.RenameTo(name, overwrite);
			}

			return this;
		}
	}
}
