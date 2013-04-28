using System;
using System.Threading;

namespace Platform.References
{
	/// <summary>
	/// A <see cref="Reference{T}"/> that acts like a <see cref="WeakReference{T}"/>
	/// but does not allow the <see cref="Target"/> to be collected unless the 
	/// <see cref="Target"/> has not been accessed for a set amount of time.
	/// This class is fairly heavyweight compared to a normal reference.  Performance
	/// will suffer if you create too many.
	/// </summary>
	public class TimedReference<T>
		: Reference<T>
		where T : class
	{
		protected Timer timer;
		private T hardReference;
		private TimeSpan timeOut;		
		private DateTime targetTime;		
		private readonly WeakReference systemReference;

		/// <summary>
		/// Constructs a new <see cref="TimedReference{T}"/>
		/// </summary>
		/// <param name="target">The reference target</param>
		/// <param name="timeout">
		/// The minimum amount of time the reference target should be uncollectable for
		/// after each access to the <see cref="Target"/> property.
		/// </param>
		public TimedReference(T target, TimeSpan timeout)
			: this(target, timeout, null)
		{
			
		}

		/// <summary>
		/// Constructs a new <see cref="TimedReference{T}"/>
		/// </summary>
		/// <param name="target">The reference target</param>
		/// <param name="timeout">
		/// The minimum amount of time the reference target should be uncollectable for
		/// after each access to the <see cref="Target"/> property.
		/// </param>
		/// <param name="queue">The references associated <see cref="IReferenceQueue{T}"/></param>
		public TimedReference(T target, TimeSpan timeout, IReferenceQueue<T> queue)
			: this(target, timeout, queue, false)
		{
			
		}

		/// <summary>
		/// Constructs a new <see cref="TimedReference{T}"/>
		/// </summary>
		/// <param name="target">The reference target</param>
		/// <param name="timeout">
		/// The minimum amount of time the reference target should be uncollectable for
		/// after each access to the <see cref="Target"/> property.
		/// </param>
		/// <param name="queue">The references associated <see cref="IReferenceQueue{T}"/></param>
		/// <param name="trackResurrection">
		/// True if the current reference should continue to reference the <see cref="Target"/>
		/// after it has been resurrected.
		/// </param>
		public TimedReference(T target, TimeSpan timeout, IReferenceQueue<T> queue, bool trackResurrection)
			: base(queue)
		{
			hardReference = target;
			timeOut = timeout;
			targetTime = DateTime.Now + timeout;
			systemReference = new System.WeakReference(target, trackResurrection);

			CreateTimer();
			CreateFinalizerListener();
		}

		protected virtual void CreateTimer()
		{
			lock (this)
			{
				timer = new Timer
					(
						delegate { Timedout(); },
						null,
						TimeSpan.FromMilliseconds(timeOut.TotalMilliseconds / 2),
						TimeSpan.FromMilliseconds(timeOut.TotalMilliseconds / 2)
					);
			}
		}

		protected virtual void ResetTimer()
		{
			lock (this)
			{
				if (timer == null)
				{
					CreateTimer();
				}
				else
				{
					timer.Change(TimeSpan.FromMilliseconds(timeOut.TotalMilliseconds / 2),
						TimeSpan.FromMilliseconds(timeOut.TotalMilliseconds / 2));
				}
			}
		}

		/// <summary>
		/// <see cref="Reference{T}.Enqueue"/>
		/// </summary>
		protected override bool Enqueue()
		{
			DeleteTimer();

			return base.Enqueue();
		}

		protected virtual void DeleteTimer()
		{
			lock (this)
			{
				if (timer != null)
				{
					ActionUtils.IgnoreExceptions(timer.Dispose);

					timer = null;
				}
			}
		}

		protected virtual void Timedout()
		{
			lock (this)
			{
				if (DateTime.Now >= targetTime)
				{
					hardReference = null;

					DeleteTimer();
				}
			}
		}

		/// <summary>
		/// <see cref="WeakReference{T}.Target"/>
		/// </summary>
		public override T Target
		{
			get
			{
				try
				{
					if (hardReference != null)
					{
						return hardReference;
					}

					T value;

					lock (this)
					{
						value = (T)systemReference.Target;

						if (value == null)
						{
							return null;	
						}
						else
						{
							hardReference = value;

							targetTime = DateTime.Now + timeOut;

							ResetTimer();
						}
					}

					return value;
				}
				catch (InvalidOperationException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets weather the reference continues to reference the target
		/// if it has been resurrected.
		/// </summary>
		public virtual bool TackResurrection
		{
			get
			{
				return systemReference.TrackResurrection;
			}
		}

		/// <summary>
		/// <see cref="Reference{T}.Clear()"/>
		/// </summary>
		public override void Clear()
		{
			systemReference.Target = null;
		}
	}
}
