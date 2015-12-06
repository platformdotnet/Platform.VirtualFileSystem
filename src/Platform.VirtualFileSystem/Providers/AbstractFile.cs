using System;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>IFile</c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="IFile"/>
	/// </summary>
	public abstract class AbstractFile
		: AbstractNode, IFile
	{
		protected AbstractFile(INodeAddress fileName, IFileSystem fileSystem)
			: base(fileSystem, fileName)
		{
		}

		IFile IFile.Create()
		{
			return (IFile)Create();
		}

		IFile IFile.Create(bool createParent)
		{
			return (IFile)Create(createParent);
		}

		IFile IFile.Refresh()
		{
			return (IFile)Refresh();
		}

		public new IFileAttributes Attributes
		{
			get
			{
				try
				{
					return (IFileAttributes)base.Attributes;
				}
				catch (InvalidCastException)
				{
					return new NodeToFileAttributes(base.Attributes);
				}
			}
		}

		public override NodeType NodeType
		{
			get
			{
				return NodeType.File;
			}
		}

		protected IFile GetOperationTargetDirectoryFile(INode target)
		{
			return GetOperationTargetDirectoryFile(this, target);
		}

		public static IFile GetOperationTargetDirectoryFile(INode thisNode, INode target)
		{
			IFile file;

			if (target.NodeType == NodeType.File)
			{
				file = (IFile)target;
			}
			else if (target.NodeType == NodeType.Directory)
			{
				file = target.ResolveFile(thisNode.Name);
			}
			else
			{			
				throw new ArgumentException("Target must be a file or directory.", "target");
			}

			return file;
		}

		public override IDirectory OperationTargetDirectory
		{
			get
			{
				return this.ParentDirectory;
			}
		}
				
		public virtual long? Length
		{
			get
			{
				return (long?)this.Attributes[FileAttributes.Length];
			}
		}

		public virtual bool IdenticalTo(IFile file, FileComparingFlags flags)
		{
			return IdenticalTo(this, file, flags);
		}

		public static bool IdenticalTo(IFile thisFile, IFile file, FileComparingFlags flags)
		{
			var service = (IFileComparingService)thisFile.GetService(new FileComparingServiceType(file, flags));
			
			return (bool)service.Compare();
		}

		public virtual IFile CreateAsHardLink(IFile targetFile)
		{
			return CreateAsHardLink(targetFile, false);
		}

		public virtual IFile CreateAsHardLink(IFile targetFile, bool overwrite)
		{
			return DoCreateHardLink(targetFile, overwrite);
		}

		protected virtual IFile DoCreateHardLink(IFile targetFile, bool overwrite)
		{
			throw new NotSupportedException();
		}
		
		public virtual IFile CreateAsHardLink(string path)
		{
			return CreateAsHardLink(path, false);
		}

		public virtual IFile CreateAsHardLink(string path, bool overwrite)
		{
			return CreateAsHardLink(this.FileSystem.ResolveFile(path), overwrite);
		}

		public override INode GetDirectoryOperationTargetNode(IDirectory directory)
		{
			return directory.ResolveFile(this.Address.NameAndQuery);
		}
	}
}