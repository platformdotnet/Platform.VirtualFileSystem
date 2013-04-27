using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem
{
	public class DirectoryHashingServiceType
		: HashingServiceType
	{
		public virtual bool Recursive { get; set; }

		public virtual Predicate<IFile> FileFilter { get; set; }

		public virtual IEnumerable<string> IncludedFileAttributes { get; set; }

		public virtual IEnumerable<string> IncludedDirectoryAttributes { get; set; }

		public virtual Predicate<IDirectory> DirectoryFilter { get; set; }

		public DirectoryHashingServiceType(bool recursive)
			: this(recursive, "md5")
		{
		}

		public DirectoryHashingServiceType(bool recursive, string algorithmName)
			: base(typeof(IStreamHashingService), algorithmName)
		{
			this.Recursive = recursive;
			this.IncludedFileAttributes = new string[0];
			this.IncludedDirectoryAttributes = new string[0];
		}
	}
}