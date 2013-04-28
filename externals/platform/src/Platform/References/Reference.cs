using System;

namespace Platform.References
{
	/// <summary>
	/// The base class for all reference types.
	/// </summary>
	/// <typeparam name="T">The type of the reference <see cref="Target"/></typeparam>
	public abstract class Reference<T>
		where T : class
	{
		internal Reference<T> nextOnQueue = null;
		private IReferenceQueue<T> referenceQueue = null;

		/// <summary>
		/// Used to monitor when the garbage collector runs.
		/// </summary>
		protected class FinalizerListener
		{
			private readonly Reference<T> reference;

			public FinalizerListener(Reference<T> reference)
			{
				this.reference = reference;
			}

			~FinalizerListener()
			{
				if (reference.Target == null)
				{
					reference.Enqueue();
				}
				else
				{
					if (!Environment.HasShutdownStarted)
					{
						new FinalizerListener(reference);
					}
				}
			}
		}

		/// <summary>
		/// Constructs a new <see cref="Reference{T}"/>
		/// </summary>
		protected Reference()
		{
			referenceQueue = null;
		}

		/// <summary>
		/// Constructs a new <see cref="Reference{T}"/>
		/// </summary>
		/// <param name="referenceQueue">The reference's associated <see cref="IReferenceQueue{T}"/></param>
		protected Reference(IReferenceQueue<T> referenceQueue)
		{
			this.referenceQueue = referenceQueue;
		}

		protected virtual void CreateFinalizerListener()
		{
			if (referenceQueue != null)
			{
				new FinalizerListener(this);
			}
		}

		/// <summary>
		/// Gets the reference target.
		/// </summary>
		public abstract T Target
		{
			get;
		}

		/// <summary>
		/// Clears the target  (sets it to null).
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Places this reference onto its associated <see cref="IReferenceQueue{T}"/>.
		/// </summary>
		/// <returns>
		/// True if the <see cref="Reference{T}"/> was placed on the <see cref="IReferenceQueue{T}"/>
		/// otherwise False if no <see cref="IReferenceQueue{T}"/> is associated or if the
		/// <see cref="Reference{T}"/> has already been enqueued.
		/// </returns>
		protected virtual bool Enqueue()
		{
			if (referenceQueue != null && nextOnQueue == null)
			{
				referenceQueue.Enqueue(this);
				referenceQueue = null;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns True if the current reference has already beened enqueued.
		/// </summary>
		/// <seealso cref="Enqueue()"/>
		protected virtual bool IsEnqueued
		{
			get
			{
				return nextOnQueue != null;
			}
		}
	}
}
