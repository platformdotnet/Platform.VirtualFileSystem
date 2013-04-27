namespace Platform.VirtualFileSystem
{
	/// <summary>
	/// Interface for files.
	/// </summary>
	public interface IFile
		: INode
	{
		new IFile Create();
		new IFile Create(bool createParent);

		new IFileAttributes Attributes
		{
			get;
		}

		long? Length
		{
			get;
		}

		/// <summary>
		/// Creates the current file as a hardlink to the specified file.
		/// </summary>
		/// <param name="targetFile">
		/// The file to link to.
		/// </param>
		/// <exception cref="System.IOException">
		/// The current file already exists as a regular file or hardlink.
		/// </exception>
		IFile CreateAsHardLink(IFile targetFile);

		/// <summary>
		/// Creates the current file as a hardlink to the specified file.
		/// </summary>
		/// <param name="targetFile">
		/// The file to link to.
		/// </param>
		/// <param name="overwrite">
		/// True if the file should be overwritten if it already exists.
		/// </param>
		/// <exception cref="System.IOException">
		/// The current file already exists as a regular file or hardlink
		/// and <c>overwrite</c> is false.
		/// </exception>
		IFile CreateAsHardLink(IFile targetFile, bool overwrite);

		/// <summary>
		/// Creates the current file as a hardlink to the specified file.
		/// </summary>
		/// <param name="targetPath">
		/// The path to the file to link to.
		/// </param>
		/// <exception cref="System.IOException">
		/// The current file already exists as a regular file or hardlink.
		/// </exception>
		IFile CreateAsHardLink(string targetPath);

		/// <summary>
		/// Creates the current file as a hardlink to the specified file.
		/// </summary>
		/// <param name="targetFile">
		/// The file to link to.
		/// </param>
		/// <param name="overwrite">
		/// True if the file should be overwritten if it already exists.
		/// </param>
		/// <exception cref="System.IOException">
		/// The current file already exists as a regular file or hardlink
		/// and <c>overwrite</c> is false.
		/// </exception>
		IFile CreateAsHardLink(string targetFile, bool overwrite);

		/// <summary>
		/// Compares this file to another file and checks if that are identical
		/// using a comparison based on the given <see cref="FileComparingFlags"/>.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="flags"></param>
		/// <returns>
		/// True if <c>fileToCompareTo</c> is identical to the current file
		/// given the constraints provided by <see cref="FileComparingFlags"/>.
		/// </returns>
		bool IdenticalTo(IFile fileToCompareTo, FileComparingFlags flags);

		/// <summary>
		/// Refreshes and returns the current file.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Implementers are suggested to implement this member using
		/// explicit interface implementation.
		/// </para>
		/// <seealso cref="INode.Refresh"/>
		/// </remarks>
		/// <returns>
		/// The current file.
		/// </returns>
		new IFile Refresh();
	}
}
