using System;
using System.Collections.Generic;
using System.Text;
using Platform.References;

namespace Platform.Collections
{
	/// <summary>
	/// A <see cref="ReferenceDictionary{K,V,R}"/> that is based on <see cref="WeakReference{T}"/> objects.
	/// The dictionary will automatically shrink when references in the dictionary become unavailable.
	/// </summary>
	/// <remarks>
	/// Because of the non-deterministic nature of the GC, the size of the dictionary will not necessarily
	/// reflect the number of items actually available in the dictionary.  The count is only an approximation.
	/// Items available one moment may be come unavailable the next.
	/// </remarks>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public class WeakReferenceDictionary<K, V>
		: ReferenceDictionary<K, V, WeakReferenceDictionary<K, V>.KeyedWeakReference>
		where V : class
	{
		/// <summary>
		/// A <see cref="WeakReference{T}"/> that contains a key.
		/// </summary>
		public class KeyedWeakReference
			: Platform.References.WeakReference<V>, IKeyed<K>			
		{
			object IKeyed.Key
			{
				get
				{
					return this.Key;
				}
			}

			public virtual K Key
			{
				get;
				set;
			}

			public KeyedWeakReference(K key, V reference)
				: this(key, reference, null)
			{
			}

			public KeyedWeakReference(K key, V reference, IReferenceQueue<V> referenceQueue)
				: base(reference, referenceQueue)
			{
				this.Key = key;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var keyedReference = (KeyedWeakReference)obj;

				if (obj == null)
				{
					return false;
				}

				return obj == this || keyedReference.Key.Equals(this.Key);
			}
		}
		private readonly System.Threading.Timer cleanerTimer;

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> backed with a
		/// <see cref="Dictionary{TKey,TValue}"/>.
		/// </summary>
		/// <remarks>
		/// Dead references will be cleaned out (removed) with every read or write operation to the
		/// dictionary (this is a fast operation based upon <see cref="IReferenceQueue{T}"/>).
		/// </remarks>
		public WeakReferenceDictionary()
			: this(typeof(Dictionary<,>))
		{
		}

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> with the provided type
		/// as the store for the dictionary.
		/// </summary>
		/// <remarks>
		/// Dead references will be cleaned out (removed) with every read or write operation to the
		/// dictionary (this is a fast operation based upon <see cref="IReferenceQueue{T}"/>).
		/// </remarks>
		/// <param name="openGenericDictionaryType">
		/// The type to use as the store for the current dictionary.  The type must implement
		/// <see cref="IDictionary{TKey,TValue}"/>.  The provided type should be the open
		/// (unrealised) generic type.  For example, it should be <c>typeof(Dictionary<,>)</c>
		/// and not <c>typeof(Dictionary<string, string>)</c>
		/// </param>
		/// <param name="constructorArgs">Arguments to pass to the backing dictionary constructor</param>
		public WeakReferenceDictionary(Type openGenericDictionaryType, params object[] constructorArgs)
			: this(TimeSpan.FromMilliseconds(-1), openGenericDictionaryType, constructorArgs)
		{
		}

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> with the provided type
		/// as the store for the dictionary.
		/// </summary>
		/// <remarks>
		/// Dead references will be cleaned out (removed) periodically at intervals specified by <see cref="periodicCleanTimeout"/>.
		/// If <see cref="periodicCleanTimeout"/> is -1 milliseconds then clean out will be performed with 
		/// every read or write operation (this is a fast operation based upon <see cref="IReferenceQueue{T}"/>).
		/// If <see cref="periodicCleanTimeout"/> is specified then clean out will not be performed by
		/// every read or write operation and will only be performed at set intervals determined by the timeout value.
		/// </remarks>
		/// <param name="periodicCleanTimeout">The amount of time</param>
		/// <param name="openGenericDictionaryType">
		/// The type to use as the store for the current dictionary.  The type must implement
		/// <see cref="IDictionary{TKey,TValue}"/>.  The provided type should be the open
		/// (unrealised) generic type.  For example, it should be <c>typeof(Dictionary<,>)</c>
		/// and not <c>typeof(Dictionary<string, string>)</c>
		/// </param>
		/// <param name="constructorArgs">Arguments to pass to the backing dictionary constructor</param>
		public WeakReferenceDictionary(TimeSpan periodicCleanTimeout, Type openGenericDictionaryType, params object[] constructorArgs)
			: base(openGenericDictionaryType, constructorArgs)
		{
			if (periodicCleanTimeout.TotalMilliseconds != -1)
			{
				cleanerTimer = new System.Threading.Timer(OnTimer, null, (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2), (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2));
			}
		}

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> backed with a
		/// <see cref="Dictionary{TKey,TValue}"/>.
		/// </summary>
		/// <remarks>
		/// Dead references will be cleaned out (removed) periodically at intervals specified by <see cref="periodicCleanTimeout"/>.
		/// If <see cref="periodicCleanTimeout"/> is -1 milliseconds then clean out will be performed with 
		/// every read or write operation (this is a fast operation based upon <see cref="IReferenceQueue{T}"/>).
		/// If <see cref="periodicCleanTimeout"/> is specified then clean out will not be performed by
		/// every read or write operation and will only be performed at set intervals determined by the timeout value.
		/// </remarks>
		/// <param name="periodicCleanTimeout">The amount of time</param>
		public WeakReferenceDictionary(TimeSpan periodicCleanTimeout)
			: this(periodicCleanTimeout, typeof(Dictionary<,>))
		{
			if (periodicCleanTimeout.TotalMilliseconds != -1)
			{
				cleanerTimer = new System.Threading.Timer(OnTimer, null, (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2), (int)Math.Round(periodicCleanTimeout.TotalMilliseconds / 2));
			}
		}

		~WeakReferenceDictionary()
		{
			System.Threading.Timer timer = cleanerTimer;

			if (timer != null)
			{
				timer.Dispose();

				timer = null;
			}
		}

		/// <summary>
		/// Called by the periodic clean out timer
		/// </summary>
		protected virtual void OnTimer(object state)
		{
			base.CleanDeadReferences();
		}

		/// <summary>
		/// Overrides <see cref="ReferenceDictionary{K,V,R}.CleanDeadReferences"/> and does not
		/// perform cleaning if a timer has been set.
		/// </summary>
		/// <returns>True if dead references were cleaned out</returns>
		protected override bool CleanDeadReferences()
		{
			if (cleanerTimer != null)
			{
				// Let the timer do the cleaning

				return false;
			}

			return base.CleanDeadReferences();
		}

		/// <summary>
		/// Creates a <see cref="KeyedWeakReference"/> with the supplied key and value.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The value</param>
		/// <returns>A new <see cref="KeyedWeakReference"/></returns>
		protected override KeyedWeakReference CreateReference(K key, V value)
		{
			return new KeyedWeakReference(key, value, referenceQueue);
		}
	}
}
