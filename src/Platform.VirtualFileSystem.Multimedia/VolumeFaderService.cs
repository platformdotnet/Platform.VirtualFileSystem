using System;
using Platform.VirtualFileSystem.Providers;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Summary description for VolumeFaderService.
	/// </summary>
	public class VolumeFaderService
		: AbstractService
	{
		private readonly ISoundFile m_SoundFile;

		public VolumeFaderService(ISoundFile soundFile)
		{
			m_SoundFile = soundFile;
		}
	}
}
