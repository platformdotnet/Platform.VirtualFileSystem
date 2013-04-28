using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.References
{
	/// <summary>
	/// A <see cref="WeakReference<T>"/> that fires an event after its target has been finalized
	/// </summary>
	/// <typeparam name="T">
	/// The type of the target that the reference points to.
	/// </typeparam>
	public class NotifyingWeakReference<T>
		: WeakReference<T>
		where T : class
	{
		/// <summary>
		/// An event that is raised a reference target is collected.  The event
		/// will be raised on the garbage collector finalizer thread.
		/// </summary>
		public virtual event EventHandler ReferenceCollected;

		protected virtual void OnReferenceCollected(EventArgs eventArgs)
		{
			if (ReferenceCollected != null)
			{
				ReferenceCollected(this, eventArgs);
			}
		}

		private class GarbageCollectionListener
		{
			private readonly NotifyingWeakReference<T> notifyingReference;

			public GarbageCollectionListener(NotifyingWeakReference<T> notifyingReference)
			{
				this.notifyingReference = notifyingReference;
			}

			~GarbageCollectionListener()
			{
				if (notifyingReference.Target == null)
				{
					notifyingReference.OnReferenceCollected(EventArgs.Empty);
				}
				else
				{
					new GarbageCollectionListener(notifyingReference);
				}
			}
		}

		/// <summary>
		/// Constructs a new <see cref="NotifyingWeakReference{T}"/>
		/// </summary>
		/// <param name="value">The reference target</param>
		public NotifyingWeakReference(T value)
			: base(value)
		{
			new GarbageCollectionListener(this);
		}

		/// <summary>
		/// Constructs a new <see cref="NotifyingWeakReference{T}"/>
		/// </summary>
		/// <param name="value">The reference target</param>
		/// <param name="trackResurrection">
		/// True if the current reference should continue to reference the target
		/// after it has been resurrected.
		/// </param>
		public NotifyingWeakReference(T value, bool trackResurrection)
			: base(value, null, trackResurrection)
		{
			new GarbageCollectionListener(this);
		}
	}
}
