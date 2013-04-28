using System;
using System.Collections;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class CompositeMediaFileFactory
		: MediaFileFactory
	{
		public virtual IList ComponentFactories { get; private set; }

		public CompositeMediaFileFactory()
		{
			this.ComponentFactories = new ArrayList();
		}

		public override IMediaFile NewMediaFile(IFile file, MediaFileNodeType mediaFileNodeType)
		{
			var lastException = new NotSupportedException();

			foreach (MediaFileFactory factory in this.ComponentFactories)
			{
				try
				{
					return factory.NewMediaFile(file, mediaFileNodeType);
				}
				catch (NotSupportedException e)
				{
					lastException = e;
				}
			}

			throw lastException;
		}

	}
}
