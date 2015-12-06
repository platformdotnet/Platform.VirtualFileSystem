using System;
using System.Security.Cryptography;

namespace Platform.VirtualFileSystem
{
	public class StandardHashingService
		: AbstractRunnable, IValued
	{
		private readonly IFile file;
		private readonly HashAlgorithm hashAlgorithm;

		public StandardHashingService(INode node, string algorithm)
		{
			if (!node.NodeType.Equals(NodeType.File))
			{
				throw new ArgumentException("", "node");
			}

			this.file = (IFile)node;

			switch (algorithm.ToLower())
			{
				case "md5":
					this.hashAlgorithm = new MD5CryptoServiceProvider();
					break;
				case "sha1":
					this.hashAlgorithm = new SHA1Managed();
					break;
				case "sha256":
					this.hashAlgorithm = new SHA256Managed();
					break;
				case "sha384":
					this.hashAlgorithm = new SHA384Managed();
					break;
				case "sha512":
					this.hashAlgorithm = new SHA512Managed();
					break;
				default:
					try
					{
						this.hashAlgorithm = (HashAlgorithm)Activator.CreateInstance(Type.GetType(algorithm));
					}
					catch (Exception)
					{
						throw new ArgumentException("Unknown hashing algorithm", algorithm);
					}
					break;
			}
		}

		public override void Run()
		{
			using (var stream = this.file.GetContent().GetInputStream())
			{
				this.hashAlgorithm.ComputeHash(stream);
			}
		}

		public object Value
		{
			get
			{
				return this.hashAlgorithm.Hash;
			}
		}
	}
}