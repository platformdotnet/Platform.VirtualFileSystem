#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Platform.VirtualFileSystem.Providers.Local
{
	public abstract class Native
	{
		public static Native GetInstance()
		{
			return instance;
		}

		private static readonly Native instance;

		static Native()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT
				|| Environment.OSVersion.Platform == PlatformID.Win32S
				|| Environment.OSVersion.Platform == PlatformID.Win32Windows
				|| Environment.OSVersion.Platform == PlatformID.WinCE)
			{
				instance = new NativeWin32();
			}
			else
			{
				instance = new NativePosix();
			}
		}

		public virtual string DefaultContentName
		{
			get
			{
				return "";
			}
		}

		public struct ContentInfo
		{
			public string Name { get; set; }

			public long Length { get; set; }

			public ContentInfo(string name, long length) : this()
			{
				Name = name;
				Length = length;
			}	
		}

		public virtual IEnumerable<ContentInfo> GetContentInfos(string path)
		{
			throw new NotSupportedException();
		}

		public virtual string GetShortPath(string path)
		{
			return path;
		}

		public virtual void DeleteFileContent(string path, string contentName)
		{
			if (contentName.IsNullOrEmpty())
			{
				System.IO.File.Delete(path);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public virtual string GetSymbolicLinkTarget(string path)
		{
			return null;
		}

		public virtual NodeType GetSymbolicLinkTargetType(string path)
		{
			return null;
		}

		public virtual bool SupportsAlternateContentStreams
		{
			get
			{
				return false;
			}
		}

		public virtual void CreateHardLink(string path, string target)
		{
			throw new NotSupportedException();
		}

		public virtual Stream OpenAlternateContentStream(string path, string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (contentName.IsNullOrEmpty())
			{
				return new FileStream(path, fileMode, fileAccess, fileShare);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public virtual IEnumerable<string> ListExtendedAttributes(string path)
		{
			if (SupportsAlternateContentStreams)
			{
				foreach (var contentInfo in GetContentInfos(path))
				{
					if (IsAlternateStreamExtendedAttribute(contentInfo.Name))
					{
						yield return contentInfo.Name.Substring(4);
					}
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		private bool IsAlternateStreamExtendedAttribute(string contentName)
		{
			return contentName.Length > 4 && contentName.StartsWith("$XA.");
		}

		private string GetExtendedAttributeAlternateStreamName(string attributeName)
		{
			return "$XA." + attributeName;
		}

		public virtual void SetExtendedAttribute(string path, string attributeName, byte[] value, int offset, int count)
		{
			if (SupportsAlternateContentStreams)
			{
				if (value == null)
				{
					try
					{
						this.DeleteFileContent(path, GetExtendedAttributeAlternateStreamName(attributeName));
					}
					catch (FileNodeNotFoundException)
					{
					}
				}
				else
				{
					for (var i = 0; i < 16; i++)
					{
						using (var stream = this.OpenAlternateContentStream(path, GetExtendedAttributeAlternateStreamName(attributeName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
						{
							stream.Write((byte[]) value, 0, ((byte[]) value).Length);
						}

						System.Threading.Thread.Sleep(50);
					}
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public virtual byte[] GetExtendedAttribute(string path, string attributeName)
		{
			if (this.SupportsAlternateContentStreams)
			{
				var bytes = new List<byte>();

				try
				{
					using (var stream = this.OpenAlternateContentStream(path, GetExtendedAttributeAlternateStreamName(attributeName), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
						int x;

						while ((x = stream.ReadByte()) != -1)
						{
							bytes.Add((byte)x);
						}
					}
				}
				catch (IOException)
				{
					return null;
				}

				return bytes.ToArray();
			}
			else
			{
				throw new NotSupportedException();
			}
		}
	}
}
