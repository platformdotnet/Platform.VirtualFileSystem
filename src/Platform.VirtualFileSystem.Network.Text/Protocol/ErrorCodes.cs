using System;

namespace Platform.VirtualFileSystem.Network.Text.Protocol
{
	public sealed class ErrorCodes
	{
		public static readonly string CANCELLED = "CANCELLED";
		public static readonly string END_OF_FILE = "END_OF_FILE";
		public static readonly string NOT_SUPPORTED = "NOT_SUPPORTED";
		public static readonly string UNAUTHORISED = "UNAUTHORISED";
		public static readonly string FILE_ACCESS = "FILE_ACCESS";
		public static readonly string IO_ERROR = "IO_ERROR";
		public static readonly string FILE_NOT_FOUND = "FILE_NOT_FOUND";
		public static readonly string DIRECTORY_NOT_FOUND = "DIRECTORY_NOT_FOUND";
		public static readonly string UNEXPECTED = "UNEXPECTED";
		public static readonly string MISSING_PARAMS = "MISSING_PARAMS";
		public static readonly string TOO_MANY_PARAMS = "TOO_MANY_PARAMS";
		public static readonly string INVALID_PARAM = "INVALID_PARAM";
		public static readonly string INVALID_VALUE = "INVALID_VALUE";
		public static readonly string MALFORMED_URI = "MALFORMED_URI";
		public static readonly string INVALID_COMMAND = "INVALID_COMMAND";
		public static readonly string UNEXPECTED_COMMAND = "UNEXPECTED_COMMAND";
	}
}
 