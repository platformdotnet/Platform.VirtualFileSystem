using System;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.DataFile
{
	public class XmlObjectDataFileLoaderSaver<T>
		: IDataFileLoaderSaver<T>
		where T : class, new()
	{
		public virtual XmlSerializer<T> Deserializer { get; set; }
		public virtual XmlSerializer<T> Serializer { get; set; }

		public XmlObjectDataFileLoaderSaver()
			: this(XmlSerializer<T>.New())
		{
		}

		public XmlObjectDataFileLoaderSaver(XmlSerializer<T> serializer)
			: this(serializer, serializer)
		{
		}

		public XmlObjectDataFileLoaderSaver(XmlSerializer<T> serializer, XmlSerializer<T> deserializer)
		{
			this.Serializer = serializer;
			this.Deserializer = deserializer;
		}

		public virtual T Load(DataFile<T> dataFile)
		{
			const int retrycount = 10;

			for (var i = 0; i < retrycount; i++)
			{
				try
				{
					using (var reader = dataFile.File.GetContent().GetReader())
					{
						var retval = this.Deserializer.Deserialize(reader);

						if (retval is IObjectWithGeneratedDefaults)
						{
							if (((IObjectWithGeneratedDefaults)retval).RequiresGeneratedDefaults)
							{
								((IObjectWithGeneratedDefaults)retval).GenerateDefaults();

								Save(dataFile, retval);
							}
						}

						return retval;
					}
				}
				catch (Exception)
				{
					if (i == retrycount - 1)
					{
						throw;
					}
				}

				System.Threading.Thread.Sleep(500);
			}

			throw new InvalidOperationException();
		}

		protected virtual void Save(DataFile<T> dataFile, T value)
		{
			using (var writer = dataFile.File.GetContent().GetWriter())
			{
				this.Serializer.Serialize(value, writer);
			}
		}

		public virtual void Save(DataFile<T> dataFile)
		{
			if (!dataFile.ParentDirectory.Exists)
			{
				dataFile.ParentDirectory.Create(true);
			}

			using (var writer = dataFile.File.GetContent().GetWriter())
			{
				this.Serializer.Serialize(dataFile.Value, writer);
			}
		}
	}
}
