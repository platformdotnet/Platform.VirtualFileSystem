using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Mono.Unix;
using Mono.Unix.Native;

namespace Platform.VirtualFileSystem.Providers.Local
{
	public class NativePosix
		: NativeManaged
	{
		public override bool SupportsAlternateContentStreams
		{
			get
			{
				return false;
			}
		}

		public override string GetSymbolicLinkTarget(string path)
		{
			UnixSymbolicLinkInfo symbolicLinkInfo;

			try
			{
				symbolicLinkInfo = new UnixSymbolicLinkInfo(path);
			}
			catch (InvalidOperationException)
			{
				/* Not a symbolic link */

				return null;
			}

			string retval = null;

			try
			{

				if (!symbolicLinkInfo.IsSymbolicLink)
				{
					return null;
				}

				retval = symbolicLinkInfo.ContentsPath;

				if (retval != "/" && retval.EndsWith("/"))
				{
					retval = retval.Substring(0, retval.Length - 1);
				}
			}
			catch (OutOfMemoryException)
			{
				throw;
			}
			catch (StackOverflowException)
			{
				throw;
			}
			catch
			{
			}

			return retval;
		}

		public override NodeType GetSymbolicLinkTargetType(string path)
		{
			UnixSymbolicLinkInfo obj;

			try
			{
				obj = new UnixSymbolicLinkInfo(path);
			}
			catch (InvalidOperationException)
			{
				/* Not a symbolic link */

				return null;
			}

			try
			{
				if (!obj.IsSymbolicLink)
				{
					return null;
				}

				var retval = obj.GetContents();

				if (retval == null)
				{
					return null;
				}

				var isDirectory = retval.IsDirectory;

				if (isDirectory)
				{
					return NodeType.Directory;
				}

				var isRegularFile = retval.IsRegularFile;

				if (isRegularFile)
				{
					return NodeType.File;
				}

				return null;
			}
			catch (OutOfMemoryException)
			{
				throw;
			}
			catch (StackOverflowException)
			{
				throw;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public override IEnumerable<string> ListExtendedAttributes(string path)
		{
			string[] retval;

			var size = Syscall.listxattr(path, out retval);

			if (size == -1)
			{
				var err = Marshal.GetLastWin32Error();

				if (err == (int)Errno.ERANGE)
				{
					yield break;
				}
				else if (err == (int)Errno.ENOENT)
				{
					throw new FileNodeNotFoundException();
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			for (var i = 0; i < retval.Length; i++)
			{
				if (retval[i].StartsWith("user."))
				{
					yield return retval[i].Substring(5);
				}

				yield return retval[i];
			}
		}

		public override byte[] GetExtendedAttribute(string path, string attributeName)
		{
			long size;
			var buffer = new byte[256];

			if (attributeName.IndexOf('.') < 0)
			{
				attributeName = "user." + attributeName;
			}

			while (true)
			{
				size = Syscall.getxattr(path, attributeName, buffer, (ulong)buffer.Length);

				if (size > 0)
				{
					break;
				}

				var err = Marshal.GetLastWin32Error();

				if (err == (int)Errno.ERANGE)
				{
					buffer = new byte[size];

					continue;
				}
				else if (err == (int)Errno.EOPNOTSUPP)
				{
					throw new NotSupportedException("GetExtendedAttribute: " + path);
				}
				else if (err == (int)Errno.ENOENT)
				{
					throw new FileNodeNotFoundException("FileNotFound: " + path);
				}
				else
				{
					throw new NotSupportedException("GetExtendedAttribute: " + path + " (errno=" + err + ")");
				}
			}

			if (size == buffer.Length)
			{
				return buffer;
			}
			else
			{
				return ArrayUtils.NewArray(buffer, 0, (int)size);
			}
		}

		public override void SetExtendedAttribute(string path, string attributeName, byte[] value, int offset, int count)
		{
			int x;
			int err;

			if (attributeName.IndexOf('.') < 0)
			{
				attributeName = "user." + attributeName;
			}

			if (value == null)
			{
				x = Syscall.removexattr(path, attributeName);

				if (x == 0)
				{
					return;
				}

				err = Marshal.GetLastWin32Error();

				if (err == (int)Errno.ENOENT)
				{
					throw new FileNotFoundException();
				}
				if (err == (int)Errno.ENODATA)
				{
					// Deleting a non-existant attribute is ok
				}
				else if (err == (int)Errno.EOPNOTSUPP)
				{
					throw new NotSupportedException("File system for file [" + path + "] does not support extended attributes");
				}
			}
			else
			{
				x = Syscall.setxattr(path, attributeName, value, (ulong)value.Length, 0);

				if (x == 0)
				{
					return;
				}

				err = Marshal.GetLastWin32Error();

				if (err == (int)Errno.ENOSPC || err == (int)Errno.EDQUOT)
				{
					throw new System.IO.IOException("Not enough disk space");
				}
				else if (err == (int)Errno.EOPNOTSUPP)
				{
					throw new NotSupportedException("File system for file [" + path + "] does not support extended attributes");
				}
			}
		}
	}
}
