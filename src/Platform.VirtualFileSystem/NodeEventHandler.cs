using System;

namespace Platform.VirtualFileSystem
{
	public delegate void NodeActivityEventHandler(object sender, NodeActivityEventArgs eventArgs);

	public class NodeActivityEventArgs
		: EventArgs
	{
		public virtual INode Node { get; set; }

		public FileSystemActivity Activity
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		/// <remarks>
		/// This property returns the current name of the node.  If the node has been renamed
		/// then this property reflects the new name of the node.  To get the object representing
		/// the renamed node, the node should be resolved with the new name.
		/// </remarks>
		public string CurrentName { get; set; }

		/// <summary>
		/// OriginalName
		/// </summary>
		public string OriginalName { get; set; }

		/// <summary>
		/// Resolves the node.
		/// </summary>
		/// <remarks>
		/// If this node has been renamed then this method will return the new node representing
		/// the renamed node otherwise this method will return the current node.
		/// </remarks>
		public virtual INode Resolve()
		{
			return Node.ParentDirectory.Resolve(this.CurrentName);
		}

		public NodeActivityEventArgs(FileSystemActivity activity, INode node)
			: this(activity, node, null)
		{
		}

		public NodeActivityEventArgs(FileSystemActivity activity, INode node, string name)
		{
			this.Node = node;

			this.OriginalName = Node.Address.Name;
			
			if (name == null)
			{
				this.CurrentName = node.Name;
			}
			else
			{
				this.CurrentName = name;
			}

			this.Activity = activity;
		}

		public override string ToString()
		{
			return String.Format("{0} {1}", this.Activity, Node.Address.AbsolutePath);
		}
	}
}
