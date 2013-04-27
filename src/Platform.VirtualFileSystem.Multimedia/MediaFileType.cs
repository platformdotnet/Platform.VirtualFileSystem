using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class MediaFileType
	{
		private interface IUnknownMediaFileType
		{
		}

		private interface IAutoMediaFileType
		{
		}

		public static readonly MediaFileType Auto = new MediaFileType(typeof(IAutoMediaFileType));
		public static readonly MediaFileType Unknown = new MediaFileType(typeof(IUnknownMediaFileType));
		public static readonly MediaFileType SoundFile = new MediaFileType(typeof(ISoundFile));
		public static readonly MediaFileType VideoFile = new MediaFileType(typeof(IVideoFile));
		
		private Type m_RuntimeType;

		public virtual Type RuntimeType
		{
			get
			{
				return m_RuntimeType;
			}
		}

		protected MediaFileType(Type runtimeType)
		{
			m_RuntimeType = runtimeType;
		}

		public override int GetHashCode()
		{
			return m_RuntimeType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			MediaFileType temp = obj as MediaFileType;

			if (temp == null)
			{
				return false;
			}

			return temp.m_RuntimeType.Equals(m_RuntimeType);
		}
	}
}
