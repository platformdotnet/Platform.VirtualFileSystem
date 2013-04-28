using System;
using System.Collections.Generic;
using System.Text;
using Platform.References;

namespace Platform.Collections
{
	/// <summary>
	/// A <see cref="ReferenceDictionary{K,V,R}"/> that is based on <see cref="WeakReference{T}"/> objects.
	/// The dictionary will automatically shrink when references in the dictionary become unavailable.
	/// Unlike a <see cref="WeakReferenceDictionary{K,V}"/>, items within the <see cref="TimedReferenceDictionary{K,V}"/>
	/// will be guaranteed to be uncollected (will be hard-referenced) for a minimum amount of time that can be set
	/// with a constructor argument.
	/// </summary>
	/// <remarks>
	/// Because of the non-deterministic nature of the GC, the size of the dictionary will not necessarily
	/// reflect the number of items actually available in the dictionary.  The count is only an approximation.
	/// Items available one moment may be come unavailable the next.
	/// </remarks>
	/// <typeparam name="K">The key type</typeparam>
	/// <typeparam name="V">The value type</typeparam>
	public class TimedReferenceDictionary<K, V>
		: ReferenceDictionary<K, V, TimedReferenceDictionary<K, V>.KeyedTimedReference>
		where V : class
	{
		public class KeyedTimedReference
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

			public virtual DateTime LastAccess
			{
				get;
				set;
			}

			protected virtual TimeSpan TimeOut
			{
				get;
				set;
			}

			public virtual V HardReference
			{
				get;
				set;
			}

			public KeyedTimedReference(K key, V reference, IReferenceQueue<V> referenceQueue, TimeSpan timeout)
				: base(reference, referenceQueue)
			{
				this.Key = key;
				this.HardReference = reference;
				this.LastAccess = DateTime.Now;
				this.TimeOut = timeout;
			}

			public override int GetHashCode()
			{
				return Key.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var keyedReference = (KeyedTimedReference)obj;

				if (obj == null)
				{
					return false;
				}

				if (obj == this)
				{
					return true;
				}

				return keyedReference.Key.Equals(this.Key);
			}
		}

		public virtual TimeSpan TimeOut
		{
			get;
			protected set;
		}

		private System.Threading.Timer timer;
		private readonly int maximumCount = -1;
		private volatile bool cleanOnNext = false;

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> backed by a <see cref="Dictionary{K, V}"/>.
		/// </summary>
		/// <param name="timeout">The amount of time each item in the dictionary will be guaranteed to be uncollected for</param>
		public TimedReferenceDictionary(TimeSpan timeout)
			: this(timeout, typeof(Dictionary<,>))
		{
		}

		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> with the provided type
		/// as the store for the dictionary.
		/// </summary>
		/// <param name="timeout">The amount of time each item in the dictionary will be guaranteed to be uncollected for</param>
		/// <param name="openGenericDictionaryType">
		/// The type to use as the store for the current dictionary.  The type must implement
		/// <see cref="IDictionary{TKey,TValue}"/>.  The provided type should be the open
		/// (unrealised) generic type.  For example, it should be <c>typeof(Dictionary<,>)</c>
		/// and not <c>typeof(Dictionary<string, string>)</c>
		/// </param>
		/// <param name="constructorArgs">Arguments to pass to the backing dictionary constructor</param>
		public TimedReferenceDictionary(TimeSpan timeout, Type openGenericDictionaryType, params object[] constructorArgs)
			: this(timeout, -1, openGenericDictionaryType, constructorArgs)
		{
		}
		
		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> backed by a <see cref="Dictionary{K, V}"/>.
		/// </summary>
		/// <param name="timeout">
		/// The amount of time each item in the dictionary will be guaranteed to be uncollected for
		/// </param>
		/// <param name="maximumCount">
		/// The maximum number of items allowed in the dictionary.  Pass -1 for no limit.
		/// </param>
		public TimedReferenceDictionary(TimeSpan timeout, int maximumCount)
			: this(timeout, maximumCount, typeof(Dictionary<,>))
		{
		}
        
		/// <summary>
		/// Creates a new <see cref="WeakReferenceDictionary{K,V}"/> with the provided type
		/// as the store for the dictionary.
		/// </summary>
		/// <param name="timeout">
		/// The amount of time each item in the dictionary will be guaranteed to be uncollected for
		/// </param>
		/// <param name="maximumCount">
		/// The maximum number of items allowed in the dictionary.  Pass -1 for no limit.
		/// </param>
		/// <param name="openGenericDictionaryType">
		/// The type to use as the store for the current dictionary.  The type must implement
		/// <see cref="IDictionary{TKey,TValue}"/>.  The provided type should be the open
		/// (unrealised) generic type.  For example, it should be <c>typeof(Dictionary<,>)</c>
		/// and not <c>typeof(Dictionary<string, string>)</c>
		/// </param>
		/// <param name="constructorArgs">Arguments to pass to the backing dictionary constructor</param>
		public TimedReferenceDictionary(TimeSpan timeout, int maximumCount, Type openGenericDictionaryType, params object[] constructorArgs)
			: base(openGenericDictionaryType, constructorArgs)
		{
			this.TimeOut = timeout;
			
			if (maximumCount != -1)
			{
				this.maximumCount = Math.Max(maximumCount, 16);
			}
			else
			{
				this.maximumCount = -1;
			}

			timer = new System.Threading.Timer(OnTimer, null, (int)Math.Round(timeout.TotalMilliseconds / 2), (int)Math.Round(timeout.TotalMilliseconds/ 2));
		}

