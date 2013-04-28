using System;

namespace Platform.References
{
	/// <summary>
	/// A generic <see cref="WeakReference"/> implementation that supports
	/// GC monitoring using <see cref="IReferenceQueue{T}"/>.
	/// </summary>
	public class WeakReference<T>
		: Reference<T>
		where T : class
	{
		private readonly System.WeakReference systemReference;

		/// <summary>
		/// Constructs a new <see cref="WeakReference{T}"/> with the given target.
		/// </summary>
		/// <param name="target"></param>
		public WeakReference(T target)
			: this(target, null)
		{
			
		}

		public WeakReference(T target, IReferenceQueue<T> queue)
			: this(target, queue, false)
		{
			
		}

		public WeakReference(T target, IReferenceQueue<T> queue, bool trackResurrection)
			: base(queue)
		{
			systemReference = new System.WeakReference(target, trackResurrection);

			CreateFinalizerListener();
		}

		/// <summary>
		/// The reference target.
		/// </summary>
		public override T Target
		{
			get
			{
				return (T)systemReference.Target;
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
		/// Clears the target that the <see cref="Reference{T}"/> points to.
		/// </summary>
		public override void Clear()
		{
			systemReference.Target = null;

			Enqueue();
		}
	}
}
