using System;
using System.Security.Cryptography;
using System.Text;
using Platform.Text;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem
{
	public class StandardTempIdentityFileService
		: AbstractService, ITempIdentityFileService
	{
		private readonly IFile file;
		private readonly IFile tempFile;
		private TempIdentityFileServiceType serviceType;

		public static bool IsTempIdentityFile(string name)
		{
			return name.StartsWith("$TMP_ID");
		}

		public StandardTempIdentityFileService(IFile file, TempIdentityFileServiceType serviceType)
		{			
			IDirectory dir;

			var buffer = new StringBuilder(file.Address.Uri.Length * 2);

			this.file = file;
			this.serviceType= serviceType;

			buffer.Append("$TMP_ID_");
			buffer.Append(file.Name).Append('_');

			if (serviceType.TempFileSystem != null)
			{				
				buffer.Append(TextConversion.ToBase32String(new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(file.Address.Uri))).Trim('='));
				buffer.Append('_');
			}

			string s;

			if (serviceType.UniqueIdentifier.IndexOf(PredicateUtils.Not<char>(Char.IsLetterOrDigit)) >= 0)
			{
				s = TextConversion.ToBase32String(Encoding.ASCII.GetBytes(serviceType.UniqueIdentifier)).Trim('=');
			}
			else
			{
				s = serviceType.UniqueIdentifier;
			}

			buffer.Append(s);
						
			if (serviceType.TempFileSystem == null)
			{
				dir = file.ParentDirectory;

				this.tempFile = dir.ResolveFile(buffer.ToString());
			}
			else
			{
				var tempFileSystem = serviceType.TempFileSystem;

				dir = tempFileSystem.ResolveDirectory("/VFSTempIdentity");
			
				dir.Create();

				this.tempFile = dir.ResolveFile(buffer.ToString());
			}
		}

		public virtual IFile GetTempFile()
		{
			return this.tempFile;
		}

		public virtual IFile GetOriginalFile()
		{
			return this.file;
		}
	}
}