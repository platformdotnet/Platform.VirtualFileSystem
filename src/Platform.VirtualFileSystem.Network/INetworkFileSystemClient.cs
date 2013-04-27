using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Platform;

namespace Platform.VirtualFileSystem.Network.Client
{
	interface INetworkFileSystemClient
	{
		IEnumerable<string> List(string uri);
		IEnumerable<Pair<string, Pair<string, object>>> ListAttributes(string uri);

		void Rename(string uri, string newName);
		void Move(string srcUri, string desUri);
		void Copy(string srcUri, string desUri);
		void Delete(string uri, bool recursive);

		IEnumerable<Pair<string, object>> GetAttributes(string uri);
		void SetAttributes(string uri, IEnumerable<Pair<string, object>> attributes);

		Stream OpenStream(string uri, FileMode fileMode, FileAccess fileAccess, FileShare fileShare);
		
		IEnumerable<string> ComputeHash(string uri);
	}
}
