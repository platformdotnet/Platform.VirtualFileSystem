using System;
using System.IO;
using System.Net;
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
		{
			WebResponse response;

			if (!(contentName == null
				|| contentName == DefaultContentName))
			{
				throw new NotSupportedException();
			}
			
			encoding = null;

			try
			{
				response = WebRequest.Create(this.Address.Uri).GetResponse();
			}
			catch (WebException e)
			{
				this.exists = false;
				
				throw new FileNotFoundException(this.Address.Uri, e);
			}

			this.exists = true;
			
			try
			{
				var contentType = response.Headers["content-type"];

				if (contentType != null)
				{
					var match = charsetRegex.Match(contentType);

					if (match.Success)
					{
						encoding = match.Groups[1].Value;
					}
				}

				this.contentLength = response.ContentLength;
				
				try
				{
					this.creationDate = DateTime.Parse(response.Headers["Date"]);
				}
				catch (FormatException)
				{
					this.creationDate = null;
				}

				return response.GetResponseStream();
			}
			catch (Exception e)
			{
				throw new IOException("Unexpected error", e);	
			}
		}

		protected override Stream DoGetOutputStream(string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			if (contentName != null)
			{
				throw new NotSupportedException();
			}

			if (mode == FileMode.Append)
			{
				throw new NotSupportedException("FileMode: " + mode.ToString());
			}

			return WebRequest.Create(this.Address.Uri).GetRequestStream();
		}

		public class WebFileAttributes
			: AbstractTypeBasedFileAttributes
		{
			private WebFile webFile;

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
