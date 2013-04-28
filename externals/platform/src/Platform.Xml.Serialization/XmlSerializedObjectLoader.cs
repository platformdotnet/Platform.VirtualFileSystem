using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Platform.Xml.Serialization
{
	/// <summary>
	/// Maintains an up to date instance of an object that has been serialized
	/// to a file.
	/// </summary>
	/// <remarks>
	/// The current up to date instance of the object can be retrieved using
	/// the CurrentObject property.
	/// 
	/// If the file has changed, it will be deserialized into a new object
	/// causing the CurrentObject to change.
	/// 
	/// If the file has been deleted, the CurrentObject property will be
	/// changed to null.
	/// 
	/// </remarks>
 	public class XmlSerializedObjectLoader<T>
		where T : new()
 	{		
 		private readonly FileInfo fileInfo;
 		private readonly XmlSerializer<T> serializer;
 		private T currentObject;
 		private readonly T defaultObject;
 		public event EventHandler ObjectUpdated;
 		private readonly Timer loadTimer;
 		private bool tryingToLoad;
		private FileSystemWatcher fileSystemWatcher;
		
		public static readonly Encoding DefaultEncoding = new UTF8Encoding(false);

		public Encoding Encoding
		{
			get
			{
				return encoding;
			}

			set
			{
				if (value == null)
				{
					value = DefaultEncoding;
				}

				encoding = value;
			}
		}
		private Encoding encoding;
 		 								
 		/// <summary>
 		/// Creates a SerializableObjectUpdater.
 		/// </summary>
 		/// <param name="path">
 		/// The path to the file that this object should monitor
 		/// </param>
 		/// <param name="serializer">
 		/// The serializer that should be used to deserialize the file.
 		/// </param>
 		public XmlSerializedObjectLoader(string path, T defaultObject, XmlSerializer<T> serializer)
 		{
			encoding = DefaultEncoding;

 			this.defaultObject = defaultObject;
 			currentObject = defaultObject;
 			fileInfo = new FileInfo(path);
 			this.serializer = serializer;
 			tryingToLoad = false;
			LockObjectBeforeSerializing = false;

			fileSystemWatcher = new FileSystemWatcher(fileInfo.Directory.FullName);

			fileSystemWatcher.Changed += FileSystemEvent;
 			fileSystemWatcher.Deleted += FileSystemEvent;
 
 			loadTimer = new Timer(new TimerCallback(LoadCallback), null, Timeout.Infinite, Timeout.Infinite);
 		}

		public bool LockObjectBeforeSerializing
		{
			get;
			set;
		}

		/// <summary>
		/// Tells the object loader to start auto-loading the object.
		/// </summary>
 		public void StartMonitoring()
 		{
			// Don't do anything if we've already started.
			if (fileSystemWatcher.EnableRaisingEvents)
			{
				return;
			}

			// If the file exists...

			try
			{
				if (fileInfo.Exists)
				{
					// Load it.

					Load();
				}
				else
				{
					// Save the file (the default), then load to get a new object.

					Save();
					Load();
				}
			}
			catch (XmlSerializerException e)
			{
				Console.Error.WriteLine(e.Message);
			}
 						
 			fileSystemWatcher.EnableRaisingEvents = true;
 		}
 
 		private void LoadCallback(object o)
 		{
			try
			{
				Load();
			}
			catch (XmlSerializerException e)
			{
				Console.Error.WriteLine(e.Message);
			}
 		}
 
 		/// <summary>
 		/// Event handler for the FileSystemWatcher.
 		/// </summary>
 		/// <param name="sender"></param>
 		/// <param name="e"></param>
 		private void FileSystemEvent(object sender, FileSystemEventArgs e)
 		{
 			lock (this)
 			{
 				// BUGBUG: .NET has a bug where the name part of e.AbsolutePath 
 				// if lowercase.  Wrapping in a FileInfo object doesn't help.
 
 				if (e.FullPath.ToLower() != fileInfo.FullName.ToLower())
 				{					
 					return;
 				}				
 
 				if (tryingToLoad)
 				{
 					return;
 				}
 
				try
				{
					switch (e.ChangeType)
					{
						case WatcherChangeTypes.Deleted:
	 
							currentObject = defaultObject;
	 						
							try
							{
								//Save();
							}
							catch (XmlSerializerException)
							{
							}
	 
							break;
	 
						case WatcherChangeTypes.Created:
	 
							goto case WatcherChangeTypes.Changed;
	 
						case WatcherChangeTypes.Changed:
	 						
							Load();
							
							break;
					}
				}
				catch(Exception)
				{
				}
 			}
 		}
 
 		/// <summary>
 		/// Forces the CurrentObject to be reloaded.
 		/// </summary>
 		/// <remarks>
 		/// The CurrentObject is reloaded regardless of whether the related
 		/// file has changed or not.
 		/// </remarks> 		
 		public void Load()
 		{
 			lock(this)
 			{
 				if (fileInfo.Exists)
 				{
 					TextReader reader;
 
 					/*
 					 * Try opening the file.
 					 */
 
 					try
 					{
 						reader = 
							new StreamReader
							(
 								new FileStream(fileInfo.FullName, 
								FileMode.Open, 
								FileAccess.Read,
								FileShare.None)
							, true);
 					}
 					catch (IOException)
 					{
 						// Try again in 100 ms
 
 						tryingToLoad = true;
 
 						loadTimer.Change(100, Timeout.Infinite);
 
 						return;
 					}
 
 					/*
 					 * File successfully opended.
 					 */
 
 					tryingToLoad = false;
 		
 					loadTimer.Change(Timeout.Infinite, Timeout.Infinite);
 					
					try
					{
						try
						{
							NewObjectLoaded(serializer.Deserialize(reader));
						}
						catch (Exception e)
						{							
							Save();

							throw e;
						}
					}
					finally
					{
						reader.Close();
					}
 				}
 			}
 		}
 
 		private void FireObjectUpdated()
 		{
 			if (ObjectUpdated != null)
 			{
 				ObjectUpdated(this, new EventArgs());
 			}
 		}
 		
 		/// <summary>
 		/// Save the current object.
 		/// </summary> 		
 		public void Save()
 		{
 			lock (this)
 			{
 				if (currentObject != null)
 				{
 					TextWriter writer;
 					
 					writer = 
 						new StreamWriter
 						(
 							new FileStream
							(
								fileInfo.FullName, 
								FileMode.Create, 
								FileAccess.Write,
								FileShare.None
							),
 							encoding
 						);

 					using (writer)
 					{
						
						if (LockObjectBeforeSerializing)
						{
							Monitor.Enter(currentObject);
						}

						try
						{
							serializer.Serialize(currentObject, writer);
						}
						catch (XmlSerializerException e)
						{
							Console.WriteLine(e.StackTrace);
						}
						
						if (LockObjectBeforeSerializing)
						{
							Monitor.Exit(currentObject);
						}
 					}
 				}
 			}
 		}
 
 		/// <summary>
 		/// Gets the object reflecting the most up to date deserialized
 		/// state of the file monitored by this object monitor.
 		/// </summary>
 		public object CurrentObject
 		{
 			get
 			{
 				return currentObject;
 			}
 		}

		/// <summary>
		/// Called when a new object is loaded.
		/// </summary>
		/// <param name="val"></param>
 		private void NewObjectLoaded(T val)
 		{
 			currentObject = val;

 			FireObjectUpdated();
 		}
 	}
}
