using System;
using System.Threading;
using Platform;

namespace Platform.VirtualFileSystem.Multimedia
{
	/// <summary>
	/// Summary description for IMediaFile.
	/// </summary>
	public interface IMediaFile
		: IFile, ITask
	{
		/// <summary>
		/// 
		/// </summary>
		event EventHandler Opened;

		/// <summary>
		/// 
		/// </summary>
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

		/// <summary>
		/// 
		/// </summary>
		IMeter PlaybackMeter
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		TimeSpan PlaybackPosition
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		TimeSpan PlaybackLength
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		VolumeLevel VolumeLevel
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		void Open();

		/// <summary>
		/// 
		/// </summary>
		void Close();

		/// <summary>
		/// 
		/// </summary>
		void Play();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		void Seek(TimeSpan position);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		void WaitForPlaybackPosition(TimeSpan position);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		bool WaitForPlaybackPosition(TimeSpan position, int timeout);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="timeout"></param>
		/// <returns></returns>
		bool WaitForPlaybackPosition(TimeSpan position, TimeSpan timeout);

		/// <summary>
		/// 
		/// </summary>
		void WaitForFinish();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		bool WaitForFinish(int timeout);

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <seealso cref="GetWaitHandleForPlaybackPosition(TimeSpan)"/>
		/// </remarks>
		/// <param name="timeout"></param>
		bool WaitForFinish(TimeSpan timeout);

		void FadeToLevel(VolumeLevel level, TimeSpan time);
	}
}
