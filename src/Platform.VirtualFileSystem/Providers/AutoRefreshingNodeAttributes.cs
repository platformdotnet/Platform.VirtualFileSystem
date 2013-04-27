using System;

namespace Platform.VirtualFileSystem.Providers
{
	public class AutoRefreshingFileAttributes
		: AutoRefreshingNodeAttributes, IFileAttributes
	{
		public AutoRefreshingFileAttributes(INodeAttributes details, int autoRefreshCount)
			: base(details, autoRefreshCount)
		{
		}

		public new IFileAttributes Wrappee
		{
			get
			{
				return (IFileAttributes)base.Wrappee;
			}
		}

		IFileAttributes IFileAttributes.Refresh()
		{
			return (IFileAttributes)this.Refresh();
		}

		public virtual long? Length
		{
			get
			{
				if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
				{
					if (this.autoRefreshCount != -1)
					{
						this.autoRefreshCount--;
					}

					this.Wrappee.Refresh();
				}

				return this.Wrappee.Length;
			}
		}
	}
		
	public class AutoRefreshingNodeAttributes
		: NodeAttributesWrapper
	{
		protected int autoRefreshCount;
		protected int orignalAutoRefreshCount;

		/// <summary>
		/// Construct a new <c>AutoRefreshingNodeAttributes</c> class that automatically
		/// refreshes the details on every query for a set number of queries.
		/// </summary>
		/// <remarks>
		/// When <c>localAutoRefreshCount</c> is specified
		/// </remarks>
		/// <param name="details">The underlying details implementation to use.</param>
		/// <param name="autoRefreshCount">
		/// The minimum number of future queries in which to auto refresh.  -1 means infininty.
		/// </param>
		public AutoRefreshingNodeAttributes(INodeAttributes details, int autoRefreshCount)
			: base(details)
		{
			if (autoRefreshCount < -1)
			{
				throw new ArgumentOutOfRangeException("Can't be less than -1", "localAutoRefreshCount");
			}

			this.orignalAutoRefreshCount = this.autoRefreshCount = autoRefreshCount;
		}

		#region INodeAttributes Members
		
		/// <summary>
		/// Ensures that the current object will auto refresh for at least the next
		/// <c>localAutoRefreshCount</c> queries.
		/// </summary>
		/// <remarks>
		/// If the current object doesn't have an <c>localAutoRefreshCount</c> or if the
		/// current object already has been set with a higher <c>localAutoRefreshCount</c>
		/// then this method will have no effect.
		/// </remarks>
		/// <param name="localAutoRefreshCount">The new <c>localAutoRefreshCount</c>.</param>
		public virtual void EnsureAutoRefreshCount(int localAutoRefreshCount)
		{
			lock (this)
			{
				if (this.autoRefreshCount >= localAutoRefreshCount)
				{
					return;
				}

				this.autoRefreshCount = localAutoRefreshCount;
			}
		}

		public override object this[string name]
		{
			get
			{
				if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
				{
					if (this.autoRefreshCount != -1)
					{
						this.autoRefreshCount--;
					}

					this.Wrappee.Refresh();
				}

				return base[name];
			}
			set
			{
				base[name] = value;
			}
		}

		public override DateTime? CreationTime
		{
			get
			{
				lock (this)
				{
					if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
					{
						if (this.autoRefreshCount != -1)
						{
							this.autoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.CreationTime;
				}
			}
			set
			{
				this.Wrappee.CreationTime = value;
			}
		}

		public override DateTime? LastAccessTime
		{
			get
			{
				lock (this)
				{
					if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
					{
						if (this.autoRefreshCount != -1)
						{
							this.autoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.LastAccessTime;
				}
			}
			set
			{
				this.Wrappee.LastAccessTime = value;
			}
		}

		public override DateTime? LastWriteTime
		{
			get
			{
				lock (this)
				{
					if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
					{
						if (this.autoRefreshCount != -1)
						{
							this.autoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.LastWriteTime;
				}
			}
			set
			{
				this.Wrappee.LastWriteTime = value;
			}
		}

		public override bool Exists
		{
			get
			{
				lock (this)
				{
					if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
					{
						if (this.autoRefreshCount != -1)
						{
							this.autoRefreshCount--;
						}

						this.Wrappee.Refresh();
					}

					return this.Wrappee.Exists;
				}
			}
		}

		public override System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>> GetEnumerator()
		{
			if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
			{
				this.Wrappee.Refresh();
			}

			return base.GetEnumerator();
		}

		public override System.Collections.Generic.IEnumerable<string> Names
		{
			get
			{
				if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
				{
					this.Wrappee.Refresh();
				}

				return base.Names;
			}
		}

		public override System.Collections.Generic.IEnumerable<object> Values
		{
			get
			{
				if (this.autoRefreshCount == -1 || this.autoRefreshCount > 0)
				{
					this.Wrappee.Refresh();
				}

				return base.Values;
			}
		}

		public override INodeAttributes Refresh()
		{
			lock (this)
			{
				if (this.autoRefreshCount == 0)
				{
					this.autoRefreshCount = this.orignalAutoRefreshCount;
				}
			}

			return this;
		}

		#endregion
	}
}
