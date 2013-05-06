using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.VirtualFileSystem.DataFile
{
	public class DataFileNodeType<T>
		: NodeType
		where T : class
	{
		public virtual Type DataType
		{
			get { return typeof(T); }
		}

		public virtual bool AutoLoad { get; private set; }
		public virtual TimeSpan RetryTimeout { get; set; }
		public virtual Func<DataFile<T>, T> Load { get; private set; }
		public virtual Action<DataFile<T>> Save { get; private set; }

		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver)
			: this(loaderSaver, false, null as NodeType)
		{
		}

		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver, NodeType innerNodeType)
			: this(loaderSaver, false, innerNodeType)
		{
		}

		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver, bool autoLoad)
			: this(loaderSaver, autoLoad, null as NodeType)
		{
		}

		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver, bool autoLoad, NodeType innerNodeType)
			: this(loaderSaver, autoLoad, TimeSpan.FromSeconds(10), innerNodeType)
		{
		}

		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver, bool autoLoad, TimeSpan retryTimeOut)
			: this(loaderSaver.Load, loaderSaver.Save, autoLoad, retryTimeOut, null as NodeType)
		{
		}
		
		public DataFileNodeType(IDataFileLoaderSaver<T> loaderSaver, bool autoLoad, TimeSpan retryTimeOut, NodeType innerNodeType)
			: this(loaderSaver.Load, loaderSaver.Save, autoLoad, retryTimeOut, innerNodeType)
		{
		}

		public DataFileNodeType(Func<DataFile<T>, T> loader, Action<DataFile<T>> saver, bool autoLoad, TimeSpan retryTimeOut)
			: this(loader, saver, autoLoad, retryTimeOut, null as NodeType)
		{
		}

		public DataFileNodeType(Func<DataFile<T>, T> loader, Action<DataFile<T>> saver, bool autoLoad, TimeSpan retryTimeOut, NodeType innerNodeType)
			: base(innerNodeType)
		{			
			this.Save = saver;
			this.Load = loader;
			this.AutoLoad = autoLoad;
			this.RetryTimeout = retryTimeOut;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj == this)
			{
				return true;
			}

			return false;
		}
	}
}
