using System;

namespace Platform.VirtualFileSystem
{
	[Serializable]
	public class MalformedUriException
		: FormatException
	{
		public virtual string Uri { get; private set; }

		public MalformedUriException()
			: base()
		{	
			this.Uri = "";
		}

		public MalformedUriException(string uri)
			: base()
		{
			this.Uri = uri;
		}

		public MalformedUriException(string uri, string message)
			: base(message)
		{
			this.Uri = uri;
		}

		public MalformedUriException(string uri, string message, Exception innerException)
			: base(message, innerException)
		{
			this.Uri = uri;
		}
	}
}