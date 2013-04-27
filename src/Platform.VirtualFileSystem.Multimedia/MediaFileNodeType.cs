using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class MediaFileNodeType
		: NodeType
	{
		public static readonly MediaFileNodeType SoundFile
			= new MediaFileNodeType(MediaFileType.SoundFile);

		/// <summary>
		/// VideoDevice
		/// </summary>
		public virtual string VideoDevice
		{
			get
			{
				return m_VideoDevice;
			}
			set
			{
				m_VideoDevice = value;
			}
		}
		/// <summary>
		/// <see cref="VideoDevice"/>
		/// </summary>
		private string m_VideoDevice;

		/// <summary>
		/// AudioDevice
		/// </summary>
		public virtual string AudioDevice
		{
			get
			{
				return m_AudioDevice;
			}
			set
			{
				m_AudioDevice = value;
			}
		}
		/// <summary>
		/// <see cref="AudioDevice"/>
		/// </summary>
		private string m_AudioDevice;
	
		/// <summary>
		///  MediaFileType
		/// </summary>
		public virtual MediaFileType MediaFileType
		{
			get
			{
				return m_MediaFileType;
			}
		}
		/// <summary>
		/// <see cref="MediaFileType"/>
		/// </summary>
		private MediaFileType m_MediaFileType;

		/// <summary>
		/// UniqueInstance
		/// </summary>
		public virtual bool UniqueInstance
		{
			get
			{
				return m_UniqueInstance;
			}
			set
			{
				m_UniqueInstance = value;
			}
		}
		/// <summary>
		/// <see cref="UniqueInstance"/>
		/// </summary>
		private bool m_UniqueInstance;

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
			m_MediaFileType = mediaFileType;
			m_UniqueInstance = uniqueInstance;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ m_MediaFileType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			MediaFileNodeType typedObj;
				
			typedObj = obj as MediaFileNodeType;

			if (typedObj == null)
			{
				return false;
			}

			return base.Equals(obj) && m_MediaFileType == typedObj.m_MediaFileType
			       && m_AudioDevice == typedObj.AudioDevice
			       && m_VideoDevice == typedObj.VideoDevice;
				
		}
	}
}
