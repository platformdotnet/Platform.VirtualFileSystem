using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.DataFile
{
	public class DataFile<T>
		: FileWrapper
		where T : class
	{
		public virtual T GetValue(DataFileLoadFlags loadFlags)
		{
			if (loadFlags == DataFileLoadFlags.Refresh)
			{
				this.Load();

				return this.Value;
			}
			else if (loadFlags == DataFileLoadFlags.RefreshIfNull)
			{
				if (this.Value == null)
				{
					this.Load();
				}

				return this.Value;
			}

			return this.Value;
		}

		public virtual T Value
		{
			get
			{
				using (GetAutoLock().Lock())
				{
					return this.value;
				}
			}
			set
			{
				using (GetAutoLock().Lock())
				{
					this.value = value;
				}
			}
		}
		private T value;

		public new DataFileNodeType<T> NodeType { get; private set; }

		public virtual IFile File { get; set; }

		public override INode Refresh()
		{
			base.Refresh();

			Load();

			return this;
		}

		public DataFile(IFile file, DataFileNodeType<T> nodeType)
			: this(file, null, nodeType)
		{
		}

		public DataFile(IFile file, T value, DataFileNodeType<T> nodeType)
			: base(file)
		{
			this.File = file;
			this.value = value;
			this.NodeType = nodeType;
			
			if (nodeType.AutoLoad)
			{
				Load();
			}

			if (this.NodeType.AutoLoad)
			{
				this.File.Activity += new NodeActivityEventHandler(File_Activity);
			}
		}

		void File_Activity(object sender, NodeActivityEventArgs eventArgs)
		{
			if (!this.NodeType.AutoLoad)
			{
				return;
			}

			if (eventArgs.Activity == FileSystemActivity.Changed)
			{
				using (GetAutoLock().Lock())
				{
					Load();
				}
			}
			else if (eventArgs.Activity == FileSystemActivity.Created)
			{
				using (GetAutoLock().Lock())
				{
					Load();
				};
			}
			else if (eventArgs.Activity == FileSystemActivity.Deleted)
			{
				using (GetAutoLock().Lock())
				{
					this.value = null;
				}
			}
			else if (eventArgs.Activity == FileSystemActivity.Renamed)
			{
				using (GetAutoLock().Lock())
				{
					this.value = null;
				}
			}
		}
				
		public virtual void Save()
		{
			using (GetAutoLock().Lock())
			{
				ActionUtils.ToRetryAction<object>
				(
					delegate
					{
						this.NodeType.Save(this);
					},
					this.NodeType.RetryTimeout,
					e => e is System.IO.IOException
				)(null);
			}
		}

		public virtual void Load()
		{
			if (!this.Attributes.Exists)
			{
				this.value = null;
			}
			else
			{
				ActionUtils.ToRetryAction<object>
				(
					delegate
					{
						using (GetAutoLock().Lock())
						{
							try
							{
								this.value = this.NodeType.Load(this);
							}
							catch (NodeNotFoundException)
							{
								this.Attributes.Refresh();

								if (!this.Attributes.Exists)
								{
									this.value = null;

									return;
								}
							}
						}
					},
					this.NodeType.RetryTimeout,
					e => e is System.IO.IOException
				)(null);
			}
		}
	}
}
