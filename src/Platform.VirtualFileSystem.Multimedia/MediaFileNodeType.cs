using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class MediaFileNodeType
		: NodeType
	{
		public static readonly MediaFileNodeType SoundFile
			= new MediaFileNodeType(MediaFileType.SoundFile);

		public virtual string VideoDevice { get; set; }

		public virtual string AudioDevice { get; set; }

		public virtual MediaFileType MediaFileType { get; private set; }

		public virtual bool UniqueInstance { get; set; }

		/// <summary>
		/// Creates a new <see cref="MediaFileNodeType"/>
		/// </summary>
		public MediaFileNodeType()
			: this(MediaFileType.Unknown)
		{
		}

		/// <summary>
		/// Creates a new <see cref="MediaFileNodeType"/>
		/// </summary>
		/// <param name="uniqueInstance">True if the <see cref="IMediaFile"/> returned should be a new unique instance (uncached)</param>
		public MediaFileNodeType(bool uniqueInstance)
			: this(MediaFileType.Unknown, false)
		{
		}

		/// <summary>
		/// Creates a new <see cref="MediaFileNodeType"/>
		/// </summary>
		/// <param name="mediaFileType">The type of <see cref="IMediaFile"/> to return</param>
		public MediaFileNodeType(MediaFileType mediaFileType)
			: this(mediaFileType, false)
		{		
		}

		/// <summary>
		/// Creates a new <see cref="MediaFileNodeType"/>
		/// </summary>
		/// <param name="uniqueInstance">True if the <see cref="IMediaFile"/> returned should be a new unique instance (uncached)</param>
		public MediaFileNodeType(MediaFileType mediaFileType, bool uniqueInstance)
			: base(mediaFileType.RuntimeType)
		{			
			this.MediaFileType = mediaFileType;
			this.UniqueInstance = uniqueInstance;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ this.MediaFileType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var typedObj = obj as MediaFileNodeType;

			if (typedObj == null)
			{
				return false;
			}

			return base.Equals(obj) && this.MediaFileType == typedObj.MediaFileType
			       && this.AudioDevice == typedObj.AudioDevice
			       && this.VideoDevice == typedObj.VideoDevice;
				
		}
	}
}
