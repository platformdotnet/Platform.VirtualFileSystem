using System;

namespace Platform.VirtualFileSystem.Multimedia
{
	public interface IMediaFile
		: IFile, ITask
	{
		event EventHandler Opened;
		event EventHandler Closed;

		/// <summary>
		/// Gets the <see cref="ITask.TaskState"/> of the task.
		/// </summary>
		/// <remarks>
		/// Unknown		Unknown
		/// Unstarted	Not opened
		/// Stopped		Opened but not Playing
		/// Running		Playing
		/// Paused		Paused
		/// </remarks>
		new TaskState TaskState
		{
			get;
		}

		IMeter PlaybackMeter
		{
			get;
		}

		TimeSpan PlaybackPosition
		{
			get;
			set;
		}

		TimeSpan PlaybackLength
		{
			get;
		}
		
		VolumeLevel VolumeLevel
		{
			get;
			set;
		}

		void Open();
		void Close();
		void Play();
		void Seek(TimeSpan position);
		void WaitForPlaybackPosition(TimeSpan position);
		bool WaitForPlaybackPosition(TimeSpan position, int timeout);
		bool WaitForPlaybackPosition(TimeSpan position, TimeSpan timeout);
		void WaitForFinish();
		bool WaitForFinish(int timeout);
		bool WaitForFinish(TimeSpan timeout);
		void FadeToLevel(VolumeLevel level, TimeSpan time);
	}
}
