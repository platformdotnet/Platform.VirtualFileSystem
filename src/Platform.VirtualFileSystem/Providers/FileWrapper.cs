namespace Platform.VirtualFileSystem.Providers
{
	public abstract class FileWrapper
		: NodeDelegationWrapper, IFile
	{
		protected FileWrapper(IFile innerFile)
			: base(innerFile)
		{
		}

		public new IFileAttributes Attributes
		{
			get
			{
				return (IFileAttributes)this.Wrappee.Attributes;
			}
		}

		IFile IFile.Refresh()
		{
			return (IFile)this.Refresh();
		}

		IFile IFile.Create()
		{
			return this.Wrappee.Create();
		}

		IFile IFile.Create(bool createParent)
		{
			return this.Wrappee.Create(createParent);
		}

		public new virtual IFile Wrappee
		{
			get
			{
				return (IFile)base.Wrappee;
			}
		}

		public virtual long? Length
		{
			get
			{
				return this.Wrappee.Length;
			}
		}

		public virtual bool IdenticalTo(IFile file, FileComparingFlags flags)
		{
			return this.Wrappee.IdenticalTo(file, flags);
		}

		public virtual IFile CreateAsHardLink(IFile file)
		{
			this.Wrappee.CreateAsHardLink(file);

			return this;
		}

		public virtual IFile CreateAsHardLink(IFile file, bool overwrite)
		{
			this.Wrappee.CreateAsHardLink(file, overwrite);

			return this;
		}

		public virtual IFile CreateAsHardLink(string path)
		{
			this.Wrappee.CreateAsHardLink(path);

			return this;
		}

		public virtual IFile CreateAsHardLink(string path, bool overwrite)
		{
			this.Wrappee.CreateAsHardLink(path, overwrite);

			return this;
		}
	}
}
