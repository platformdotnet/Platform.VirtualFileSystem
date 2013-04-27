using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using Platform.IO;

namespace Platform.VirtualFileSystem.Providers
{
	public class StandardDirectoryHashingService
		: AbstractDirectoryHashingService
	{
		private readonly HashAlgorithm hashAlgorithm;

		public StandardDirectoryHashingService(INode directory, DirectoryHashingServiceType serviceType)
			: base((IDirectory)directory, serviceType)
		{
			hashAlgorithm = new SHA512Managed();
		}

		private void WriteAttribute(BinaryWriter writer, INode node, string name)
		{
			var value = node.Attributes[name];

			if (value == null)
			{
				return;
			}

			if (value is DateTime)
			{
				writer.Write(((DateTime)value).ToUniversalTime().Ticks);
			}
			else if (value.GetType() == typeof(string))
			{
				writer.Write((string)value);
			}
			else if (value.GetType() == typeof(short))
			{
				writer.Write((short)value);
			}
			else if (value.GetType() == typeof(int))
			{
				writer.Write((int)value);
			}
			else if (value.GetType() == typeof(long))
			{
				writer.Write((long)value);
			}
			else if (value.GetType() == typeof(bool))
			{
				writer.Write((bool)value);
			}
			else
			{
				typeof(BinaryWriter).InvokeMember("Write", BindingFlags.Public | BindingFlags.InvokeMethod, null, writer, new object[] { value });
			}
		}

		public override HashValue ComputeHash(long offset, long length)
		{
			int dircount;
			int dircountr;

			var stream = new MemoryStream();			
			var binaryWriter = new BinaryWriter(stream);
			var attributesStream = new MemoryStream();

			var filecount = dircount = 0;
			var filecountr = dircountr = 0;
			
			var attributesBinaryWriter = new BinaryWriter(attributesStream);
			
			this.OperatingNode.Refresh();

			foreach (var node in this.OperatingNode.GetFiles().Sorted<IFile>((n1, n2) => StringComparer.Ordinal.Compare(n1.Name, n2.Name)))
			{
				var nodeRefreshed = false;

				attributesBinaryWriter.Write(node.Name);
								
				foreach (var included in this.ServiceType.IncludedFileAttributes)
				{
					if (!nodeRefreshed)
					{
						node.Refresh();
						nodeRefreshed = true;
					}

					WriteAttribute(attributesBinaryWriter, node, included);
				}

				if (this.ServiceType.AlgorithmType != null)
				{
					var service = (IHashingService) node.GetService(new FileHashingServiceType(this.ServiceType.AlgorithmName));

					service.ComputeHash(offset, length).WriteTo(binaryWriter);
				}

				filecount++;
				filecountr++;
			}

			foreach (var node in this.OperatingNode.GetDirectories().Sorted<IDirectory>(
				(n1, n2) => StringComparer.Ordinal.Compare(n1.Name, n2.Name)))
			{
				var nodeRefreshed = false;

				attributesBinaryWriter.Write(node.Name);
								
				foreach (var included in this.ServiceType.IncludedDirectoryAttributes)
				{
					if (!nodeRefreshed)
					{						
						node.Refresh();
						nodeRefreshed = true;
					}

					WriteAttribute(attributesBinaryWriter, node, included);
				}

				if (this.ServiceType.Recursive)
				{
					int x;

					var service = (IHashingService)node.GetService(this.ServiceType);
					var result = service.ComputeHash(offset, length);
					var reader = new BinaryReader(new MemoryStream(result.Value));

					x = reader.ReadInt32();
					x = reader.ReadInt32();

					dircountr += reader.ReadInt32();
					filecountr += reader.ReadInt32();

					result.WriteTo(binaryWriter);
				}

				dircount++;
				dircountr++;
			}

			binaryWriter.Flush();
			attributesBinaryWriter.Flush();

			var resultBuffer = new byte[(hashAlgorithm.HashSize / 8 * 2) + 16];
			var memoryStream = new MemoryStream(resultBuffer);
			var resultWriter = new BinaryWriter(memoryStream);

			resultWriter.Write(dircount);
			resultWriter.Write(filecount);
			resultWriter.Write(dircountr);
			resultWriter.Write(filecountr);
			
			resultWriter.Write(hashAlgorithm.ComputeHash(attributesStream.GetBuffer(), 0, (int)attributesStream.Length));
			resultWriter.Write(hashAlgorithm.ComputeHash(stream.GetBuffer(), 0, (int)stream.Length));
			resultWriter.Flush();

			return new HashValue(resultBuffer, "sha512", offset, length);
		}
	}
}
