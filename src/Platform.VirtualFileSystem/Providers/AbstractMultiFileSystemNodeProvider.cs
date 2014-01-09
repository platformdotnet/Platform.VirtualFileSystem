using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractMultiFileSystemNodeProvider
		: AbstractNodeProvider
	{
		protected struct FileSystemOptionsCacheKey
		{
			public readonly object key;
			public readonly FileSystemOptions options;

			public FileSystemOptionsCacheKey(object key, FileSystemOptions options)
				: this()
			{
				this.key = key;
				this.options = options;
			}
		}

		protected class FileSystemOptionsCacheKeyComparer
			: IEqualityComparer<FileSystemOptionsCacheKey>
		{
			public static readonly FileSystemOptionsCacheKeyComparer Default = new FileSystemOptionsCacheKeyComparer();

			public bool Equals(FileSystemOptionsCacheKey x, FileSystemOptionsCacheKey y)
			{
				return Object.Equals(x.key, y.key) && Object.ReferenceEquals(x.options, y.options);
			}

			public int GetHashCode(FileSystemOptionsCacheKey obj)
			{
				var retval = 0;

				if (obj.key != null)
				{
					retval ^= obj.GetHashCode();
				}

				retval ^= RuntimeHelpers.GetHashCode(obj.options);

				return retval;
			}
		}

		private readonly IDictionary<FileSystemOptionsCacheKey, IFileSystem> fileSystems = new Dictionary<FileSystemOptionsCacheKey, IFileSystem>(FileSystemOptionsCacheKeyComparer.Default);

		protected AbstractMultiFileSystemNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		protected void AddFileSystem(object key, IFileSystem fileSystem, FileSystemOptions options)
		{
			this.fileSystems[new FileSystemOptionsCacheKey(key, options)] = fileSystem;
		}

		protected void RemoveFileSystem(object key, IFileSystem fileSystem, FileSystemOptions options)
		{
			this.fileSystems.Remove(new FileSystemOptionsCacheKey(key, options));
		}

		protected virtual IFileSystem FindFileSystem(object key, FileSystemOptions options)
		{
			IFileSystem retval;

			if (this.fileSystems.TryGetValue(new FileSystemOptionsCacheKey(key, options), out retval))
			{
				return retval;
			}

			return null;
		}

		public override INode Find(INodeResolver resolver, string uri, NodeType nodeType, FileSystemOptions options)
		{
			var nodeAddress = ParseUri(uri);

			return Find(nodeAddress, nodeType, options);
		}

		protected virtual INode Find(INodeAddress nodeAddress, NodeType nodeType, FileSystemOptions options)
		{
			var rootAddress = nodeAddress.ResolveAddress(FileSystemManager.RootPath);

			options = options ?? FileSystemOptions.Default;

			var fileSystem = this.FindFileSystem(rootAddress, options);

			if (fileSystem == null)
			{
				bool cache;

				fileSystem = NewFileSystem(rootAddress, options, out cache);

				fileSystem.Closed += FileSystem_Closed;

				if (cache)
				{
					AddFileSystem(rootAddress, fileSystem, options);
					AddFileSystem(rootAddress, fileSystem, fileSystem.Options);
				}
			}

			return fileSystem.Resolve(nodeAddress, nodeType);
		}

		protected abstract IFileSystem NewFileSystem(INodeAddress rootAddress, FileSystemOptions options, out bool cache);
			
		protected abstract INodeAddress ParseUri(string uri);

		private void FileSystem_Closed(object sender, EventArgs e)
		{
			lock (this)
			{
				var fileSystem = (IFileSystem)sender;

				RemoveFileSystem(fileSystem.RootDirectory.Address, fileSystem, fileSystem.Options);
			}
		}
	}
}
