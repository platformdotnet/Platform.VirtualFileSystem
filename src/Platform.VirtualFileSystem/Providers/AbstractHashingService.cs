namespace Platform.VirtualFileSystem.Providers
{
	public abstract class AbstractHashingService
		: AbstractNodeService, IHashingService
	{
		public HashingServiceType ServiceType
		{
			get
			{
				return this.hashingServiceType;
			}
		}
		private readonly HashingServiceType hashingServiceType;

		protected AbstractHashingService(INode node, HashingServiceType hashingServiceType)
			: base(node)
		{
			this.hashingServiceType = hashingServiceType;
		}

		public virtual HashValue ComputeHash()
		{
			return ComputeHash(0, -1);
		}

		public abstract HashValue ComputeHash(long offset, long length);

		public virtual HashValue ComputeHash(HashValue inputResult)
		{
			return ComputeHash(inputResult, 0, -1);
		}

		public virtual HashValue ComputeHash(HashValue inputResult, long outputOffset, long outputLength)
		{
			var service = (IHashingService)this.OperatingNode.GetService((HashingServiceType)this.ServiceType.Clone(inputResult.AlgorithmName));
			var result = service.ComputeHash(inputResult.Offset, inputResult.Length);

			if (result.Equals(inputResult))
			{
				return inputResult;
			}

			return ComputeHash(outputOffset, outputLength);
		}
	}
}