		~TimedReferenceDictionary()
		{
			var localTimer = this.timer;

			if (localTimer != null)
			{
				localTimer.Dispose();

				this.timer = null;
			}
		}

		/// <summary>
		/// Tries to get an item from the <see cref="TimedReferenceDictionary{K,V}"/>.
		/// </summary>
		/// <param name="key">The key of the value</param>
		/// <param name="value">A variable in which to place the item</param>
		/// <returns>True if the item was found and returned otherwise False</returns>
		public override bool TryGetValue(K key, out V value)
		{
			lock (this.SyncLock)
			{
				KeyedTimedReference reference;

				if (!dictionary.TryGetValue(key, out reference))
				{
					value = null;

					return false;
				}

				if (DateTime.Now - reference.LastAccess > this.TimeOut)
				{
					reference.HardReference = null;
				}
				
				value = reference.Target;

				if (value != null)
				{
					reference.LastAccess = DateTime.Now;
				}
				
				return value != null;
			}
		}
        
		/// <summary>
		/// Raised when the timer which turns items into WeakReferences when their
		/// last access time has exceeded the dictionary's <see cref="TimeOut"/> period.
		/// </summary>
		/// <param name="state">Will always be null</param>
		protected virtual void OnTimer(object state)
		{
			lock (this.SyncLock)
			{
				CheckMaximumCount();

				GC.Collect();
			
				foreach (var reference in dictionary.Values)
				{
					if (DateTime.Now - reference.LastAccess > this.TimeOut)
					{
						reference.HardReference = null;

						cleanOnNext = true;
					}
				}

				GC.Collect();
				GC.WaitForPendingFinalizers();

				base.CleanDeadReferences();
			}
		}
		
		protected override bool CleanDeadReferences()
		{
			// In general do nothing.  Let the timer do the cleaning.

			if (cleanOnNext)
			{
				if (base.CleanDeadReferences())
				{
					cleanOnNext = false;

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Adds an item to the dictionary.
		/// </summary>
		/// <param name="key">The key for the item</param>
		/// <param name="value">The value iof the item</param>
		public override void Add(K key, V value)
		{
			CheckMaximumCount();

			base.Add(key, value);
		}

		/// <summary>
		/// Gets or sets an item by key
		/// </summary>
		/// <param name="key">The key of the item to get or set</param>
		/// <returns>The item</returns>
		public override V this[K key]
		{
			get
			{
				return base[key];
			}
			set
			{
				CheckMaximumCount();

				base[key] = value;
			}
		}
		private int lastMaximumFlushCount = 0;

		/// <summary>
		/// Makes sure the dictionary doesn't grow larger than specified by removing items
		/// that have been least accessed.
		/// </summary>
		private void CheckMaximumCount()
		{
			lock (this.SyncLock)
			{
				if (maximumCount != -1 && dictionary.Count >= maximumCount + lastMaximumFlushCount)
				{
					var target = (int)Math.Round(Math.Min(this.Count, maximumCount) / 2.0);

					var removeCount = this.Count - target;

					if (removeCount > 0)
					{
						int originalCount = this.Count;
						int smallestIndex = -1;
						DateTime smallestTime = DateTime.MaxValue;
						ILList<TimedReferenceDictionary<K, V>.KeyedTimedReference> removeList;

						Action routine = delegate
						{
							removeList = new ArrayList<TimedReferenceDictionary<K, V>.KeyedTimedReference>(removeCount);

							foreach (var reference in dictionary.Values)
							{
								if (removeList.Count < removeCount
									|| (removeList.Count >= removeCount && reference.LastAccess < smallestTime))
								{
									if (removeList.Count < removeCount)
									{
										removeList.Add(reference);

										if (reference.LastAccess < smallestTime)
										{
											smallestIndex = removeList.Count - 1;
											smallestTime = reference.LastAccess;
										}

										continue;
									}

									removeList[smallestIndex] = reference;

									if (reference.LastAccess < smallestTime)
									{
										smallestTime = reference.LastAccess;
									}
								}
							}

							foreach (var item in removeList)
							{
								item.HardReference = null;
							}

							removeList.Clear();
						};

						routine();

						GC.Collect();
						GC.WaitForPendingFinalizers();

						base.CleanDeadReferences();

						lastMaximumFlushCount = Math.Max(this.Count - target, 0) * 2;
					}
				}
			}
		}

		/// <summary>
		/// Creates a <see cref="TimedReferenceDictionary{K, V}"/> with the supplied key and value.
		/// </summary>
		/// <param name="key">The key</param>
		/// <param name="value">The value</param>
		/// <returns>A new <see cref="TimedReferenceDictionary{K, V}"/></returns>
		protected override TimedReferenceDictionary<K, V>.KeyedTimedReference CreateReference(K key, V value)
		{
			return new KeyedTimedReference(key, value, referenceQueue, this.TimeOut);
		}
	}
}
