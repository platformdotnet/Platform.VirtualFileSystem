using System;
using System.IO;
using System.Text;

namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface representing the contents of a node or file.
	/// </summary>
	/// <remarks>
	/// <p>This interface can be used to get streams to read and write to and from a file.</p>
	/// <see cref="IFile.GetContent()"/>
	/// </remarks>
	public interface INodeContent
	{
		/// <summary>
		/// Delete the contents of the file.
		/// </summary>
		void Delete();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileMode"></param>
		/// <param name="fileAccess"></param>
		/// <param name="fileShare"></param>
		/// <returns></returns>
		Stream OpenStream(FileMode fileMode, FileAccess fileAccess, FileShare fileShare);

		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <remarks>
		/// The default <see cref="FileShare"/> for the stream will be <see cref="FileShare.ReadWrite"/>.
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for reading.
		/// </returns>
		Stream GetInputStream();

		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// reasons.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for reading.
		/// </returns>
		Stream GetInputStream(FileShare sharing);
		
		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <param name="encodingHint">
		/// A reference to a string that will receive the encoding of the <see cref="InputStream"/>.
		/// Most file systems don't support encoding detection on files and will set encoding to
		/// <c>null</c>.
		/// </param>
		/// <remarks>
		/// The default <see cref="FileShare"/> for the stream will be <see cref="FileShare.ReadWrite"/>.
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for reading.
		/// </returns>
		Stream GetInputStream(out string encodingHint);

		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <param name="encoding">
		/// A reference to a string that will receive the encoding of the <see cref="InputStream"/>.
		/// Most file systems don't support encoding detection on files and will set encoding to
		/// <c>null</c>.  The encoding string may not necessarily be supported by the CLR framework.
		/// </param>
		/// <param name="encodingHint">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for reading.
		/// </returns>
		Stream GetInputStream(out string encodingHint, FileShare sharing);
		
		Stream GetInputStream(FileMode mode);

		Stream GetInputStream(FileMode mode, FileShare sharing);

		Stream GetInputStream(out string encodingHint, FileMode mode);

		Stream GetInputStream(out string encodingHint, FileMode mode, FileShare sharing);

		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <remarks>
		/// The default <see cref="FileShare"/> for the stream will be <see cref="FileShare.Read"/>.
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for writing.
		/// </returns>
		Stream GetOutputStream();

		/// <summary>
		/// Gets a stream for reading from this file.
		/// </summary>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for writing.
		/// </returns>
		Stream GetOutputStream(FileShare sharing);

		/// <summary>
		/// Gets a stream for writing from this file.
		/// </summary>
		/// <param name="encodingHint">
		/// The hint about the encoding of the stream.  Most file systems don't support encoding markings
		/// and will ignore this value.
		/// </param>
		/// <remarks>
		/// The default <see cref="FileShare"/> for the stream will be <see cref="FileShare.ReadWrite"/>.
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for writing.
		/// </returns>
		Stream GetOutputStream(string encodingHint);

		/// <summary>
		/// Gets a stream for writing from this file.
		/// </summary>
		/// <param name="encodingHint">
		/// The hint about the encoding of the stream.  Most file systems don't support encoding markings
		/// and will ignore this value.
		/// </param>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A new <see cref="Stream"/> for writing.
		/// </returns>
		Stream GetOutputStream(string encodingHint, FileShare sharing);


		Stream GetOutputStream(FileMode mode);

		Stream GetOutputStream(FileMode mode, FileShare sharing);

		Stream GetOutputStream(string encodingHint, FileMode mode);

		Stream GetOutputStream(string encodingHint, FileMode mode, FileShare sharing);
		
		/// <summary>
		/// Get the <c>TextReader</c> for reading from the file.
		/// </summary>
		/// <remarks>
		/// If the encoding can't be automatically determined, the system default encoding is used.
		/// </remarks>
		TextReader GetReader();

		/// <summary>
		/// Get the <c>TextReader</c> for reading from the file.
		/// </summary>
		/// <remarks>
		/// If the encoding can't be automatically determined, the system default encoding is used.
		/// </remarks>
		/// <returns>
		/// A <see cref="TextReader"/> for the file.
		/// </returns>
		TextReader GetReader(FileShare sharing);

		/// <summary>
		/// Get the <c>TextReader</c> for reading from the file.
		/// </summary>
		/// <returns>
		/// A <see cref="TextReader"/> for the file.
		/// </returns>
		TextReader GetReader(out Encoding encoding);

		/// <summary>
		/// Get the <c>TextReader</c> for reading from the file.
		/// </summary>
		/// <param name="encoding">
		/// A reference to a string that will receive the encoding of the <see cref="Reader"/>.
		/// Most file systems don't support encoding detection on files and will set encoding to
		/// <c>null</c>.  The encoding string may not necessarily be supported by the CLR framework.
		/// </param>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A <see cref="TextReader"/> for the file.
		/// </returns>
		TextReader GetReader(out Encoding encoding, FileShare sharing);

		/// <summary>
		/// Get the <c>TextWriter</c> for writing to the file.
		/// </summary>
		/// <remarks>
		/// The text writer will use the default encoding (UTF-8).
		/// </remarks>
		/// <returns>
		/// A <see cref="TextWriter"/> for the file.
		/// </returns>
		TextWriter GetWriter();

		/// <summary>
		/// Get the <c>TextWriter</c> for writing to the file.
		/// </summary>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// <p>
		/// The text writer will use the default encoding (UTF-8).
		/// </p>
		/// </remarks>
		/// <returns>
		/// A <see cref="TextWriter"/> for the file.
		/// </returns>
		TextWriter GetWriter(FileShare sharing);

		/// <summary>
		/// Get the <c>TextWriter</c> for writing to the file.
		/// </summary>
		/// <param name="encoding">
		/// The encoding for the writer.
		/// </param>
		/// <returns>
		/// A <see cref="TextWriter"/> for the file.
		/// </returns>
		TextWriter GetWriter(Encoding encoding);

		/// <summary>
		/// Get the <c>TextWriter</c> for writing to the file.
		/// </summary>
		/// <param name="sharing">
		/// Specifies the sharing level preferred for this file.
		/// </param>
		/// <remarks>
		/// <p>
		/// The sharing level may or may not be honoured depending on the underlying filesystem support.
		/// For example, web file systems always support read sharing.
		/// </p>
		/// </remarks>
		/// <returns>
		/// A <see cref="TextWriter"/> for the file.
		/// </returns>
		TextWriter GetWriter(Encoding encoding, FileShare sharing);
	}
}
