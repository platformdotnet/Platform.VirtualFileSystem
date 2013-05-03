using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Platform.VirtualFileSystem.Providers.Web
{
	public class WebFileSystem
		: AbstractFileSystem
	{
		internal protected WebFileSystem(INodeAddress rootAddress, FileSystemOptions options)
			: base(rootAddress, null, options)
		{			
		}

		protected override INode CreateNode(INodeAddress name, NodeType nodeType)
		{
			if (nodeType == NodeType.Directory)
			{
				return new WebDirectory(this, name);
			}
			else if (nodeType == NodeType.File || nodeType == NodeType.Any)
			{
				return new WebFile(this, name);
			}
			else
			{
				throw new NodeTypeNotSupportedException(nodeType);
			}
		}

		#region NodeHelperMethods

		private static readonly Regex charsetRegex;

		static WebFileSystem()
		{
			charsetRegex = new Regex(@".*charset[=]([^ ;]*)", RegexOptions.Compiled);
		}

		internal static Stream DoGetInputStream(INode node, string contentName, out string encoding, FileMode mode, FileShare sharing, out DateTime? creationDate, out bool exists, out long contentLength)
		{
			WebResponse response;

			if (!(contentName == null || contentName == node.DefaultContentName))
			{
				throw new NotSupportedException();
			}
			
			encoding = null;

			try
			{
				response = WebRequest.Create(node.Address.Uri).GetResponse();
			}
			catch (WebException e)
			{
				exists = false;
				
				throw new FileNotFoundException(node.Address.Uri, e);
			}

			exists = true;
			
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

				contentLength = response.ContentLength;
				
				try
				{
					creationDate = DateTime.Parse(response.Headers["Date"]);
				}
				catch (FormatException)
				{
					creationDate = null;
				}

				return response.GetResponseStream();
			}
			catch (Exception e)
			{
				throw new IOException("Unexpected error", e);	
			}
		}

		internal static Stream DoGetOutputStream(INode node, string contentName, string encoding, FileMode mode, FileShare sharing)
		{
			if (contentName != null)
			{
				throw new NotSupportedException();
			}

			if (mode == FileMode.Append)
			{
				throw new NotSupportedException("FileMode: " + mode.ToString());
			}

			return WebRequest.Create(node.Address.Uri).GetRequestStream();
		}

		#endregion
	}
}