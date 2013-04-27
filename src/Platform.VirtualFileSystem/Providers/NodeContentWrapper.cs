using System;
using System.IO;
using System.Text;

namespace Platform.VirtualFileSystem.Providers
{
	public class NodeContentWrapper
		: MarshalByRefObject, INodeContent
	{
		protected virtual INodeContent Wrappee { get; private set; }

		public NodeContentWrapper(INodeContent wrappee)
		{
			this.Wrappee = wrappee;
		}

		public virtual void Delete()
		{
			this.Wrappee.Delete();
		}

		public virtual Stream OpenStream(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			return this.Wrappee.OpenStream(fileMode, fileAccess, fileShare);
		}

		public virtual Stream GetInputStream()
		{
			return this.Wrappee.GetInputStream();
		}

		public virtual Stream GetInputStream(out string encoding)
		{
			return this.Wrappee.GetInputStream(out encoding);
		}

		public virtual Stream GetInputStream(FileShare sharing)
		{
			return this.Wrappee.GetInputStream(sharing);
		}

		public virtual Stream GetInputStream(out string encoding, FileShare sharing)
		{
			return this.Wrappee.GetInputStream(out encoding, sharing);
		}

		public virtual Stream GetInputStream(FileMode mode)
		{
			return this.Wrappee.GetInputStream(mode);
		}

		public virtual Stream GetInputStream(FileMode mode, FileShare sharing)
		{
			return this.Wrappee.GetInputStream(mode, sharing);
		}

		public virtual Stream GetInputStream(out string encoding, FileMode mode)
		{
			return this.Wrappee.GetInputStream(out encoding, mode);
		}

		public virtual Stream GetInputStream(out string encoding, FileMode mode, FileShare sharing)
		{
			return this.Wrappee.GetInputStream(out encoding, mode, sharing);
		}

		public virtual Stream GetOutputStream()
		{
			return this.Wrappee.GetOutputStream();
		}

		public virtual Stream GetOutputStream(FileShare sharing)
		{
			return this.Wrappee.GetOutputStream(sharing);
		}

		public virtual Stream GetOutputStream(FileMode mode)
		{
			return this.Wrappee.GetOutputStream(mode);
		}

		public virtual Stream GetOutputStream(string encoding, FileMode mode)
		{
			return this.Wrappee.GetOutputStream(encoding, mode);
		}

		public virtual Stream GetOutputStream(string encoding)
		{
			return this.Wrappee.GetOutputStream(encoding);
		}

		public virtual Stream GetOutputStream(string encoding, FileShare sharing)
		{
			return this.Wrappee.GetOutputStream(encoding, sharing);
		}

		public virtual Stream GetOutputStream(string encoding, FileMode mode, FileShare sharing)
		{
			return this.Wrappee.GetOutputStream(encoding, mode, sharing);
		}

		public virtual Stream GetOutputStream(FileMode mode, FileShare sharing)
		{
			return this.Wrappee.GetOutputStream(mode, sharing);
		}

		public virtual TextReader GetReader()
		{
			return this.Wrappee.GetReader();
		}

		public virtual TextReader GetReader(FileShare sharing)
		{
			return this.Wrappee.GetReader(sharing);
		}

		public virtual TextReader GetReader(out Encoding encoding)
		{
			return this.Wrappee.GetReader(out encoding);
		}

		public virtual TextReader GetReader(out Encoding encoding, FileShare sharing)
		{
			return this.Wrappee.GetReader(out encoding, sharing);
		}

		public virtual TextWriter GetWriter()
		{
			return this.Wrappee.GetWriter();
		}

		public virtual TextWriter GetWriter(FileShare sharing)
		{
			return this.Wrappee.GetWriter(sharing);
		}

		public virtual TextWriter GetWriter(Encoding encoding)
		{
			return this.Wrappee.GetWriter(encoding);
		}

		public virtual TextWriter GetWriter(Encoding encoding, FileShare sharing)
		{
			return this.Wrappee.GetWriter(encoding, sharing);
		}

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode ();
		}

		public override string ToString()
		{
			return this.Wrappee.ToString();
		}
	}
}
