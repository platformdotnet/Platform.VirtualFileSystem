namespace Platform.VirtualFileSystem
{
	public class FileTransferServiceType
		: ServiceType
	{
		/// <summary>
		/// HashAlgorithmName
		/// </summary>
		public string HashAlgorithmName { get; set; }

		/// <summary>
		///  AttributesToTransfer
		/// </summary>
		public virtual string[] AttributesToTransfer { get; set; }

		/// <summary>
		///  BufferSize
		/// </summary>
		public virtual int BufferSize { get; set; }

		/// <summary>
		///  VerifyIntegrity
		/// </summary>
		public virtual bool VerifyIntegrity { get; set; }

		/// <summary>
		///  Destination
		/// </summary>
		public virtual IFile Destination { get; set; }

		/// <summary>
		/// The size of chunks (bytes) that are transferred at once.
		/// </summary>
		public virtual int ChunkSize { get; set; }

		public FileTransferServiceType(IFile destination)
			: this(destination, false)
		{
		}

		public FileTransferServiceType(IFile destination, bool verifyIntegrity)
			: this(destination, verifyIntegrity, 128 * 1024, 128)
		{
		}

		public FileTransferServiceType(IFile destination, bool verifyIntegrity, int bufferSize, int chunkSize)
		{
			this.Destination = destination;
			this.VerifyIntegrity = verifyIntegrity;
			this.BufferSize = bufferSize;			
			this.ChunkSize = chunkSize;
			this.HashAlgorithmName = "md5";
			this.BufferSize = 4096;

			this.AttributesToTransfer = new string[] { "CreationTime", "LastWriteTime", "LastAccessTime" };
		}
	}
}