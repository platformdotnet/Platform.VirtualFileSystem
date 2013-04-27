using System.IO;

namespace Platform.VirtualFileSystem
{
	public class StreamHashingServiceType
		: HashingServiceType
	{
		public virtual Stream Stream { get; set; }

		public StreamHashingServiceType(Stream stream)
			: this(stream, "md5")
		{
			this.Stream = stream;
		}

		public StreamHashingServiceType(Stream stream, string algorithmName)
			: base(typeof(IStreamHashingService), algorithmName)
		{
			this.Stream = stream;
		}
	}
}