using System;
using System.Security.Cryptography;
using Platform.IO;
using Platform.Text;

namespace Platform.VirtualFileSystem.Providers
{
	public class StandardStreamHashingService
		: AbstractStreamHashingService
	{
		private IFile file;
		private readonly HashAlgorithm hashAlgorithm;

		public StandardStreamHashingService(IFile file, StreamHashingServiceType serviceType)
			: base(file, serviceType)
		{
			this.file = file;
			this.hashAlgorithm = (HashAlgorithm)Activator.CreateInstance(this.ServiceType.AlgorithmType);
		}

		public override HashValue ComputeHash()
		{
			return ComputeHash(0, -1);
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			MeteringStream stream;

			if (offset == 0 && length == -1 && !this.OperatingStream.CanSeek)
			{
				var retval = StandardFileHashingService.GetHashFromCache(this.OperatingNode, this.ServiceType.AlgorithmName);

				if (retval != null)
				{
					return retval.Value;
				}

				stream = new MeteringStream(this.OperatingStream);

				return new HashValue(this.hashAlgorithm.ComputeHash(stream), this.ServiceType.AlgorithmName, 0, Convert.ToInt32(stream.ReadMeter.Value));
			}
			else
			{
				if (offset == 0 && length == this.OperatingNode.Length)
				{
					if (StandardFileHashingService.ConfigurationSection.Default.CanCacheHash(this.OperatingNode))
					{
						string s;

						if ((s = Convert.ToString(this.OperatingNode.Attributes["extended:" + ServiceType.AlgorithmName])) != "")
						{
							return new HashValue(TextConversion.FromHexString(s), this.ServiceType.AlgorithmName, 0, -1);
						}
					}
				}

				if (!this.OperatingStream.CanSeek)
				{
					throw new NotSupportedException("ComputeHash_StreamCannotSeek");
				}

				stream = new MeteringStream(new PartialStream(this.OperatingStream, offset, length));

				return new HashValue(this.hashAlgorithm.ComputeHash(stream), this.ServiceType.AlgorithmName, offset, Convert.ToInt64(stream.ReadMeter.Value));
			}
		}
	}
}
