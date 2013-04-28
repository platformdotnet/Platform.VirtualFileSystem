using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class VolumeFaderService
		: AbstractService
	{
		private readonly ISoundFile soundFile;

		public VolumeFaderService(ISoundFile soundFile)
		{
			this.soundFile = soundFile;
		}
	}
}
