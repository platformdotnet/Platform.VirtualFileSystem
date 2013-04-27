using System;

namespace Platform.VirtualFileSystem.Network.Protocol
{
	public sealed class ErrorCode
	{
		public static readonly string ENDOFFILE = "ENDOFFILE";
		public static readonly string NOTSUPPORTED = "NOTSUPPORTED";
		public static readonly string FILEACCESS = "FILEACCESS";
		public static readonly string IOERROR = "IOERROR";
		public static readonly string FILENOTFOUND = "FILENOTFOUND";
		public static readonly string DIRECTORYNOTFOUND = "DIRECTORYNOTFOUND";
		public static readonly string UNEXPECTED = "UNEXPECTED";
		public static readonly string MISSINGPARAM = "MISSINGPARAMS";
		public static readonly string TOOMANYPARAMS = "TOOMANYPARAMS";
		public static readonly string INVALIDPARAM = "INVALIDPARAM";
		public static readonly string INVALIDVALUE = "INVALIDVALUE";
		public static readonly string MALFORMEDURI= "MALFORMEDURI";
		public static readonly string INVALIDCOMMAND = "INVALIDCOMMAND";
		public static readonly string UNEXPECTEDCOMMAND = "UNEXPECTEDCOMMAND";
	}
}
 