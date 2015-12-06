using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebFile
		: AbstractFile
	{
		private static readonly Regex charsetRegex;

		static WebFile()
		{
			charsetRegex = new Regex(@".*charset[=]([^ ;]*)", RegexOptions.Compiled);
		}

		private bool exists;
		private long contentLength = -1;
		private DateTime? creationDate;

		protected override Stream DoGetInputStream(string contentName, out string encoding, FileMode mode, FileShare sharing)
		{	return WebFileSystem.DoGetInputStream(this, contentName, out encoding, mode, sharing, out creationDate, out exists, out contentLength);
			
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			return WebFileSystem.DoGetOutputStream(this, contentName, encoding, mode, sharing);
		}

		public class WebFileAttributes
			: AbstractTypeBasedFileAttributes
		{
			private readonly WebFile webFile;

			public WebFileAttributes(WebFile webFile)
				: base(webFile)
			{
				this.webFile = webFile;
			}

			public override long? Length
			{
				get
				{
					return this.webFile.contentLength;
				}
			}

			public override DateTime? CreationTime
			{
				get
				{
					return this.webFile.creationDate;
				}
				set
				{					
				}
			}

			public override DateTime? LastWriteTime
			{
				get
				{
					return this.webFile.creationDate;
				}
				set
				{
				}
			}

			public override bool Exists
			{
				get
				{
					return this.webFile.exists;
				}
			}

			public override INodeAttributes Refresh()
			{
				lock (this.webFile)
				{
					try
					{
						this.webFile.GetContent().GetInputStream();

						this.webFile.exists = true;
					}
					catch (FileNotFoundException)
					{
						this.webFile.exists = false;
					}
				}

				return this;
			}
		}

		protected override INodeAttributes CreateAttributes()
		{
			return new AutoRefreshingNodeAttributes(new WebFileAttributes(this), 1);
		}

		public override bool SupportsActivityEvents
		{
			get
			{
				return false;
			}
		}

		public override INode Delete()
		{
			throw new NotSupportedException("WebFile.Delete");
		}
		
		public WebFile(WebFileSystem fileSystem, INodeAddress name)
			: base(name, fileSystem)
		{			
		}
	}
}
