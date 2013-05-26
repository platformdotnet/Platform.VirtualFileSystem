using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractMultiFileSystemNodeProvider
		: AbstractNodeProvider
	{
		private readonly IDictionary<Pair<object, FileSystemOptions>, IFileSystem> fileSystems = new Dictionary<Pair<object, FileSystemOptions>, IFileSystem>();

		protected AbstractMultiFileSystemNodeProvider(IFileSystemManager manager)
			: base(manager)
		{
		}

		protected void AddFileSystem(object key, IFileSystem fileSystem, FileSystemOptions options)
		{
			this.fileSystems[new Pair<object, FileSystemOptions>(key, options)] = fileSystem;
		}

		protected void RemoveFileSystem(object key, IFileSystem fileSystem, FileSystemOptions options)
		{
			this.fileSystems.Remove(new Pair<object, FileSystemOptions>(key, options));
		}

		protected virtual IFileSystem FindFileSystem(object key, FileSystemOptions options)
		{
			IFileSystem retval;

			if (this.fileSystems.TryGetValue(new Pair<object, FileSystemOptions>(key, options), out retval))
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

			var fileSystem = this.FindFileSystem(rootAddress, options);

			if (fileSystem == null)
			{
				bool cache;

				options = options ?? FileSystemOptions.NewDefault();

				fileSystem = NewFileSystem(rootAddress, options, out cache);

				fileSystem.Closed += FileSystem_Closed;

				if (cache)
				{
					AddFileSystem(rootAddress, fileSystem, options);
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
