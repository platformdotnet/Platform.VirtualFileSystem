using System;
using System.IO;
using System.Text;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>INodeContent</c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="INodeContent"/>
	/// </summary>
	public abstract class AbstractNodeContent
		: MarshalByRefObject, INodeContent
	{
		public abstract void Delete();
		
		public virtual Stream OpenStream(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			return DoOpenStream(fileMode, fileAccess, fileShare);
		}

		protected virtual Stream DoOpenStream(FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			throw new NotSupportedException();
		}

		public virtual TextReader GetReader()
		{
			return new StreamReader(GetInputStream());
		}

		public virtual TextReader GetReader(FileShare sharing)
		{
			return new StreamReader(GetInputStream(sharing));
		}

		public virtual TextReader GetReader(out Encoding encoding)
		{
			return GetReader(out encoding, FileShare.ReadWrite);
		}

		public virtual TextReader GetReader(out Encoding encoding, FileShare sharing)
		{
			string encodingName;
			StreamReader reader;

			var stream = this.GetInputStream(out encodingName, sharing);
			
			if (encodingName == null)
			{
				reader = new StreamReader(stream);
			}
			else
			{
				var enc = Encoding.GetEncoding(encodingName);

				if (enc != null)
				{
					reader = new StreamReader(stream, enc);
				}
				else
				{
					reader = new StreamReader(stream);
				}
			}

			encoding = reader.CurrentEncoding;

			return reader;
		}

		public virtual TextWriter GetWriter()
		{
			return new StreamWriter(GetOutputStream());
		}

		public virtual TextWriter GetWriter(FileShare sharing)
		{
			return new StreamWriter(GetOutputStream(sharing));
		}

		public virtual TextWriter GetWriter(Encoding encoding)
		{
			return new StreamWriter(GetOutputStream(), encoding);
		}

		public virtual TextWriter GetWriter(Encoding encoding, FileShare sharing)
		{
			return new StreamWriter(GetOutputStream(sharing), encoding);
		}

		public virtual Stream GetInputStream()
		{
			string encoding;

			return GetInputStream(out encoding);
		}

		public virtual Stream GetInputStream(FileShare sharing)
		{
			string encoding;

			return GetInputStream(out encoding, sharing);
		}

		public virtual Stream GetInputStream(out string encoding)
		{
			return GetInputStream(out encoding, FileShare.ReadWrite);
		}

		public virtual Stream GetInputStream(out string encoding, FileMode mode)
		{
			return GetInputStream(out encoding, mode, FileShare.ReadWrite);
		}

		public virtual Stream GetInputStream(FileMode mode)
		{
			string encoding;

			return GetInputStream (out encoding, mode, FileShare.ReadWrite);
		}

		public virtual Stream GetInputStream(FileMode mode, FileShare sharing)
		{
			string encoding;

			return GetInputStream (out encoding, mode, sharing);
		}

		public virtual Stream GetInputStream(out string encoding, FileMode mode, FileShare sharing)
		{
			return DoGetInputStream(out encoding, mode, sharing);
		}

		protected abstract Stream DoGetInputStream(out string encoding, FileMode mode, FileShare sharing);

		public virtual Stream GetOutputStream()
		{
			return GetOutputStream(Encoding.UTF8.EncodingName);
		}

		public virtual Stream GetOutputStream(string encoding)
		{
			return GetOutputStream(encoding, FileShare.Read);
		}

		public virtual Stream GetOutputStream(FileShare sharing)
		{
			return GetOutputStream(Encoding.UTF8.EncodingName, sharing);
		}

		public virtual Stream GetOutputStream(FileMode mode)
		{
			return GetOutputStream(mode, FileShare.Read);
		}

		public virtual Stream GetOutputStream(FileMode mode, FileShare sharing)
		{
			return GetOutputStream(Encoding.UTF8.EncodingName, mode, sharing);
		}

		public virtual Stream GetOutputStream(string encoding, FileMode mode)
		{
			return GetOutputStream(encoding, mode, FileShare.Read);
		}

		public virtual Stream GetOutputStream(string encoding, FileMode mode, FileShare sharing)
		{
			return DoGetOutputStream(encoding, mode, sharing);
		}

		protected abstract Stream DoGetOutputStream(string encoding, FileMode mode, FileShare sharing);

		public virtual Stream GetInputStream(out string encoding, FileShare sharing)
		{
			return GetInputStream(out encoding, FileMode.Open, sharing);
		}

		public virtual Stream GetOutputStream(string encoding, FileShare sharing)
		{
			return GetOutputStream(encoding, FileMode.Create, sharing);
		}
	}
}
