using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Platform.VirtualFileSystem.Network.Client
{
	public interface INetworkFileSystemClient
		: IDisposable
	{
		object SyncLock
		{
			get;
		}

		IPEndPoint ServerEndPoint
		{
			get;
		}

		bool Connected
		{
			get;
		}

		void Connect();
		void Disconnect();

		void Create(string uri, NodeType nodeType, bool createParent);
		void Delete(string uri, NodeType nodeType, bool recursive);
		void Move(string srcUri, string desUri, NodeType nodeType, bool overwrite);
		void Copy(string srcUri, string desUri, NodeType nodeType, bool overwrite);

		HashValue ComputeHash(string uri, NodeType nodeType, string algorithm, bool recursive, long offset, long length, IEnumerable<string> fileAttributes, IEnumerable<string> dirAttributes);
	
		HashValue ComputeHash(Stream stream, string algorithm, long offset, long length);

		void CreateHardLink(string linkUri, string targetUri, bool overwrite);

		Stream OpenRandomAccessStream(string uri, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

		IEnumerable<Pair<string, object>> GetAttributes(string uri, NodeType nodeType);
		void SetAttributes(string uri, NodeType nodeType, IEnumerable<Pair<string, object>> attributes);

		TimeSpan Ping();

		IEnumerable<Pair<string, NodeType>> List(string uri, string regex);
		IEnumerable<NetworkFileSystemEntry> ListAttributes(string uri, string regex);
	}
}
