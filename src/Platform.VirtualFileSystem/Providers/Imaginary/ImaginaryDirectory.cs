using System;
using System.IO;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers.Imaginary
{
	public class ImaginaryDirectory
		: AbstractDirectory
	{
		public virtual event EventHandler Refreshed;

		public virtual Func<NodeType, Predicate<INode>, IEnumerable<INode>> ChildrenProvider { get; set; }

		public virtual void OnRefreshed(EventArgs eventArgs)
		{
			if (Refreshed != null)
			{
				Refreshed(this, eventArgs);
			}
		}
	
		private readonly IDictionary<Pair<string, NodeType>, INode> children;

		public new DictionaryBasedNodeAttributes Attributes
		{
			get
			{
				return (DictionaryBasedNodeAttributes)base.Attributes;
			}
		}
				
		public ImaginaryDirectory(IFileSystem fileSystem, INodeAddress address)
			: base(fileSystem, address)
		{
			Comparison<Pair<string, NodeType>> comparison = delegate(Pair<string, NodeType> p1, Pair<string, NodeType> p2)
			{
				if (p1.Right != p2.Right)
				{
					return -1;
				}

				return StringComparer.CurrentCultureIgnoreCase.Compare(p1.Left, p2.Left);
			};

			this.children = new SortedDictionary<Pair<string, NodeType>, INode>(new ComparisonComparer<Pair<string, NodeType>>(comparison));
		}

		public virtual void DeleteAllChildren()
		{
			this.children.Clear();
			((ImaginaryFileSystem)this.FileSystem).NodeCache.PurgeWithDescendents(this.Address, NodeType.Any);
		}

		public virtual void Add(INode node)
		{
			this.children[new Pair<string, NodeType>(node.Name, node.NodeType)] = node;
		}

		public virtual INode GetImaginaryChild(string name, NodeType nodeType)
		{
			INode retval;
			var pair = new Pair<string, NodeType>(name, nodeType);

			if (this.children.TryGetValue(pair, out retval))
			{
				return retval;
			}

			return null;
		}

		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			if (this.ChildrenProvider != null)
			{
				foreach (var node in this.ChildrenProvider(nodeType, acceptNode))
				{
					yield return node;
				}

				yield break;
			}

			foreach (var node in GetJumpPoints(nodeType, acceptNode))
			{
				if (nodeType == NodeType.Any || node.NodeType == nodeType)
				{
					yield return node;
				}
			}

			foreach (var node in this.children.Values)
			{
				if (node.NodeType == nodeType)
				{
					yield return node;
				}
			}
		}

		public override INode DoCreate(bool createParent)
		{
			if (!createParent)
			{
				if (!this.Address.IsRoot && !ParentDirectory.Exists)
				{
					throw new IOException();
				}

				// Mono crashes with ** ERROR **: file object.c: line 1301 (mono_object_get_virtual_method): assertion failed: (res)
				// DictionaryBasedNodeAttributes attribs = this.Attributes;

				var attribs = ((DictionaryBasedNodeAttributes)base.Attributes);

				using (attribs.GetAutoLock().Lock())
				{
					attribs.SetValue<bool>("Exists", true);
				}

				return this;
			}

			if (!this.Address.IsRoot && !ParentDirectory.Exists)
			{
				ParentDirectory.Create(true);
			}

			this.Attributes.SetValue<bool>("Exists", true);

			((ImaginaryDirectory)ParentDirectory).Add(this);

			return this;
		}

		public override IDirectory Delete(bool recursive)
		{
			if (recursive)
			{
				foreach (var node in this.children.Values)
				{
					if (node is IDirectory)
					{
						((IDirectory)node).Delete(true);
					}
					else
					{
						node.Delete();
					}
				}

				this.Attributes.SetValue<bool>("Exists", false);
			}
			else
			{
				if (this.children.Count > 0)
				{
					throw new IOException();
				}

				this.Attributes.SetValue<bool>("Exists", false);
			}

			return this;
		}

		protected virtual bool AttributesSupportsAttribute(string name)
		{
			return name.Equals("exists", StringComparison.CurrentCultureIgnoreCase);
		}

		public override INode Refresh()
		{
			base.Refresh();

			OnRefreshed(EventArgs.Empty);

			return this;
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new DictionaryBasedNodeAttributes(AttributesSupportsAttribute);
		}
	}
}
