using System;
using System.Threading;

namespace Platform.Threading
{
	/// <summary>
	/// Represents a class that can wait until an <see cref="IMeter"/> attains a certain value.
	/// </summary>
	/// <remarks>
	/// <p>
	/// This class can be used to make a thread enter a sleep state until an
	/// <see cref="IMeter"/> gets to a certain value (or within a range of values).
	/// The required target value can be dynamically specified by passing in a
	/// <see cref="Predicate{T}"/> rather than a scalar value into the class constructor.
	/// </p>
	/// <p>
	/// This class does not extend <see cref="WaitHandle"/> because that would
	/// imply that this class could be passed to the <see cref="System.Threading.WaitHandle.WaitAll(System.Threading.WaitHandle[])"/>
	/// and the <see cref="System.Threading.WaitHandle.WaitAll(System.Threading.WaitHandle[])"/> methods which is not the case.
	/// If that functionality is needed, use the <see cref="MeterValueEvent{V}.WaitHandle"/>
	/// property.
	/// </p>
	/// </remarks>
	public class MeterValueEvent<V>
	{
		/// <summary>
		///  The predicate that checks the current <see cref="IMeter"/> value.
		/// </summary>
		/// <remarks>
		/// This property is read-only and can only be set once using the constructor.
		/// </remarks>
		public virtual Predicate<V> AttainedValue
		{
			get
			{
				return attainedValue;
			}
		}
		/// <summary>
		/// <see cref="AttainedValue"/>
		/// </summary>
		private readonly Predicate<V> attainedValue;

		/// <summary>
		///  The <see cref="IMeter"/> to monitor.
		/// </summary>
		/// <remarks>
		/// This property is read-only and can only be set once by the constructor.
		/// </remarks>
		public virtual IMeter ValueMeter
		{
			get
			{
				return valueMeter;
			}
		}
		/// <summary>
		/// <see cref="ValueMeter"/>
		/// </summary>
		private readonly IMeter valueMeter;

		/// <summary>
		/// Construct a new <see cref="MeterValueEvent{V}"/> based on the specified value.
		/// </summary>
		/// <param name="meter">
		/// The <see cref="IMeter"/> to monitor.
		/// </param>
		/// <param name="value">
		/// The value that will be compared with the current <see cref="IMeter"/> value.
		/// </param>
		public MeterValueEvent(IMeter meter, V value)
			: this(meter, PredicateUtils.ObjectEquals(value))
		{
		}

		/// <summary>
		/// Construct a new <see cref="MeterValueEvent{V}"/> based on the specified predicate.
		/// </summary>
		/// <param name="meter">
		/// The <see cref="IMeter"/> to monitor.
		/// </param>
		/// <param name="attainedValue">
		/// The <see cref="Predicate{T}"/> that will be used to check if the current
		/// <see cref="IMeter"/> value.  The predicate will be passed an object equivalent
		/// to the <see cref="IMeter.CurrentValue"/> property of the supplied <c>meter</c>.
		/// </param>
		public MeterValueEvent(IMeter meter, Predicate<V> attainedValue)
		{
			meter.ValueChanged += Meter_ValueChanged;

			lock (this)
			{
				valueMeter = meter;
				this.attainedValue = attainedValue;

				waitHandle = CreateWaitHandle(this.attainedValue((V)meter.CurrentValue));
			}
		}

		/// <summary>
		/// Callback when the <see cref="IMeter"/> value changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void Meter_ValueChanged(object sender, MeterEventArgs eventArgs)
		{
			lock (this)
			{
				if (attainedValue((V)eventArgs.NewValue))
				{
					Set();
				}
				else
				{
					Reset();
				}
			}
		}

		/// <summary>
		/// Convience method.  Equivalent to calling WaitOne on <see cref="MeterValueEvent{V}.WaitHandle"/>
		/// </summary>
		public virtual bool WaitOne()
		{
			return WaitOne(Timeout.Infinite, true);
		}

		/// <summary>
		/// Convience method.  Equivalent to calling WaitOne on <see cref="MeterValueEvent{V}.WaitHandle"/>
		/// </summary>
		public virtual bool WaitOne(int timeout, bool exitContext)
		{
			return waitHandle.WaitOne(TimeSpan.FromMilliseconds(timeout), exitContext);
		}

		/// <summary>
		/// Convience method.  Equivalent to calling WaitOne on <see cref="MeterValueEvent{V}.WaitHandle"/>
		/// </summary>
		public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			return waitHandle.WaitOne(timeout, exitContext);
		}

		protected virtual WaitHandle CreateWaitHandle(bool initialState)
		{
			return new ManualResetEvent(initialState);
		}

		protected virtual void Set()
		{
			((ManualResetEvent)this.WaitHandle).Set();
		}

		protected virtual void Reset()
		{
			((ManualResetEvent)this.WaitHandle).Reset();
		}

		/// <summary>
		///  The <see cref="WaitHandle"/> that will be set when the <see cref="IMeter"/> attains the required value.
		/// </summary>
		public virtual WaitHandle WaitHandle
		{
			get
			{
				return waitHandle;
			}
		}
		/// <summary>
		/// <see cref="WaitHandle"/>
		/// </summary>
		private readonly WaitHandle waitHandle;
	}
}
