using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Caches a value and automatically retrieves the new value
	/// if the cached value is invalidated by a cal to <see cref="Invalidate"/>.
	/// </summary>
	/// <typeparam name="T">The type of value to cache</typeparam>
	public struct CachedValue<T>
		: IValued, IValued<T>
	{
		/// <summary>
		/// The cached value.
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.Value;
			}			
		}

		/// <summary>
		/// The function that gets the current up-to-date value.
		/// </summary>
		public Func<T> ValueGetter
		{
			get; private set;
		}

		/// <summary>
		/// The cached value.
		/// </summary>
		public T Value
		{
			get
			{
				if (gotValue)
				{
					return value;
				}
				else
				{
					lock (typeof(CachedValue<T>))
					{
						if (gotValue)
						{
							return value;
						}
						else
						{
						    var currentValue = this.ValueGetter();
							
							System.Threading.Thread.MemoryBarrier();

                            this.value = currentValue;
							this.gotValue = true;

							return this.value;
						}
					}
				}
			}
		}
		/// <summary>
		/// <see cref="Value"/>
		/// </summary>
		private T value;

		private volatile bool gotValue;
		
		/// <summary>
		/// Constructs a new <see cref="CachedValue{T}"/>.  The object's
		/// value will retrieved at the next call to <see cref="Value"/>
		/// </summary>
		/// <param name="valueGetter">A function that retrieves the value on demand</param>
		public CachedValue(Func<T> valueGetter)
			: this()
		{			
			gotValue = false;
			value = default(T);
			this.ValueGetter = valueGetter;
		}

		/// <summary>
		/// Invalidates the current value.  The next reference to <see cref="Value"/>
		/// will cause a new value to be loaded by the <see cref="ValueGetter"/>
		/// </summary>
		public void Invalidate()
		{
			gotValue = false;
			value = default(T);
		}
	}
}
