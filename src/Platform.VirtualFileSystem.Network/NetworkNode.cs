using System;
using System.Collections.Generic;

namespace Platform.VirtualFileSystem.Network
{
	public static class NetworkNode
	{
		internal static void DoCreate(INode thisNode, bool createParent)
		{
			using (NetworkFileSystem.FreeClientContext clientContext = ((NetworkFileSystem)thisNode.FileSystem).GetFreeClientContext())
			{				
				clientContext.NetworkFileSystemClient.Create(((NetworkNodeAddress)thisNode.Address).RemoteUri, thisNode.NodeType, createParent);

				lock (thisNode.Attributes.SyncLock)
				{
					((NetworkNodeAndFileAttributes)thisNode.Attributes).SetValue<bool>("exists", true);
				}

				if (((NetworkFileSystem)thisNode.FileSystem).ShouldSupportSynthesizedActivityEvents)
				{
					NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)thisNode.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Created, thisNode.NodeType, thisNode.Name, thisNode.Address.AbsolutePath));
				}
			}
		}

		internal static void DoDelete(INode thisNode, bool recursive)
		{
			using (NetworkFileSystem.FreeClientContext clientContext = ((NetworkFileSystem)thisNode.FileSystem).GetFreeClientContext())
			{
				clientContext.NetworkFileSystemClient.Delete(((NetworkNodeAddress)thisNode.Address).RemoteUri, thisNode.NodeType, recursive);

				lock (thisNode.Attributes)
				{
					((NetworkNodeAndFileAttributes)thisNode.Attributes).Clear();
					((NetworkNodeAndFileAttributes)thisNode.Attributes).SetValue<bool>("exists", false, false);
				}
				
				if (((NetworkFileSystem)thisNode.FileSystem).ShouldSupportSynthesizedActivityEvents)
				{
					NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)thisNode.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Deleted, thisNode.NodeType, thisNode.Name, thisNode.Address.AbsolutePath));

					if (!thisNode.Address.IsRoot && thisNode.ParentDirectory is NetworkDirectory)
					{
						((NetworkDirectory)thisNode.ParentDirectory).RemoveChild(thisNode);
					}
				}
			}
		}

		internal static HashValue ComputeHash(INode thisNode, string algorithmName, bool recursive, long offset, long length, IEnumerable<string> fileAttributes, IEnumerable<string> dirAttributes)
		{
			NetworkFileSystem fileSystem;
			NetworkFileSystem.FreeClientContext clientContext;

			fileSystem = (NetworkFileSystem)thisNode.FileSystem;

			using (clientContext = fileSystem.GetFreeClientContext())
			{
				NetworkNodeAddress address;

				address = (NetworkNodeAddress)thisNode.Address;

				return clientContext.NetworkFileSystemClient.ComputeHash(address.RemoteUri, thisNode.NodeType, algorithmName, recursive, offset, length, fileAttributes, dirAttributes);
			}			
		}

		internal static void DoCopyTo(INode thisNode, INode target, Func<INode, bool, INode> baseCall, bool overwrite)
		{
			DoCopyOrMove
			(
				thisNode,
				target,
				overwrite,
				baseCall,
				delegate(NetworkFileSystem.FreeClientContext clientContext)
				{
					return clientContext.NetworkFileSystemClient.Copy;
				}
			);
						
			((NetworkNodeAndFileAttributes)target.Attributes).SetValue<bool>("exists", true);

			List<KeyValuePair<string, object>> attributes;

			attributes = new List<KeyValuePair<string, object>>();

			// Copy attributes to target node
						
			lock (thisNode.Attributes.SyncLock)
			{
				foreach (string name in thisNode.Attributes.Names)
				{
					attributes.Add(new KeyValuePair<string, object>(name, thisNode.Attributes[name]));
				}
			}

			lock (target.Attributes.SyncLock)
			{
				((NetworkNodeAndFileAttributes)target.Attributes).SetValue<bool>("exists", true);

				foreach (KeyValuePair<string, object> attribute in attributes)
				{
					target.Attributes[attribute.Key] = attribute.Value;
				}
			}
			
			if (target.ParentDirectory is NetworkDirectory)
			{
				((NetworkDirectory)target.ParentDirectory).AddChild(target);
			}

			if (((NetworkFileSystem)thisNode.FileSystem).ShouldSupportSynthesizedActivityEvents)
			{
				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)target.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Created, target.NodeType, target.Name, target.Address.AbsolutePath));
				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)target.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Changed, target.NodeType, target.Name, target.Address.AbsolutePath));
			}
		}

		internal static void DoMoveTo(INode thisNode, INode target, Func<INode, bool, INode> baseCall, bool overwrite)
		{
			DoCopyOrMove
			(
				thisNode,
				target,
				overwrite,
				baseCall,
				delegate(NetworkFileSystem.FreeClientContext clientContext)
				{
					return clientContext.NetworkFileSystemClient.Move;
				}
			);

			var attributes = new List<KeyValuePair<string,object>>();

			// Copy attributes to target node

			lock (thisNode.Attributes.SyncLock)
			{
				foreach (string name in thisNode.Attributes.Names)
				{
					attributes.Add(new KeyValuePair<string, object>(name, thisNode.Attributes[name]));
				}
			}

			lock (target.Attributes.SyncLock)
			{
				((NetworkNodeAndFileAttributes)target.Attributes).SetValue<bool>("exists", true);

				foreach (KeyValuePair<string, object> attribute in attributes)
				{
					target.Attributes[attribute.Key] = attribute.Value;
				}
			}

			// Make source not exist

			lock (thisNode.Attributes.SyncLock)
			{
				((NetworkNodeAndFileAttributes)thisNode.Attributes).Clear();
				((NetworkNodeAndFileAttributes)thisNode.Attributes).SetValue<bool>("exists", false);
			}

			if (target.ParentDirectory is NetworkDirectory)
			{
				((NetworkDirectory)target.ParentDirectory).AddChild(target);
			}

			if (((NetworkFileSystem)thisNode.FileSystem).ShouldSupportSynthesizedActivityEvents)
			{
				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)target.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Created, target.NodeType, target.Name, target.Address.AbsolutePath));
				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)target.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Changed, target.NodeType, target.Name, target.Address.AbsolutePath));

				NetworkFileSystem.RaiseActivityEvent((NetworkFileSystem)thisNode.FileSystem, new FileSystemActivityEventArgs(FileSystemActivity.Deleted, thisNode.NodeType, thisNode.Name, thisNode.Address.AbsolutePath));

				if (thisNode.ParentDirectory is NetworkDirectory)
				{
					((NetworkDirectory)thisNode.ParentDirectory).RemoveChild(thisNode);
				}
			}
		}

		internal static void DoCopyOrMove(INode thisNode, INode target, bool overwrite, Func<INode, bool, INode> baseCall, Func<NetworkFileSystem.FreeClientContext, Action<string, string, NodeType, bool>> clientCall)
		{
			IFile targetFile;
			NetworkNodeAddress address1, address2;
			NetworkFileSystem.FreeClientContext clientContext = null;

			targetFile = target.OperationTargetDirectory.ResolveFile(target.Address.Name);

			address1 = (NetworkNodeAddress)targetFile.Address;
			address2 = (NetworkNodeAddress)thisNode.Address;

			if (!(address1.ServerName == address2.ServerName
				&& address1.Port == address2.Port))
			{
				baseCall(target, overwrite);

				return;
			}

			// Get a new client context

			using (clientContext = ((NetworkFileSystem)thisNode.FileSystem).GetFreeClientContext())
			{
				clientCall(clientContext)(address2.RemoteUri, address1.RemoteUri, NodeType.File, overwrite);
			}
		}
	}
}
