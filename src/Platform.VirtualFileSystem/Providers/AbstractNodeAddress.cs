using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Collections;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers
{
	/// <summary>
	/// This class provides a skeletal implementation of the <c>INodeAddress</c>interface to minimize the effort 
	/// required to implement the interface.
	/// <seealso cref="INodeAddress"/>
	/// </summary>
	[Serializable]
	public abstract class AbstractNodeAddress
		: INodeAddress
	{
		private readonly string scheme;
		private readonly string absolutePath;

		// Cached calculated values.

		private string uri;
		private string rootUri;		
		private string extension;
		
		/// <summary>
		/// Initialises a new <see cref="AbstractNodeAddress"/>.
		/// </summary>
		/// <param name="scheme">The scheme of the <c>FileName</c>.</param>
		/// <param name="absolutePath">The <c>AbsolutePath</c> of the <c>FileName</c>.</param>
		protected AbstractNodeAddress(string scheme, string absolutePath, string query)
		{
			this.scheme = scheme;
			this.absolutePath = absolutePath;
			this.Query = query;
		}

		public virtual string InnerUri
		{
			get
			{
				return "";
			}
		}
		
		public virtual string Name
		{
			get
			{
				return this.NameAndQuery;
			}
		}

		public virtual string NameWithoutExtension
		{
			get
			{
				int x;
				string s = this.ShortName;

				x = s.LastIndexOf('.');

				if (x >= 0)
				{
					return s.Substring(0, x);
				}
				else
				{
					return s;
				}
			}
		}

		public virtual string NameWithoutQuery
		{
			get
			{
				if (this.nameWithoutQuery == null)
				{
					lock (this)
					{
						if (this.IsRoot)
						{						
							this.nameWithoutQuery = this.absolutePath;
						}
						else
						{
							this.nameWithoutQuery = TextConversion.FromEscapedHexString(this.absolutePath.Right(PredicateUtils.ObjectEquals(FileSystemManager.SeperatorChar).Not()));
						}
					}
				}

				return this.nameWithoutQuery;
			}
		}
		private string nameWithoutQuery;

		public virtual string NameAndQuery
		{
			get
			{
				if (this.nameAndQuery == null)
				{
					lock (this)
					{
						if (this.nameAndQuery == null)
						{
							string nameAndQuery;

							nameAndQuery = this.ShortName + (this.Query.Length > 0 ? '?' + this.Query : "");

							System.Threading.Thread.MemoryBarrier();

							this.nameAndQuery = nameAndQuery;
						}						
					}
				}

				return this.nameAndQuery;
			}
		}
		private string nameAndQuery;

		public virtual string Query { get; private set; }

		public virtual NameValueCollection QueryValues
		{
			get
			{
				if (this.queryValues == null)
				{
					lock (this)
					{
						if (this.queryValues == null)
						{
							var dictionary = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

							foreach (var pair in StringUriUtils.ParseQuery(this.Query))
							{
								dictionary[pair.Key] = pair.Value;
							}

							System.Threading.Thread.MemoryBarrier();

							this.queryValues = dictionary;
						}
					}
				}

				return this.queryValues;
			}
		}
		private NameValueCollection queryValues;

		public virtual string PathAndQuery
		{
			get
			{
				if (this.pathAndQuery == null)
				{
					lock (this)
					{
						if (this.pathAndQuery == null)
						{
							string localPathAndQuery;

							if (this.Query == "")
							{
								localPathAndQuery = this.AbsolutePath;
							}
							else
							{
								localPathAndQuery = this.AbsolutePath + '?' + this.Query;
							}

							System.Threading.Thread.MemoryBarrier();

							this.pathAndQuery = localPathAndQuery;
						}
					}
				}
				return this.pathAndQuery;
			}
		}
		private string pathAndQuery;

		public virtual string ShortName
		{
			get
			{
				return this.NameWithoutQuery;
			}
		}

		public virtual string AbsolutePath
		{
			get
			{
				return this.absolutePath;
			}
		}

		public virtual string Extension
		{
			get
			{
				if (this.extension == null)
				{
					lock (this)
					{
						this.extension = this.ShortName.Right(PredicateUtils.ObjectEquals('.').Not());

						if (this.extension.Length == this.ShortName.Length)
						{
							this.extension = String.Empty;
						}
					}
				}

				return this.extension;
			}
		}

		public virtual int Depth
		{
			get
			{
				if (this.IsRoot)
				{
					return 0;
				}

				if (this.depth == -1)
				{
					this.depth = FunctionUtils.VolatileAssign(() => this.absolutePath.CountChars(c => c == FileSystemManager.SeperatorChar));
				}

				return this.depth;
			}
		}
		private int depth = -1;

		public virtual string Scheme
		{
			get
			{
				return this.scheme;
			}
		}

		public virtual string Uri
		{
			get
			{
				if (this.uri == null)
				{			
					lock (this)
					{
						if (this.uri == null)
						{
							string value;

							value = this.RootUri + this.absolutePath + (this.Query == "" ? "" : ("?" + this.Query));

							value = TextConversion.ToEscapedHexString(value, TextConversion.IsStandardUrlEscapedChar);

							System.Threading.Thread.MemoryBarrier();

							this.uri = value;
						}
					}
				}

				return this.uri;
			}
		}

		public virtual string RootUri
		{
			get
			{
				if (this.rootUri == null)
				{
					lock (this)
					{
						if (this.rootUri == null)
						{
							var value = this.GetRootUri();

							System.Threading.Thread.MemoryBarrier();

							this.rootUri = value;
						}
					}
				}

				return this.rootUri;
			}
		}

		public virtual bool IsRoot
		{
			get
			{
				return this.absolutePath.Length == 1
					&& this.absolutePath[0] == FileSystemManager.SeperatorChar;
			}
		}

		public virtual INodeAddress Parent
		{
			get
			{
				if (this.IsRoot)
				{
					throw new InvalidOperationException(String.Format("Root directory has no parent"));
				}

				lock (this)
				{
					var parentPath = this.AbsolutePath.SplitAroundCharFromRight(PredicateUtils.ObjectEquals('/')).Left;
					
					if (parentPath.Length == 0)
					{
						parentPath = FileSystemManager.SeperatorString;
					}

					return CreateAddress(parentPath, "");
				}
			}
		}

		public virtual INodeAddress ResolveAddress(string name)
		{
			return ResolveAddress(name, AddressScope.FileSystem);
		}

		public virtual INodeAddress ResolveAddress(string name, AddressScope scope)
		{	
			int x;
			var query = "";

			if (name.Length == 0 || (name.Length > 0 && name[0] != FileSystemManager.RootChar))
			{
				// Path is relative.

				if (this.IsRoot)
				{
					name = FileSystemManager.RootChar + name;
				}
				else
				{
					name = this.absolutePath + FileSystemManager.SeperatorChar + name;
				}
			}

			if ((x = name.IndexOf('?')) >= 0)
			{					
				query = name.Substring(x + 1);
				name = name.Substring(0, x);
			}

			name = StringUriUtils.NormalizePath(name);

			if (!CheckPathScope(this.AbsolutePath, name,  scope))
			{
				throw new AddressScopeValidationException(name, scope);
			}

			return CreateAddress(name, query);
		}

		public virtual string GetRelativePathTo(INodeAddress name)
		{
			if (!this.RootUri.Equals(name.RootUri))
			{
				throw new ArgumentOutOfRangeException(name.ToString());
			}

			return GetRelativePathTo(name.AbsolutePath);
		}

		public virtual string GetRelativePathTo(string absolutePath)
		{
			var path = absolutePath;

			var pathLen = path.Length;
			var basePathLen = this.absolutePath.Length;
			
			// When base path is root

			if (basePathLen == 1)
			{
				if (pathLen == 1)
				{
					return ".";
				}
				else
				{
					return path.Substring(1);
				}
			}

			var pos = 0;
			var maxLen = Math.Min(basePathLen, pathLen);

			for (; pos < maxLen && this.absolutePath[pos] == path[pos]; pos++)
			{
				// Just counting.
			}                       

			if (pos == basePathLen && pos == pathLen)
			{
				// Same names.

				//return path;
				return ".";
			}
			else if (pos == basePathLen && pos < pathLen && path[pos] == FileSystemManager.SeperatorChar)
			{
				// path is a descendent.

				return path.Substring(pos + 1);
			}

			var builder = new StringBuilder(path.Length);

			if (pathLen > 1 && (pos < pathLen || this.absolutePath[pos] != FileSystemManager.SeperatorChar))
			{
				// Not a direct ancestor.  Trace backwards.

				pos = this.absolutePath.LastIndexOf(FileSystemManager.SeperatorChar, pos);

				builder.Append(path.Substring(pos));
			}

			// Prepend '../' for each elementin the base path past the common prefix.
			builder.Insert(0, "..");

			pos = this.absolutePath.IndexOf(FileSystemManager.SeperatorChar, pos + 1);

			while (pos != -1)
			{
				builder.Insert(0, "../");
				pos = this.absolutePath.IndexOf(FileSystemManager.SeperatorChar, pos + 1);
			}

			return builder.ToString();            
		}

		public virtual bool IsAncestorOf(INodeAddress name)
		{
			return name.IsDescendentOf(this, AddressScope.Descendent);
		}
		
		public virtual bool IsDescendentOf(INodeAddress name)
		{
			return IsDescendentOf(name, AddressScope.Descendent);
		}

		public virtual bool IsDescendentOf(INodeAddress name, AddressScope scope)
		{
			return CheckPathScope(name.AbsolutePath, this.AbsolutePath, scope);
		}

		public virtual bool IsDescendentOf(INodeAddress name, StringComparison comparisonType, AddressScope scope)
		{
			return CheckPathScope(name.AbsolutePath, this.AbsolutePath, comparisonType, scope);
		}

		private bool CheckPathScope(string basePath, string comparePath, AddressScope scope)
		{
			return CheckPathScope(basePath, comparePath, StringComparison.CurrentCulture, scope);
		}

		private bool CheckPathScope(string basePath, string comparePath, StringComparison comparisonType, AddressScope scope)
		{
			if (scope == AddressScope.FileSystem)
			{
				return true;
			}

			if (!comparePath.StartsWith(basePath, comparisonType))
			{
				return false;
			}

			var baseLength = basePath.Length;

			switch (scope)
			{
				case AddressScope.Child:

					if (/* ComparePath is the same as BasePath */
						comparePath.Length == baseLength
						/* ComparePath has same parent as base path but different short name */
						|| (baseLength > 1 && comparePath[baseLength] != FileSystemManager.SeperatorChar)
						/* ComparePath is a (grand)*child of basePath */
						|| comparePath.IndexOf(FileSystemManager.SeperatorChar, baseLength + 1) >= 0)
					{
						return false;
					}

					break;

				case AddressScope.Descendent:

					if (/* ComparePath is the same as base path */
						comparePath.Length == baseLength
						/* ComparePath has same parent as base path but different short name */
						|| (baseLength > 1 && comparePath[baseLength] != FileSystemManager.SeperatorChar))
					{
						return false;
					}

					break;

				case AddressScope.DescendentOrSelf:

					if (
						/* ComparePath has same parent as base path but different short name */
						baseLength > 1
						&& comparePath.Length > baseLength
						&& comparePath[baseLength] != FileSystemManager.SeperatorChar )
					{
						return false;
					}

					break;

				default:

					return false;
			}

			return true;
		}

		public virtual bool RootsAreEqual(INodeAddress nodeAddress)
		{
			return this.RootUri == nodeAddress.RootUri;
		}

		/// <summary>
		/// Create and return a new filename of the same type and schema as this file name
		/// but but with a different path.
		/// </summary>
		/// <remarks>
		/// The only difference between the current <c>INodeAddress</c> and the returned
		/// <c>INodeAddress</c> is the path.
		/// </remarks>
		/// <param name="absolutePath">The absolute path to the node</param>
		/// <param name="query">The query part of the path (empty string if there is no query part)</param>
		/// <returns></returns>
		protected abstract INodeAddress CreateAddress(string absolutePath, string query);

		/// <summary>
		/// Gets a string representing the root URI of the filesytem this filename belongs to.
		/// </summary>
		/// <returns></returns>
		protected abstract string GetRootUri();
		
		public override string ToString()
		{
			return TextConversion.FromEscapedHexString(this.Uri.ToString());
		}

		public override int GetHashCode()
		{
			return this.RootUri.GetHashCode() ^ this.absolutePath.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}

			var address = obj as AbstractNodeAddress;

			if (address == null)
			{
				return false;
			}

			return this.Uri.Equals(address.Uri);
		}

		public virtual bool ParentsEqual(INodeAddress nodeAddress)
		{
			return ParentsEqual(nodeAddress, StringComparison.CurrentCulture);	
		}

		public virtual bool ParentsEqual(INodeAddress nodeAddress, StringComparison comparisonType)
		{
			if (this.Depth != nodeAddress.Depth)
			{
				return false;
			}

			if (this.IsRoot)
			{
				return true;
			}

			var x = this.AbsolutePath.LastIndexOf('/');
			var y = nodeAddress.AbsolutePath.LastIndexOf('/');

			if (x == -1)
			{
				return false;
			}

			if (x != y)
			{
				return false;
			}

			return String.Equals(this.AbsolutePath.Substring(0, x), nodeAddress.AbsolutePath.Substring(0, y));
		}

		public virtual string PathToDepth(int depth)
		{
			if (depth == 0)
			{
				return "/";
			}

			depth++;

			var builder = new StringBuilder();

			foreach (var c in this.AbsolutePath)
			{
				if (c == '/')
				{
					depth--;

					if (depth == 0)
					{
						break;
					}
				}

				builder.Append(c);
			}

			return builder.ToString();
		}

		public virtual string DisplayUri
		{
			get
			{
				return TextConversion.FromEscapedHexString(this.Uri);
			}
		}
	}
}
