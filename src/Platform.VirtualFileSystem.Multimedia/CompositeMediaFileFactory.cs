using System;
using System.Collections;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Summary description for CompositeMediaFileFactory.
	/// </summary>
	public class CompositeMediaFileFactory
		: MediaFileFactory
	{
		/// <summary>
		///  Gets
		/// </summary>
		public virtual IList ComponentFactories
		{
			get
			{
				return m_ComponentFactories;
			}
		}
		/// <summary>
		/// <see cref="ComponentFactories"/>
		/// </summary>
		private readonly IList m_ComponentFactories;

		public CompositeMediaFileFactory()
		{
			m_ComponentFactories = new ArrayList();
		}

		public override IMediaFile NewMediaFile(IFile file, MediaFileNodeType mediaFileNodeType)
		{
			NotSupportedException lastE;

			lastE = new NotSupportedException();

			foreach (MediaFileFactory factory in m_ComponentFactories)
			{
				try
				{
					return factory.NewMediaFile(file, mediaFileNodeType);
				}
				catch (NotSupportedException e)
				{
					lastE = e;
				}
			}

			throw lastE;
		}

	}
}
