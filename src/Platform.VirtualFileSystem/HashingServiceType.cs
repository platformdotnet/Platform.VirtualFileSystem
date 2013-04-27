using System;
using System.Security.Cryptography;

namespace Platform.VirtualFileSystem
{
	public abstract class HashingServiceType
		: ServiceType, ICloneable
	{
		public virtual Type AlgorithmType { get; private set; }

		public virtual string AlgorithmName
		{
			get
			{
				if (this.algorithmName == null)
				{
					if (this.algorithmName == null)
					{
						return "none";
					}
					else
					{
						return this.AlgorithmType.Name;
					}
				}
				else
				{
					return this.algorithmName;
				}
			}
			set
			{
				switch (value)
				{
					case "md5":
						this.algorithmName = value;
						this.AlgorithmType = typeof(MD5CryptoServiceProvider);
						break;
					case "sha1":
						this.algorithmName = value;
						this.AlgorithmType = typeof(SHA1Managed);
						break;
					case "sha256":
						this.algorithmName = value;
						this.AlgorithmType = typeof(SHA256Managed);
						break;
					case "sha384":
						this.algorithmName = value;
						this.AlgorithmType = typeof(SHA384Managed);
						break;
					case "sha512":
						this.algorithmName = value;
						this.AlgorithmType = typeof(SHA512Managed);
						break;
					case "null":
					case "none":
						this.algorithmName = value;
						this.AlgorithmType = null;
						break;
					default:
						
						this.AlgorithmType = Type.GetType(value, false, true);
						
						if (this.AlgorithmType == null || !(typeof(HashAlgorithm).IsAssignableFrom(this.AlgorithmType)))
						{
							this.AlgorithmType = Type.GetType(value, false, true);
						}

						if (this.AlgorithmType == null || !(typeof(HashAlgorithm).IsAssignableFrom(this.AlgorithmType)))
						{
							this.AlgorithmType = Type.GetType(typeof(MD5CryptoServiceProvider).Namespace + "." + value, false, true);
						}

						if (this.AlgorithmType == null || !(typeof(HashAlgorithm).IsAssignableFrom(this.AlgorithmType)))
						{
							this.AlgorithmType = Type.GetType(typeof(MD5CryptoServiceProvider).Namespace + "." + value + "CryptoServiceProvider", false, true);
						}

						if (this.AlgorithmType == null || !(typeof(HashAlgorithm).IsAssignableFrom(this.AlgorithmType)))
						{
							this.AlgorithmType = Type.GetType(typeof(MD5CryptoServiceProvider).Namespace + "." + value + "Managed", false, true);
						}

						if (this.AlgorithmType == null || !(typeof(HashAlgorithm).IsAssignableFrom(this.AlgorithmType)))
						{
							throw new NotSupportedException("Algorithm Type: " + this.AlgorithmName);
						}
						
						break;
				}
			}
		}
		private string algorithmName;

		protected HashingServiceType(Type serviceType)
			: this(serviceType, "md5")
		{
		}

		protected HashingServiceType(Type serviceType, string algorithmName)
			: base(serviceType)
		{
			this.AlgorithmName = algorithmName;
		}

		public virtual object Clone()
		{
			return this.Clone(this.AlgorithmName);
		}

		public virtual object Clone(string newAlgorithmType)
		{
			var retval = (HashingServiceType)this.MemberwiseClone();

			retval.AlgorithmName = newAlgorithmType;

			return retval;
		}
	}
}