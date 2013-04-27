using System;
using System.IO;
using Platform.IO;

namespace Platform.VirtualFileSystem.Providers.Imaginary
{
	public class ImaginaryMemoryFile
		: ImaginaryFile, IValued<byte[]>
	{
		public new DictionaryBasedNodeAttributes Attributes
		{
			get
			{
				return (DictionaryBasedNodeAttributes)base.Attributes;
			}
		}

		public override bool SupportsActivityEvents
		{
			get
			{
				return true;
			}
		}

		private readonly Predicate<ImaginaryMemoryFile> existsProvider;
		private readonly Func<byte[]> valueProvider;

		public ImaginaryMemoryFile(IFileSystem fileSystem, INodeAddress address)
			: this(fileSystem, address, null, null)
		{
		}

		public ImaginaryMemoryFile(IFileSystem fileSystem, INodeAddress address, Func<byte[]> valueProvider, Predicate<ImaginaryMemoryFile> existsProvider)
			: base(fileSystem, address)
		{
			this.valueProvider = valueProvider;
			this.existsProvider = existsProvider;

			if (this.valueProvider != null)
			{
				Create();
			}

			this.Attributes.AttributeValueGetFilter = delegate(string name, object value)
			{
				if (this.existsProvider == null)
				{
					return value;
				}

				if (name.Equals("exists", StringComparison.CurrentCultureIgnoreCase))
				{
					return this.existsProvider(this);
				}

				return value;
			};
		}

		public override INode Delete()
		{
			if (!this.Exists)
			{
				throw new FileNodeNotFoundException(this.Address);
			}

			this.Attributes.SetValue<bool>("exists", false);

			OnActivity(new NodeActivityEventArgs(FileSystemActivity.Deleted, this));

			this.value = null;

			return this;
		}

		public override INode Create()
		{
			if (!this.Exists)
			{
				this.Value = new byte[0];

				this.Attributes.SetValue<bool>("exists", true);

				OnActivity(new NodeActivityEventArgs(FileSystemActivity.Created, this));
				OnCreated(new NodeActivityEventArgs(FileSystemActivity.Created, this));
			}

			return this;
		}
		
		protected override INodeAttributes CreateAttributes()
		{
			return new DictionaryBasedNodeAttributes();
		}

		#region Value

		public virtual byte[] RawNonDynamicValue
		{
			get
			{
				return this.value;
			}
		}

		public virtual byte[] Value
		{
			get
			{
				lock (this)
				{
					if (this.valueProvider != null)
					{
						this.value = this.valueProvider();
					}

					return this.value;
				}
			}
			set
			{
				lock (this)
				{
					this.value = value;

					if (!this.Exists)
					{
						this.Attributes.SetValue<bool>("exists", true);

						OnActivity(new NodeActivityEventArgs(FileSystemActivity.Created, this));
						OnCreated(new NodeActivityEventArgs(FileSystemActivity.Created, this));
					}

					OnActivity(new NodeActivityEventArgs(FileSystemActivity.Changed, this));
					OnChanged(new NodeActivityEventArgs(FileSystemActivity.Changed, this));
				}
			}
		}
		private byte[] value;

		object IValued.Value
		{
			get
			{
				return this.value;
			}
		}

		#endregion

		#region StreamUtils

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode fileMode, FileShare fileShare)
		{
			encoding = null;

			if (!this.Exists)
			{
				throw new FileNodeNotFoundException(this.Address);
			}

			return new MemoryStream(this.Value);
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode fileMode, FileShare fileShare)
		{
			MeteringStream stream;
			MemoryStream memoryStream;

			encoding = null;

			if (!this.Exists && (fileMode != FileMode.CreateNew || fileMode != FileMode.OpenOrCreate))
			{
				throw new FileNodeNotFoundException(this.Address);
			}

			stream = new MeteringStream(memoryStream = new MemoryStream());

			stream.BeforeClose += delegate
			{
				lock (this)
				{
					byte[] newValue;

					newValue = new byte[(int)memoryStream.Length];

					Array.Copy(memoryStream.GetBuffer(), newValue, (int)memoryStream.Length);

					this.Value = newValue;
				}
			};

			return stream;
		}

		#endregion
	}
}
