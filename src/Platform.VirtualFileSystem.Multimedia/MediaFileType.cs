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
		
		private readonly Type runtimeType;

		public virtual Type RuntimeType
		{
			get
			{
				return this.runtimeType;
			}
		}

		protected MediaFileType(Type runtimeType)
		{
			this.runtimeType = runtimeType;
		}

		public override int GetHashCode()
		{
			return this.runtimeType.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			var temp = obj as MediaFileType;

			if (temp == null)
			{
				return false;
			}

			return temp.runtimeType == this.runtimeType;
		}
	}
}
