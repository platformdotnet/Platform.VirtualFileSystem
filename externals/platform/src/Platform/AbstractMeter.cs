using System;

namespace Platform
{
	/// <summary>
	/// Base class for <see cref="IMeter"/> implementations
	/// </summary>
	public abstract class AbstractMeter
		: AbstractModel, IMeter
	{
		#region NullMeter

		private class NullMeter
			: AbstractMeter
		{
			public override object CurrentValue
			{
				get
				{
					return null;
				}
			}

			public override object MaximumValue
			{
				get
				{
					return null;
				}
			}

			public override object MinimumValue
			{
				get
				{
					return null;
				}
			}

			public override string Units
			{
				get
				{
					return "";
				}
			}
		}
				
		#endregion

		/// <summary>
		/// An <see cref="IMeter"/> implementation that does nothing.
		/// </summary>
		public static IMeter Null
		{
			get
			{
				return nullMeter;
			}
		}
		private static readonly IMeter nullMeter = new NullMeter();

		/// <summary>
		/// An event that is raised when the meter's value changes.
		/// </summary>
		public virtual event EventHandler<MeterEventArgs> ValueChanged;
		
		/// <summary>
		/// Raises the <see cref="ValueChanged"/> event.
		/// </summary>
		/// <param name="eventArgs">The arguments for the event</param>
        protected virtual void OnValueChanged(MeterEventArgs eventArgs)
		{
			if (this.ValueChanged != null)
			{
				this.ValueChanged(this, eventArgs);
			}
		}

		/// <summary>
		/// Raises the <see cref="ValueChanged"/> event.
		/// </summary>
		/// <param name="newValue">The new (current) value</param>
		/// <param name="oldValue">The old (previous) value</param>
		protected virtual void OnValueChanged(object oldValue, object newValue)
		{
			this.previousValue = oldValue;

			OnValueChanged((new MeterEventArgs(newValue, oldValue)));
		}

		/// <summary>
		/// Raises the <see cref="ValueChanged"/> event.  The previous value
		/// is automatically retrieved from <see cref="PreviousValue"/>.
		/// </summary>
		/// <param name="newValue">The newValue</param>
		protected virtual void OnValueChanged(object newValue)
		{
			OnValueChanged(previousValue, newValue);

            this.previousValue = newValue;
		}

		/// <summary>
		/// Raises the <see cref="ValueChanged"/> event.  The current value
		/// is retrieved form the <see cref="CurrentValue"/> property and the
		/// previous value is automatically retrieved from <see cref="PreviousValue"/>.
		/// </summary>
		protected virtual void OnValueChanged()
		{
			OnValueChanged(this.CurrentValue);

			this.previousValue = this.CurrentValue;
		}

		/// <summary>
		/// Gets the previous value of the meter.
		/// </summary>
		public virtual object PreviousValue
		{
			get
			{
				return previousValue;
			}
		}
		private object previousValue;

		/// <summary>
		/// Gets the maximum value.
		/// </summary>
		public abstract object MaximumValue
		{
			get;
		}

		/// <summary>
		/// Gets the minimum value.
		/// </summary>
		public abstract object MinimumValue
		{
			get;
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		public abstract object CurrentValue
		{
			get;
		}

		/// <summary>
		/// Gets the units for this meter.
		/// </summary>
		public abstract string Units
		{
			get;
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.CurrentValue;
			}
		}

		/// <summary>
		/// Gets the maximum value.
		/// </summary>
		object IMeter.MaximumValue
		{
			get
			{
				return this.MaximumValue;
			}
		}

		/// <summary>
		/// Gets the minimum value.
		/// </summary>
		object IMeter.MinimumValue
		{
			get
			{
				return this.MinimumValue;
			}
		}

		/// <summary>
		/// Gets the current value.
		/// </summary>
		object IMeter.CurrentValue
		{
			get
			{
				return this.CurrentValue;
			}
		}

		/// <summary>
		/// Gets how far along the current value is between the minimum
		/// value and maximum value.
		/// </summary>
		public virtual double Percentage
		{
			get
			{
				double x, y, z;

				if (CurrentValue is TimeSpan)
				{
					x = ((TimeSpan)CurrentValue).TotalMilliseconds;
					y = ((TimeSpan)MaximumValue).TotalMilliseconds;
					z = ((TimeSpan)MinimumValue).TotalMilliseconds;
				}
				else
				{
					x = Convert.ToDouble(CurrentValue);
					y = Convert.ToDouble(MaximumValue);
					z = Convert.ToDouble(MinimumValue);
				}

				if (y == 0)
				{
					return 0;
				}

				return x / (y - z);
			}
		}
        
		/// <summary>
		/// Converts this meter to a string.  The format is
		/// "CurrentValue/MaximumValue (Percent%)"
		/// </summary>
		/// <returns>A string representation of this object</returns>
		public override string ToString()
		{
			string formatString;
			object[] arguments;
									
			arguments = new object[]
				{
					this.CurrentValue,
					this.MaximumValue,
					this.Units,
					0
				};

			if (this.CurrentValue == null || this.MaximumValue == null)
			{
				formatString = "";
			}
			else if (this.Units == "")
			{
				formatString = "{0}/{1}";
			}
			else
			{
				formatString = "{0}/{1} {2}";
			}

			try
			{
				arguments[3] = Percentage * 100;

				if (formatString.Length > 0)
				{
					formatString += " ";
				}

				formatString += "({3:0}%)";
			}
			catch (InvalidCastException)
			{				
			}

			return String.Format(formatString, arguments);
		}
	}
}
