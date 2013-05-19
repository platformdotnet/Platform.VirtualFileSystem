using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using Platform.IO;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers.Shadow
{
	/// <summary>
	/// Summary description for ShadowNodeContent.
	/// </summary>
	internal class ShadowNodeContent
		: AbstractNodeContent
	{
		private IFile file;
		private readonly ShadowFile shadowFile;
		private readonly IFileSystem tempFileSystem;

		internal long targetLength = -1;
		internal byte[] targetMd5;
		internal bool hasCreationTime;
		internal DateTime creationTime;
		internal bool hasLastWriteTime;
		internal DateTime lastWriteTime;
		
		public ShadowNodeContent(ShadowFile shadowFile, IFile file, IFileSystem tempFileSystem)
		{
			var buffer = new StringBuilder(file.Address.Uri.Length * 3);

			this.shadowFile = shadowFile;
			this.tempFileSystem = tempFileSystem;
			this.file = tempFileSystem.ResolveFile(GenerateName());

			buffer.Append(ComputeHash(file.Address.Uri).TrimRight('='));

			var value = (string)file.Address.QueryValues["length"];

			if (value != null)
			{
				this.targetLength = Convert.ToInt64(value);
				
				buffer.Append('_').Append(value);
			}

			value = (string)file.Address.QueryValues["md5sum"];

			if (value != null)
			{
				this.targetMd5 = Convert.FromBase64String(value);

				buffer.Append('_').Append(value);
			}

			value = (string)file.Address.QueryValues["creationtime"];

			if (value != null)
			{
				this.hasCreationTime = true;
				this.creationTime = Convert.ToDateTime(value);

				buffer.Append('_').Append(this.creationTime.Ticks.ToString());
			}

			value = (string)file.Address.QueryValues["writetime"];

			if (value != null)
			{
				this.hasLastWriteTime = true;
				this.lastWriteTime = Convert.ToDateTime(value);

				buffer.Append('_').Append(this.lastWriteTime.Ticks.ToString());
			}

			value = buffer.ToString();
		}

		private static string ComputeHash(string s)
		{
			var encoding = Encoding.ASCII;
			var algorithm = new MD5CryptoServiceProvider();
			var output = (algorithm.ComputeHash(encoding.GetBytes(s)));

			return TextConversion.ToBase32String(output);
		}

		protected virtual string GenerateName()
		{
			return "VFSSHADOWS.TMP/" + Guid.NewGuid().ToString("N") + "." + this.shadowFile.Address.Extension;
		}

		public override Stream GetInputStream()
		{
			string encoding;

			return GetInputStream(out encoding);
		}
		
		private class ShadowOutputStream
			: StreamWrapper
		{
			private readonly IFile file;
			private readonly IFile shadowFile;

			public ShadowOutputStream(Stream stream, IFile shadowedFile, IFile file)
				: base(stream)
			{
				this.file = file;
				this.shadowFile = shadowedFile;
			}

			public override void Close()
			{
				base.Close();

				var dest = this.shadowFile.ParentDirectory.ResolveFile(this.file.Address.Name);

				this.file.MoveTo(dest.ParentDirectory, true);
				dest.RenameTo(this.shadowFile.Address.Name, true);
			}
		}

		private class ShadowInputStream
			: StreamWrapper
		{
			private Timer timer;
			private readonly IFile file;
			
			public ShadowInputStream(Stream stream, IFile file)
				: base(stream)
			{
				this.file = file;

				SetupTimer();
			}

			~ShadowInputStream()
			{
				Close();
			}

			private void SetupTimer()
			{
				TimeSpan period;

				period = TimeSpan.FromMinutes(30);

				if (this.timer != null)
				{
					this.timer.Dispose();
				}

				this.timer = new Timer(period.TotalMilliseconds);

				this.timer.AutoReset = true;
				this.timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
				this.timer.Start();
			}

			protected virtual void TimerElapsed(object sender, ElapsedEventArgs e)
			{
				lock (this)
				{
					try
					{
						this.file.Delete();	

						this.timer.Stop();
						this.timer.Close();
					}
					catch (Exception)
					{
					}
				}
			}

			private bool closed = false;

			public override void Close()
			{
				lock (this)
				{
					if (this.closed)
					{
						return;
					}

					this.closed = true;
				}

				base.Close ();

				this.file.Refresh();

				try
				{
					if (this.file.Exists)
					{
						this.file.Delete();
					}
				}
				catch (Exception)
				{
					SetupTimer();
				}
			}
		}

		private class StreamCloser
		{
			private readonly Stream stream;
			private readonly Timer timer;

			public StreamCloser(Stream stream, Timer timer)
			{
				this.stream = stream;
				this.timer = timer;
			}

			public void TimerElapsed(object sender, ElapsedEventArgs e)
			{
				this.stream.Close();
				this.timer.Close();
			}
		}

		internal IFile GetReadFile(TimeSpan timeout)
		{
			lock (this)
			{
				var timer = new Timer(timeout.TotalMilliseconds);

				timer.Elapsed += new ElapsedEventHandler(new StreamCloser(GetInputStream(), timer).TimerElapsed);
				timer.AutoReset = false;
				timer.Start();

				return this.file;
			}
		}

		public override void Delete()
		{
			this.shadowFile.GetContent().Delete();
		}

		protected override Stream DoGetInputStream(out string encoding, FileMode mode, FileShare sharing)
		{
			Stream retval;

			lock (this)
			{
				long? length = 0;
				bool error = false;
				DateTime? creationTime = null;
				DateTime? lastWriteTime = null;

				this.file.Attributes.Refresh();
				this.shadowFile.Attributes.Refresh();

				try
				{
					if (this.file.Exists)
					{
						length = this.file.Length ?? 0;
						creationTime = this.file.Attributes.CreationTime;
						lastWriteTime = this.file.Attributes.LastWriteTime;
					}
					else
					{
						error = true;
					}
				}
				catch (IOException)
				{
					error = true;
				}

				if (!error
					&& length == this.shadowFile.Length
					&& creationTime == this.shadowFile.Attributes.CreationTime
					&& lastWriteTime == this.shadowFile.Attributes.LastWriteTime
					&& (mode == FileMode.Open || mode == FileMode.OpenOrCreate))
				{
					try
					{
						retval = new ShadowInputStream(this.file.GetContent().GetInputStream(out encoding, sharing), this.file);

						return retval;
					}
					catch (IOException)
					{	
					}
				}

				this.file = this.tempFileSystem.ResolveFile(GenerateName());

				switch (mode)
				{
					case FileMode.Create:
						this.file.Create();
						break;
					case FileMode.CreateNew:
						if (this.shadowFile.Exists)
						{
							throw new IOException("File exists");
						}
						this.file.Create();
						break;
					case FileMode.OpenOrCreate:
						try
						{
							this.shadowFile.CopyTo(this.file, true);
						}
						catch (FileNotFoundException)
						{
							this.file.Create();
						}
						break;
					case FileMode.Open:
					default:
						this.file.ParentDirectory.Create(true);
						this.shadowFile.CopyTo(this.file, true);
						break;
				}
				
				this.file.Attributes.CreationTime = this.shadowFile.Attributes.CreationTime;
				this.file.Attributes.LastWriteTime = this.shadowFile.Attributes.LastWriteTime;

				retval = new ShadowInputStream(this.file.GetContent().GetInputStream(out encoding, sharing), this.file);
				
				return retval;
			}
		}

		protected override Stream DoGetOutputStream(string encoding, FileMode mode, FileShare sharing)
		{
			IFile file;

			file = this.tempFileSystem.ResolveFile(GenerateName());

			switch (mode)
			{
				case FileMode.Truncate:
					file.Create();
					break;
			}

			return new ShadowOutputStream(file.GetContent().GetOutputStream(encoding, sharing), this.shadowFile.ShadowedFile, file);
		}
	}
}
