using System;
using System.Collections;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Base class for builders of <see cref="IMediaFile"/> instances.
	/// </summary>
	/// <remarks>
	/// Implementers of <see cref="IMediaFile"/> should also implement a <see cref="MediaFileFactory"/>
	/// to provide the <c>Platform.VirtualFileSystem.Multimedia</c> framework a way to build the specific
	/// implementation of <see cref="IMediaFile"/>.
	/// </remarks>
	public abstract class MediaFileFactory
	{
		/// <summary>
		/// A <see cref="MediaFileFactory"/> that builds an <see cref="IMediaFile"/> from muiltiple 
		/// <see cref="MediaFileFactory"/> implementations
		/// </summary>
		private static readonly MediaFileFactory compositeMediaFileFactory;

		/// <summary>
		/// Gets the default <see cref="MediaFileFactory"/>
		/// </summary>
		/// <remarks>
		/// The default <see cref="MediaFileFactory"/> is a <see cref="CompositeMediaFileFactory"/>
		/// that builds <see cref="IMediaFile"/> instances based on a set of <see cref="MediaFileFactory"/>
		/// instnaces declared in the application configuration file
		/// </remarks>
		public static MediaFileFactory Default
		{
			get
			{
				return compositeMediaFileFactory;
			}
		}

		/// <summary>
		/// Static construct.  Builds the default <see cref="MediaFileFactory"/>.
		/// </summary>
		static MediaFileFactory()
		{
			var compositeFactory = new CompositeMediaFileFactory();

			foreach (var factory in ConfigurationSection.GetInstance().MediaFileFactories)
			{
				compositeFactory.ComponentFactories.Add(Activator.CreateInstance(factory.Type));
			}

			compositeMediaFileFactory = compositeFactory;
		}

		/// <summary>
		/// Builds a new <see cref="IMediaFile"/> instance from the given URI
		/// </summary>
		/// <param name="uri">The uri to the file</param>
		/// <param name="mediaFileNodeType">The <see cref="MediaFileNodeType"/> for the <see cref="IMediaFile"/></param>
		/// <returns>
		/// An <see cref="IMediaFile"/> instnace
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// The given <see cref="MediaFileNodeType"/> is not supported
		/// </exception>
		public virtual IMediaFile NewMediaFile(string uri, MediaFileNodeType mediaFileNodeType)
		{
			return NewMediaFile(FileSystemManager.GetManager().ResolveFile(uri), mediaFileNodeType);
		}

		/// <summary>
		/// Builds a new <see cref="IMediaFile"/> instance from the given file
		/// </summary>
		/// <param name="file">The underlying file for the <see cref="IMediaFile"/></param>
		/// <param name="mediaFileNodeType">The <see cref="MediaFileNodeType"/> for the <see cref="IMediaFile"/></param>
		/// <returns>
		/// An <see cref="IMediaFile"/> instnace
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// The given <see cref="MediaFileNodeType"/> is not supported
		/// </exception>
		public abstract IMediaFile NewMediaFile(IFile file, MediaFileNodeType mediaFileNodeType);
	}
}
