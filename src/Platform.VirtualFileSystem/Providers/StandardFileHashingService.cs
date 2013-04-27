using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Platform;
using Platform.IO;
using Platform.Text;
using Platform.Xml.Serialization;

namespace Platform.VirtualFileSystem.Providers
{	
	public class StandardFileHashingService
		: AbstractFileHashingService
	{
		public class ConfigurationSectionHandler
			: XmlConfigurationBlockSectionHandler<ConfigurationSection>
		{
		}

		[XmlElement("Configuration")]
		public class ConfigurationSection
		{
			public static ConfigurationSection Default
			{
				get
				{
					var retval = ConfigurationBlock<ConfigurationSection>.Load("Platform/VirtualFileSystem/StandardFileHashingService/Configuration");

					if (retval == null)
					{
						retval = new ConfigurationSection();
					}

					return retval;
				}
			}

			[XmlElement]
			public string CacheHashFileUriRegex
			{
				get
				{
					return this.cacheHashFileUriRegex;
				}
				set
				{
					this.cacheHashFileUriRegex = value;

					this.cacheHashFileUriRegexRegex = new Regex(this.cacheHashFileUriRegex, RegexOptions.IgnoreCase);
				}
			}
			private string cacheHashFileUriRegex = "";

			private Regex cacheHashFileUriRegexRegex;

			public virtual bool CanCacheHash(INode node)
			{
				if (this.cacheHashFileUriRegex == "")
				{
					return false;
				}

				return this.cacheHashFileUriRegexRegex.IsMatch(node.Address.Uri);
			}
		}

		public static HashValue? GetHashFromCache(IFile file, string algorithmName)
		{
			if (algorithmName == null)
			{
				return null;
			}

			if (StandardFileHashingService.ConfigurationSection.Default.CanCacheHash(file))
			{
				string s;
				var bytes = (byte[]) file.Attributes["extended:" + algorithmName];
			
				if (bytes == null)
				{
					return null;
				}

				if ((s = Encoding.ASCII.GetString(bytes)) != "")
				{
					return new HashValue(TextConversion.FromHexString(s), algorithmName, 0, -1);
				}
			}

			return null;
		}

		public static HashValue SaveHashToCache(IFile file, string algorithmName, HashValue hashValue)
		{
			SaveHashToCache(file, algorithmName, hashValue.TextValue);

			return hashValue;
		}

		public static string SaveHashToCache(IFile file, string algorithmName, string hashValue)
		{
			return SaveHashToCache(file, algorithmName, hashValue, file);
		}

		public static string SaveHashToCache(IFile file, string algorithmName, string hashValue, IFile referenceFile)
		{
			if (ConfigurationSection.Default.CanCacheHash(referenceFile))
			{
				var current = file.Attributes["extended" + algorithmName] as byte[];
				var newvalue = Encoding.ASCII.GetBytes(hashValue);

				if (current != null)
				{
					if (current.ElementsAreEqual(newvalue))
					{
						return hashValue;
					}
				}

				file.Attributes["extended:" + algorithmName] = newvalue;
			}

			return hashValue;
		}

		protected HashAlgorithm Algorithm { get; private set; }

		public StandardFileHashingService(INode file, FileHashingServiceType serviceType)
			: base((IFile)file, serviceType)
		{
			this.Algorithm = (HashAlgorithm)Activator.CreateInstance(this.ServiceType.AlgorithmType);
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			HashValue retval;

			if (length == 0)
			{
				return new HashValue(this.Algorithm.ComputeHash(new byte[0]), this.ServiceType.AlgorithmName, offset, 0L);
			}

			if (offset == 0 && length == -1 && ServiceType.AlgorithmType != null)
			{
				var retvaln = GetHashFromCache(this.OperatingNode, this.ServiceType.AlgorithmName);

				if (retvaln != null)
				{
					return retvaln.Value;
				}
			}

			var stream = this.OperatingNode.GetContent().GetInputStream();

			if (!(offset == 0 && length == -1 && !stream.CanSeek))
			{
				stream = new PartialStream(stream, offset, length);

				if (length <= 0)
				{
					stream = new BufferedStream(stream, 128 * 1024);
				}
				else
				{
					stream = new BufferedStream(stream, Math.Min(128 * 1024, (int)length));
				}
			}

			var meteringStream = new MeteringStream(stream);

			using (meteringStream)
			{
				retval = new HashValue(this.Algorithm.ComputeHash(meteringStream), this.ServiceType.AlgorithmName, offset, Convert.ToInt64(meteringStream.ReadMeter.Value));
			}

			if (offset == 0 && length == -1 && ServiceType.AlgorithmType != null)
			{
				try
				{
				//	SaveHashToCache(this.OperatingNode, this.ServiceType.AlgorithmName, retval);
				}
				catch (IOException)
				{
					// TODO: Log
				}
			}

			return retval;
		}
	}
}
