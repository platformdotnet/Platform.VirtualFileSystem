using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using Platform;

namespace Platform.VirtualFileSystem.Multimedia
{
	public class ValueFaderEventArgs
		: EventArgs
	{
		/// <summary>
		///  OldValue
		/// </summary>
		public virtual object OldValue
		{
			get
			{
				return m_OldValue;
			}
			set
			{
				m_OldValue = value;
			}
		}
		/// <summary>
		/// <see cref="OldValue"/>
		/// </summary>
		private object m_OldValue;

		/// <summary>
		///  NewValue
		/// </summary>
		public virtual object NewValue
		{
			get
			{
				return m_NewValue;
			}
			set
			{
				m_NewValue = value;
			}
		}
		/// <summary>
		/// <see cref="NewValue"/>
		/// </summary>
		private object m_NewValue;

		public ValueFaderEventArgs(object oldValue, object newValue)
		{
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}
	}

	public delegate void ValueFaderEventHandler(object sender, ValueFaderEventArgs eventArgs);

	public delegate object ValueDisplacer(object value, object target, double percent);

	public class ValueFader
	{
		private Timer m_Timer;
		private object m_CurrentValue;

		public virtual event ValueFaderEventHandler NewValue;

		/// <summary>
		///  ValueDisplacer
		/// </summary>
		public virtual ValueDisplacer ValueDisplacer
		{
			get
			{
				return m_ValueDisplacer;
			}
			set
			{
				m_ValueDisplacer = value;
			}
		}
		/// <summary>
		/// <see cref="ValueDisplacer"/>
		/// </summary>
		private ValueDisplacer m_ValueDisplacer;

		/// <summary>
		///  FadeTime
		/// </summary>
		public virtual TimeSpan FadeTime
		{
			get
			{
				return m_FadeTime;
			}
			set
			{
				lock (this)
				{
					m_FadeTime = value;
				}
			}
		}
		/// <summary>
		/// <see cref="FadeTime"/>
		/// </summary>
		private TimeSpan m_FadeTime;
		
		public ValueFader(TimeSpan fadeTime, ValueDisplacer valueDisplacer)
			: this(fadeTime, valueDisplacer, TimeSpan.FromMilliseconds(25))
		{
			
		}

		public ValueFader(TimeSpan fadeTime, ValueDisplacer valueDisplacer, TimeSpan resolution)
		{
			m_FadeTime = fadeTime;
			m_ValueDisplacer = valueDisplacer;
			m_Timer = new Timer(new TimerCallback(OnTimer), null, resolution, resolution);	
		}

		protected virtual void OnTimer(object state)
		{
			TimeSpan diff;
			object oldValue;
			double diffPercent;
			
			lock (this)
			{
				if (m_TargetValue == null)
				{
					return;
				}

				diff = m_TargetDateTime - DateTime.Now;

				if (diff.TotalMilliseconds < 0)
				{
					diffPercent = 100.0;	
				}
				else
				{
					diffPercent = diff.TotalMilliseconds / m_FadeTime.TotalMilliseconds * 100.0;
				}

				oldValue = m_CurrentValue;

				m_CurrentValue = ValueDisplacer(m_CurrentValue, m_TargetValue, diffPercent);

				OnNewValue(new ValueFaderEventArgs(oldValue, m_CurrentValue));
			}
		}

		protected virtual void OnNewValue(ValueFaderEventArgs eventArgs)
		{
			if (NewValue != null)
			{
				NewValue(this, eventArgs);
			}
		}

		/// <summary>
		///  TargetValue
		/// </summary>
		public virtual object TargetValue
		{
			get
			{
				return m_TargetValue;
			}
			set
			{				
				if (value != m_TargetValue)
				{
					m_TargetDateTime = DateTime.Now.Add(m_FadeTime);
				}

				m_TargetValue = value;
			}
		}
		/// <summary>
		/// <see cref="TargetValue"/>
		/// </summary>
		private object m_TargetValue;

		private DateTime m_TargetDateTime;
	}

	public class VolumeFader
	{
		//private Pair m_MaxVolume;
		private IMediaFile m_MediaFile;
		private TimeSpan m_FadeOnStart;
		private TimeSpan m_FadeOnEnd;
		private ValueFader m_ValueFader;
		
