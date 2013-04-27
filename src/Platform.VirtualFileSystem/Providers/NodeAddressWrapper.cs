using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Platform.VirtualFileSystem.Providers
{
	[Serializable]
	public class NodeAddressWrapper
		: INodeAddress
	{
		private readonly INodeAddress wrappee;

		public virtual string InnerUri
		{
			get { return this.Wrappee.InnerUri; }
		}

		public virtual INodeAddress Wrappee
		{
			get { return this.wrappee; }
		}

		public NodeAddressWrapper(INodeAddress wrappee)
		{
			this.wrappee = wrappee;
		}

		public virtual string Name
		{
			get { return this.Wrappee.Name; }
		}

		public virtual string NameWithoutQuery
		{
			get { return this.Wrappee.NameWithoutQuery; }
		}


		public virtual string NameWithoutExtension
		{
			get { return this.Wrappee.NameWithoutExtension; }
		}

		public virtual string NameAndQuery
		{
			get { return this.Wrappee.NameAndQuery; }
		}

		public virtual string Query
		{
			get { return this.Wrappee.Query; }
		}

		public virtual NameValueCollection QueryValues
		{
			get { return this.Wrappee.QueryValues; }

		}

		public virtual string PathAndQuery
		{
			get { return this.Wrappee.PathAndQuery; }
		}

		public virtual string ShortName
		{
			get { return this.Wrappee.ShortName; }
		}

		public virtual string AbsolutePath
		{
			get { return this.Wrappee.AbsolutePath; }
		}

		public virtual string Extension
		{
			get { return this.Wrappee.Extension; }
		}

		public virtual int Depth
		{
			get { return this.Wrappee.Depth; }
		}

		public virtual string Scheme
		{
			get { return this.Wrappee.Scheme; }
		}

		public virtual string Uri
		{
			get { return this.Wrappee.Uri; }
		}

		public virtual string RootUri
		{
			get { return this.Wrappee.RootUri; }
		}

		public virtual INodeAddress Parent
		{
			get { return this.Wrappee.Parent; }
		}

		public virtual INodeAddress ResolveAddress(string name)
		{
			return this.Wrappee.ResolveAddress(name);
		}

		public virtual INodeAddress ResolveAddress(string name, AddressScope scope)
		{
			return this.Wrappee.ResolveAddress(name, scope);
		}

		public virtual string GetRelativePathTo(INodeAddress name)
		{
			return this.Wrappee.GetRelativePathTo(name);
		}

		public virtual bool IsAncestorOf(INodeAddress name)
		{
			return this.Wrappee.IsAncestorOf(name);
		}

		public virtual bool IsDescendentOf(INodeAddress name)
		{
			return this.Wrappee.IsDescendentOf(name);
		}

		public virtual bool IsDescendentOf(INodeAddress name, AddressScope scope)
		{
			return this.Wrappee.IsDescendentOf(name, scope);
		}

		public virtual bool RootsAreEqual(INodeAddress nodeAddress)
		{
			return this.Wrappee.RootsAreEqual(nodeAddress);
		}

		public virtual bool IsRoot
		{
			get { return this.Wrappee.IsRoot; }
		}

		public virtual bool IsDescendentOf(INodeAddress name, StringComparison comparisonType, AddressScope scope)
		{
			return this.Wrappee.IsDescendentOf(name, comparisonType, scope);
		}

		public virtual bool ParentsEqual(INodeAddress nodeAddress)
		{
			return this.Wrappee.ParentsEqual(nodeAddress);
		}

		public virtual bool ParentsEqual(INodeAddress nodeAddress, StringComparison comparisonType)
		{
			return this.Wrappee.ParentsEqual(nodeAddress, comparisonType);
		}

		public virtual string PathToDepth(int depth)
		{
			return this.Wrappee.PathToDepth(depth);
		}

		public virtual string GetRelativePathTo(string absolutePath)
		{
			return this.Wrappee.GetRelativePathTo(absolutePath);
		}

		public virtual string DisplayUri
		{
			get { return this.Wrappee.DisplayUri; }
		}
	}
}
