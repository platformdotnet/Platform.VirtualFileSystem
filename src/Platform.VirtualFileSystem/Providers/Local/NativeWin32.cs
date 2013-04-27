using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Platform.VirtualFileSystem.Providers.Local
{
	public class NativeWin32
		: NativeManaged
	{
		private static class Kernel32
		{
			public const int MAX_PATH = 256; 
			public const string STREAM_SEPERATOR = ":";
			public const int INVALID_HANDLE_VALUE = -1;
			public const int ERROR_FILE_NOT_FOUND = 0x02;
			public const int ERROR_PATH_NOT_FOUND = 0x03;
			public const int ERROR_TOO_MANY_LINKS = 1142;
			
			[StructLayout(LayoutKind.Sequential)]
			public struct LARGE_INTEGER
			{
				public int Low;
				public int High;

				public long ToInt64()
				{
					return ((long)High << 32) + (long)Low;
				}
			}

			[Flags]
			public enum FileFlags : uint
			{
				WriteThrough = 0x80000000,
				Overlapped = 0x40000000,
				NoBuffering = 0x20000000,
				RandomAccess = 0x10000000,
				SequentialScan = 0x8000000,
				DeleteOnClose = 0x4000000,
				BackupSemantics = 0x2000000,
				PosixSemantics = 0x1000000,
				OpenReparsePoint = 0x200000,
				OpenNoRecall = 0x100000
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct WIN32_STREAM_ID
			{
				public int dwStreamID;
				public int dwStreamAttributes;
				public LARGE_INTEGER Size;
				public int dwStreamNameSize;
			}

			[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern int CreateHardLinkW(string path, string target, IntPtr security);

			[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern bool DeleteFileW(string fileName);

			[DllImport("kernel32", SetLastError = true)]
			public static extern int GetLastError();

			[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
			public static extern IntPtr CreateFileW(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

			[DllImport("kernel32", SetLastError = true)]
			public static extern bool CloseHandle(IntPtr hObject);

			[DllImport("kernel32", SetLastError = true)]
			public static extern bool BackupRead(IntPtr hFile, IntPtr pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref IntPtr context);

			[DllImport("kernel32", SetLastError = true)]
			public static extern bool BackupRead(IntPtr hFile, ref WIN32_STREAM_ID pBuffer, int lBytes, ref int lRead, bool bAbort, bool bSecurity, ref IntPtr context);

			[DllImport("kernel32", SetLastError = true)]
			public static extern bool BackupSeek(IntPtr hFile, int dwLowBytesToSeek, int dwHighBytesToSeek, ref int dwLow, ref int dwHigh, ref IntPtr context);
		}
		
		public override string DefaultContentName
		{
			get
			{
				return "$DATA";
			}
		}

		public override void CreateHardLink(string path, string target)
		{
			var x = Kernel32.CreateHardLinkW(path, target, IntPtr.Zero);

			if (x <= 0)
			{
				var error = Kernel32.GetLastError();

				if (error == Kernel32.ERROR_TOO_MANY_LINKS)
				{
					throw new TooManyLinksException();
				}
				else
				{
					throw new IOException(String.Format(GetType().Name + "_CreateHardLink_Error_{0}_Path={1}_Target={2}", error, path, target));
				}
			}
		}

		public override void DeleteFileContent(string path, string contentName)
		{
			string fullPath;

			if (contentName.IsNullOrEmpty())
			{
				fullPath = path;
			}
			else
			{
				fullPath = path + ":" + contentName;
			}

			if (!Kernel32.DeleteFileW(fullPath))
			{
				var errorCode = Kernel32.GetLastError();

				if (errorCode == Kernel32.ERROR_FILE_NOT_FOUND || errorCode == Kernel32.ERROR_PATH_NOT_FOUND)
				{
					throw new FileNodeNotFoundException(fullPath);
				}
				else
				{
					throw new IOException(String.Format("Win32.DeleteFileContent Error={0} Path={1} ContentName={2}", Kernel32.GetLastError(), path, contentName));
				}
			}
		}

		public override bool SupportsAlternateContentStreams
		{
			get
			{
				return true;
			}
		}

		public override Stream OpenAlternateContentStream(string path, string contentName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			var handle = Kernel32.CreateFileW
			(
				path + ":" + contentName,
				(int)fileAccess,
				(int)fileShare,
				IntPtr.Zero,
				(int)fileMode,
				0,
				IntPtr.Zero
			);

			if (handle == new IntPtr(-1))
			{
				var err = Marshal.GetLastWin32Error();

				throw new IOException("Win32Error: " + err);
			}

			var retval = new FileStream(new SafeFileHandle(handle, true), fileAccess);
			
			return retval;
		}

		public override IEnumerable<ContentInfo> GetContentInfos(string path)
		{
			long length;
			var contentInfos = new List<ContentInfo>();

			try
			{
				length = new FileInfo(path).Length;
			}
			catch (FileNotFoundException)
			{
				yield break;
			}

			yield return new ContentInfo(this.DefaultContentName, length);

			var hFile = Kernel32.CreateFileW(path, (int)FileAccess.Read, (int)FileShare.ReadWrite, IntPtr.Zero, (int)FileMode.Open, (int)Kernel32.FileFlags.BackupSemantics, IntPtr.Zero);

			if (hFile.ToInt32() == Kernel32.INVALID_HANDLE_VALUE)
			{
				yield break;
			}

			try
			{
				var sid = new Kernel32.WIN32_STREAM_ID();
				var dwStreamHeaderSize = Marshal.SizeOf(sid);
				var context = new IntPtr();
				var shouldContinue = true;

				while (shouldContinue)
				{
					var lRead = 0;

					shouldContinue = Kernel32.BackupRead(hFile, ref sid, dwStreamHeaderSize, ref lRead, false, false, ref context);

					if (shouldContinue && lRead == dwStreamHeaderSize)
					{
						if (sid.dwStreamNameSize > 0)
						{
							var pName = Marshal.AllocHGlobal(sid.dwStreamNameSize);
							
							lRead = 0;

							try
							{
								shouldContinue = Kernel32.BackupRead(hFile, pName, sid.dwStreamNameSize, ref lRead, false, false, ref context);

								var bName = new char[sid.dwStreamNameSize];

								Marshal.Copy(pName, bName, 0, sid.dwStreamNameSize);

								// Name Format ":NAME:$DATA\0"

								var streamName = new string(bName);
								var i = streamName.IndexOf(Kernel32.STREAM_SEPERATOR, 1, StringComparison.Ordinal);

								if (i > -1)
								{
									streamName = streamName.Substring(1, i - 1);
								}
								else
								{
									i = streamName.IndexOf('\0');

									if (i > -1)
									{
										streamName = streamName.Substring(1, i - 1);
									}
								}

								contentInfos.Add(new ContentInfo(streamName, sid.Size.ToInt64()));
							}
							finally
							{
								Marshal.FreeHGlobal(pName);
							}
						}

						// Skip the stream contents

						var l = 0;
						var h = 0;

						shouldContinue = Kernel32.BackupSeek(hFile, sid.Size.Low, sid.Size.High, ref l, ref h, ref context);
					}
					else
					{
						break;
					}
				}
			}
			finally
			{
				Kernel32.CloseHandle(hFile);
			}

			foreach (var contentInfo in contentInfos)
			{
				yield return contentInfo;
			}
		}

		[DllImport("kernel32.dll", EntryPoint = "GetShortPathNameA", CharSet = CharSet.Ansi)]
		protected static extern int GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);

		public override string GetShortPath(string path)
		{
			var ret = 0;
			var buffer = new StringBuilder(path.Length + 1);

			try
			{
				ret = GetShortPathName(path, buffer, buffer.Capacity);
			}
			catch (DllNotFoundException)
			{
			}

			if (ret == 0)
			{
				return path;
			}
			else
			{
				return buffer.ToString();
			}
		}
	}
}