		private static IList m_VolumeFaders = new ArrayList();
		
		public static void AttachTo(IMediaFile mediaFile, TimeSpan fadeOnStart, TimeSpan fadeOnEnd, TimeSpan fadeOnPause, TimeSpan fadeOnResume)
		{
			Pair<WeakReference, WeakReference> pair;
			VolumeFader fader;

			fader = new VolumeFader(mediaFile, fadeOnStart, fadeOnEnd, fadeOnPause, fadeOnResume);

			pair = new Pair<WeakReference, WeakReference>(new WeakReference(fader), new WeakReference(mediaFile));

			for (int i = 0; i < m_VolumeFaders.Count; i++)
			{
				Pair<WeakReference, WeakReference> pair2;

				pair2 = (Pair<WeakReference, WeakReference>)m_VolumeFaders[i];

				if (((WeakReference)pair2.Left).Target == null
					|| ((WeakReference)pair2.Right).Target == null)
				{
					m_VolumeFaders[i] = pair;

					return;
				}
			}

			m_VolumeFaders.Add(pair);
		}

		public static void DetachFrom(IMediaFile mediaFile)
		{
			
		}

		public VolumeFader(IMediaFile mediaFile, TimeSpan fadeOnAll)
			: this(mediaFile, fadeOnAll, fadeOnAll, fadeOnAll, fadeOnAll)
		{
			
		}

		public VolumeFader(IMediaFile mediaFile, TimeSpan fadeOnStart, TimeSpan fadeOnEnd, TimeSpan fadeOnPause, TimeSpan fadeOnResume)
		{
			m_MediaFile = mediaFile;
			m_FadeOnStart = fadeOnStart;
			m_FadeOnEnd = fadeOnEnd;

			m_MediaFile.Progress.ValueChanged += MediaFile_Progress_ValueChanged;
			m_MediaFile.TaskStateChanged += new TaskEventHandler(MediaFile_TaskStateChanged);

			m_ValueFader = new ValueFader(TimeSpan.Zero, new ValueDisplacer(ValueDisplacer));

			m_ValueFader.NewValue += delegate(object sender, ValueFaderEventArgs eventArgs)
			{
				m_MediaFile.VolumeLevel = (VolumeLevel)eventArgs.NewValue;
			};
		}

		private object ValueDisplacer(object value, object targetValue, double percent)
		{
			VolumeLevel v1, v2, retval;

			if (value == null)
			{
				return m_MediaFile.VolumeLevel;
			}

			v1 = (VolumeLevel)value;
			v2 = (VolumeLevel)targetValue;

			retval = new VolumeLevel();

			if ((int)v2.Left > (int)v1.Left)
			{
				retval.Left = (int)v1.Left + (int)((int)v1.Left * (percent / 100));
			}
			else
			{
				retval.Left = (int)v1.Left - (int)((int)v1.Left * (percent / 100));
			}

			if ((int)v2.Right > (int)v1.Right)
			{
				retval.Right = (int)v1.Right + (int)((int)v1.Right * (percent / 100));
			}
			else
			{
				retval.Right = (int)v1.Right - (int)((int)v1.Right * (percent / 100));
			}

			return retval;
		}

		protected virtual void MediaFile_Progress_ValueChanged(object sender, MeterEventArgs eventArgs)
		{
			if (((TimeSpan)eventArgs.NewValue) >= m_MediaFile.PlaybackLength - m_FadeOnEnd)
			{
				lock (m_ValueFader)
				{
					m_ValueFader.FadeTime = m_FadeOnEnd;
					m_ValueFader.TargetValue = new VolumeLevel(0, 0);
				}
			}
		}

		protected virtual void MediaFile_TaskStateChanged(object sender, TaskEventArgs eventArgs)
		{
			if (eventArgs.NewState == TaskState.Running)
			{
				lock (this)
				{
					lock (m_ValueFader)
					{
						m_ValueFader.FadeTime = m_FadeOnStart;
						//m_ValueFader.TargetValue = m_MaxVolume;
					}
				}
			}
		}
	}
}
