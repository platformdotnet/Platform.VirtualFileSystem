using System;
using System.Collections.Generic;
using System.Net;
using Platform.Collections;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkFileSystemManager
		: AbstractFileSystemManager
	{
		private readonly IPAddress ipAddress;
		private readonly IDictionary<string, IFileSystem> fileSystemCache;

		public NetworkFileSystemManager(IPAddress ipAddress)
		{
			this.ipAddress = ipAddress;
			this.fileSystemCache = new TimedReferenceDictionary<string, IFileSystem>(TimeSpan.FromMinutes(5));
		}

		public override INode Resolve(string uri, NodeType nodeType, AddressScope scope, FileSystemOptions options)
		{
			bool success;
			IFileSystem fileSystem;

			var address = LayeredNodeAddress.Parse(uri);
						
			lock (this.fileSystemCache)
			{
				success = this.fileSystemCache.TryGetValue(address.RootUri, out fileSystem);
			}

			if (!success)
			{
				var networkUri = String.Format("netvfs://{0}[" + uri + "]/", this.ipAddress);

				fileSystem = FileSystemManager.GetManager().ResolveDirectory(networkUri).FileSystem;

				lock (this.fileSystemCache)
				{
					this.fileSystemCache.Add(address.RootUri, fileSystem);
				}
			}

			return fileSystem.Resolve(address.PathAndQuery);
		}
	}
}
