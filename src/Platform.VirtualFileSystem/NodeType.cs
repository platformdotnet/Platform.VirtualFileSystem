using System;
using System.Reflection;

namespace Platform.VirtualFileSystem
{
	[Serializable]
	public class NodeType
	{
		/// <summary>
		/// An invalid node type.
		/// </summary>
		public static readonly NodeType None = new NodeType(typeof(NodeNone));

		/// <summary>
		/// The type of any node that can be converted to <see cref="IFile"/>.
		/// </summary>
		public static readonly NodeType File = new NodeType(typeof(IFile));

		/// <summary>
		/// The type of any node that can be converted to <see cref="IDirectory"/>.
		/// </summary>
		public static readonly NodeType Directory  = new NodeType(typeof(IDirectory));

		/// <summary>
		/// The type of any node.
		/// </summary>
		public static readonly NodeType Any = new NodeType(typeof(NodeAny));

		/// <summary>
		/// Dummy class to represent the runtime type of <c>NodeType.None</c>.
		/// </summary>
		private static class NodeNone
		{
		}

		/// <summary>
		/// Dummy class to represent the runtime type of <c>NodeType.Any</c>.
		/// </summary>
		private static class NodeAny
		{
		}

		/// <summary>
		/// Get the Common Language Runtime <c>Type</c> that represents the type of node this is.
		/// </summary>
		public virtual Type RuntimeType { get; private set; }

		public virtual NodeType InnerType { get; set; }

		public NodeType()
			: this(null as NodeType)
		{
		}

		public NodeType(NodeType innerType)
		{
			this.RuntimeType = GetType();
			this.InnerType = innerType;			
		}

		public NodeType(Type runtimeType)
			: this(runtimeType, null)
		{
		}

		public NodeType(Type runtimeType, NodeType innerType)
		{
			this.RuntimeType = runtimeType;
			this.InnerType = innerType;
		}

		public static bool IsFile(NodeType nodeType)
		{
			return NodeType.File.Is(nodeType);
		}

		public static bool IsDirectory(NodeType nodeType)
		{
			return NodeType.Directory.Is(nodeType);
		}

		public static bool IsFile(INode node)
		{
			return NodeType.File.Is(node.NodeType);
		}

		public static bool IsDirectory(INode node)
		{
			return NodeType.Directory.Is(node.NodeType);
		}

		public virtual bool Is(Type type)
		{
			return type.IsAssignableFrom(this.RuntimeType);
		}

		public virtual bool Is(NodeType type)
		{			
			if (this == NodeType.Any)
			{
				return true;
			}

			if (type == NodeType.Any)
			{
				return true;
			}

			return type.RuntimeType.IsAssignableFrom(this.RuntimeType);
		}

		public override bool Equals(object obj)
		{
			var nodeType = obj as NodeType;

			if (nodeType == null)
			{
				return false;
			}

			return this.RuntimeType == nodeType.RuntimeType;
		}

		public override int GetHashCode()
		{
			return RuntimeType.GetHashCode();
		}

		static FieldInfo[] publicStaticFields;

		public override string ToString()
		{
			if (publicStaticFields == null)
			{
				publicStaticFields = typeof(NodeType).GetFields(BindingFlags.Static | BindingFlags.Public);
			}

			foreach (var fieldInfo in publicStaticFields)
			{
				var nodeType = (NodeType)fieldInfo.GetValue(null);

				if (this.Equals(nodeType))
				{
					return fieldInfo.Name;
				}
			}

			return RuntimeType.Name;
		}

		public virtual bool IsLikeDirectory
		{
			get
			{
				return this.Equals(NodeType.Directory);
			}
		}

		public static NodeType FromName(string nodeTypeName)
		{
			switch (nodeTypeName.ToUpper())
			{
				case "FILE":
				case "F":
					return NodeType.File;
				case "DIRECTORY":
				case "D":
					return NodeType.Directory;
				case "ANY":
				case "A":
					return NodeType.Any;
				default:
					throw new NotSupportedException("NodeType_" + nodeTypeName);
			}
		}
	}
}
