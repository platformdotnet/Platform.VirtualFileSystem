using System;
using System.Collections.Generic;
using System.Diagnostics;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Network
{
	public class NetworkDirectory
		: AbstractDirectory
	{
		private IDictionary<string, INode> children;
		private Pair<DirectoryRefreshMask, int> directoryRefreshInfo;

		public new NetworkNodeAddress Address
		{
			get
			{
				return (NetworkNodeAddress)base.Address;
			}
		}

		protected internal NetworkDirectory(IFileSystem fileSystem, INodeAddress address)
			: base(fileSystem, address)
		{
			this.directoryRefreshInfo = new Pair<DirectoryRefreshMask, int> {Left = DirectoryRefreshMask.All, Right = 0};
		}

		public override IDirectory Refresh(DirectoryRefreshMask mask)
		{
			if ((mask & DirectoryRefreshMask.Children) != 0)				
			{
				lock (this.SyncLock)
				{
					this.directoryRefreshInfo.Left |= mask;
					this.directoryRefreshInfo.Right++;
				}
			}

			return base.Refresh(mask);
		}

		public override IService GetService(ServiceType serviceType)
		{
			if (serviceType is DirectoryHashingServiceType)
			{
				return new NetworkDirectoryHashingService(this, (DirectoryHashingServiceType)serviceType);
			}

			return base.GetService(serviceType);
		}

		[ThreadStatic]
		private static IDictionary<string, INode> threadLocalNewChildren;

		public override IEnumerable<INode> DoGetChildren(NodeType nodeType, Predicate<INode> acceptNode)
		{
			var listedAllChildren = false;
			IDictionary<string, INode> newChildren;
			Pair<DirectoryRefreshMask, int> refreshInfo;
			
			foreach (INode node in this.GetJumpPoints(nodeType, acceptNode))
			{
				yield return node;
			}
						
			var shouldLoadFromNetwork = false;
						
			lock (this.SyncLock)
			{
				refreshInfo = this.directoryRefreshInfo;

				if ((refreshInfo.Left & DirectoryRefreshMask.Children) != 0)
				{
					shouldLoadFromNetwork = true;
				}

				if (this.children == null)
				{
					this.children = new SortedDictionary<string, INode>(StringComparer.CurrentCultureIgnoreCase);
				}
			}

			if (shouldLoadFromNetwork)
			{
				string regex = null;
				var skipAcceptNode = false; ;
				var toRemove = new List<string>();

				var newAttributes = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

				if (NetworkDirectory.threadLocalNewChildren == null)
				{
					NetworkDirectory.threadLocalNewChildren = new SortedDictionary<string, INode>(StringComparer.CurrentCultureIgnoreCase);
				}
				else
				{
					NetworkDirectory.threadLocalNewChildren.Clear();
				}

				newChildren = NetworkDirectory.threadLocalNewChildren;

				try
				{
					if (RegexBasedNodePredicateHelper.IsRegexBasedNodePredicate(acceptNode)
						/* Only load based on REGEX if not refreshing AllChildren */
						&& refreshInfo.Left != DirectoryRefreshMask.AllChildren)
					{
						regex = RegexBasedNodePredicateHelper.GetRegexFromRegexBasedNodePredicate(acceptNode).ToString();

						if (RegexBasedNodePredicateHelper.PredicateNameBasedOnly(acceptNode))
						{
							skipAcceptNode = true;
						}
					}

					using (var freeClientContext = ((NetworkFileSystem)this.FileSystem).GetFreeClientContext())
					{
						var networkclient = freeClientContext.NetworkFileSystemClient;

						IEnumerator<NetworkFileSystemEntry> enumerator;

						var enumerable = networkclient.ListAttributes(this.Address.RemoteUri, regex);

						using (enumerator = enumerable.GetEnumerator())
						{
							while (true)
							{
								try
								{
									if (!enumerator.MoveNext())
									{
										break;
									}
								}
								catch (DirectoryNodeNotFoundException)
								{
									lock (this.SyncLock)
									{
										foreach (var node in this.children.Values)
										{
											((NetworkNodeAndFileAttributes)node.Attributes).Clear();
											((NetworkNodeAndFileAttributes)node.Attributes).SetValue<bool>("exists", false);
										}

										this.children.Clear();
									}

									throw;
								}

								var entry = enumerator.Current;

								INode child = null;

								if (entry.NodeType == NodeType.Directory)
								{
									child = this.ResolveDirectory(entry.Name);
								}
								else
								{
									child = this.ResolveFile(entry.Name);
								}

								var attributes = (NetworkNodeAndFileAttributes)child.Attributes;

								newAttributes.Clear();

								foreach (var attribute in entry.ReadAttributes())
								{
									newAttributes[attribute.Key] = attribute.Value;
								}

								try
								{
									lock (attributes.SyncLock)
									{
										toRemove.Clear();

										foreach (var attribute in newAttributes)
										{
											attributes.SetValue<object>(attribute.Key, attribute.Value);
										}

										foreach (var name in attributes.Names)
										{
											if (!newAttributes.ContainsKey(name))
											{
												toRemove.Add(name);
											}
										}

										foreach (var name in toRemove)
										{
											attributes.SetValue<object>(name, null);
										}

										attributes.SetValue<bool>("exists", true);
									}

									newChildren.Add(child.Name, child);
								}
								catch (Exception)
								{
									Debugger.Break();
								}

								if ((nodeType.Equals(NodeType.Any) || child.NodeType.Equals(nodeType))
									&& (skipAcceptNode || acceptNode(child)))
								{
									yield return child;
								}
							}
						}

						if (regex == null)
						{
							listedAllChildren = true;
						}
					}
				}
				finally
				{
					lock (this.SyncLock)
					{
						if (listedAllChildren)
						{
							toRemove.Clear();

							foreach (var node in this.children.Values)
							{
								if (!newChildren.ContainsKey(node.Name))
								{
									((NetworkNodeAndFileAttributes)node.Attributes).Clear();
									((NetworkNodeAndFileAttributes)node.Attributes).SetValue<bool>("exists", false);

									toRemove.Add(node.Name);									
								}
							}

							foreach (var name in toRemove)
							{
								this.children.Remove(name);
							}

							if (this.directoryRefreshInfo.Right == refreshInfo.Right)
							{
								this.directoryRefreshInfo.Left = DirectoryRefreshMask.None;
							}
						}

						foreach (var node in newChildren.Values)
						{
							this.children[node.Name] = node;
						}
					}

					toRemove.Clear();
				}
					
				yield break;
			}
			else
			{
				var retvals = new List<INode>();

				// Copy the children because we want to yield outside the lock

				lock (this.SyncLock)
				{
					foreach (var pair in this.children)
					{
						if ((nodeType.Equals(NodeType.Any) || pair.Value.NodeType.Equals(nodeType)) && acceptNode(pair.Value))
						{
							retvals.Add(pair.Value);
						}
					}
				}

				foreach (var node in retvals)
				{
					yield return node;
				}
			}
		}

		internal virtual void AddChild(INode node)
		{
			lock (this.SyncLock)
			{
				if (this.children == null)
				{
					this.children = new SortedDictionary<string, INode>(StringComparer.CurrentCultureIgnoreCase);
				}

				this.children[node.Name] = node;
			}
		}

		internal virtual void RemoveChild(INode node)
		{
			lock (this.SyncLock)
			{
				if (this.children == null)
				{
					return;
				}

				this.children.Remove(node.Name);
			}
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new NetworkNodeAndFileAttributes(this);
		}

		public override INode DoCreate(bool createParent)
		{
			NetworkNode.DoCreate(this, createParent);

			return this;
		}

		protected override INode DoDelete()
		{
			NetworkNode.DoDelete(this, false);

			return this;
		}

		protected override IDirectory DoDelete(bool recursive)
		{
			NetworkNode.DoDelete(this, true);

			return this;
		}

		protected override INode DoCopyTo(INode target, bool overwrite)
		{
			NetworkNode.DoCopyTo(this, target, base.CopyTo, overwrite);

			return this;
		}
 
		protected override INode DoMoveTo(INode target, bool overwrite)
		{
			NetworkNode.DoMoveTo(this, target, base.MoveTo, overwrite);

			return this;
		}

		protected override IFileSystem DoCreateView(string scheme, FileSystemOptions options)
		{
			return new NetworkFileSystem(NetworkNodeAddress.CreateAsRoot(scheme, this.Address, true), options);
		}
	}
}
