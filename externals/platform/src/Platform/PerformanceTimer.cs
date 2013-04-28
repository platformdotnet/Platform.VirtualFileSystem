using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Platform
{
	/// <summary>
	/// Useful utility to timing the performance of certain blocks of code.
	/// Use the performance timer with a using statement.
	/// </summary>
	public class PerformanceTimer
		: IDisposable
	{
		/// <summary>
		/// The name of the timer.
		/// </summary>
		public virtual string Name
		{
			get; set;
		}

		/// <summary>
		/// The writer for the timer.
		/// </summary>
		public virtual TextWriter Writer
		{
			get; set;
		}

		public virtual double TotalMilliseconds
		{
			get
			{
				if (this.endMilliseconds != null)
				{
					return this.endMilliseconds.Value - this.startMilliseconds;
				}
				else
				{
					return clock.CurrentMilliseconds - this.startMilliseconds;
				}
			}
		}

		/// <summary>
		/// The amount of time that has elapsed so far (or in total if the timer has been disposed)
		/// </summary>
		public virtual TimeSpan TimeElapsed
		{
			get
			{
				if (this.endMilliseconds != null)
				{
					return TimeSpan.FromTicks((long)((this.endMilliseconds.Value - this.startMilliseconds) * 10000));
				}
				else
				{
					return TimeSpan.FromTicks((long)((clock.CurrentMilliseconds - this.startMilliseconds) * 10000d));
				}
			}
		}
	
		/// <summary>
		/// Construcs a new performance timer.  By default outputs performance
		/// information to Console.Out in debug mode and to TextWriter.Null in release mode.
		/// </summary>
		public PerformanceTimer()
			: this((string)null)
		{
		}

		/// <summary>
		/// Constructs a new performance timer
		/// </summary>
		/// <param name="writer">The writer to write performance information to</param>
		public PerformanceTimer(TextWriter writer)
			: this(null, writer)
		{
		}

		/// <summary>
		/// Constructs a new performance timer
		/// </summary>
		/// <param name="name">The name of the timer</param>
		public PerformanceTimer(string name)
			: this(name, (TextWriter)null)
		{
		}

		private readonly Clock clock;
		private double? endMilliseconds;
		private readonly double startMilliseconds;
		

		/// <summary>
		/// Constructs a new performance timer
		/// </summary>
		/// <param name="writer">The writer to write performance information to</param>
		/// <param name="name">The name of the timer</param>
		public PerformanceTimer(string name, TextWriter writer)
			: this(writer, name, DateTimeClock.Default)
		{
		}

		public PerformanceTimer(Clock clock)
			: this(null, null, clock)
		{
		}

		public PerformanceTimer(string name, Clock clock)
			: this(null, name, clock)
		{
		}

		/// <summary>
		/// Constructs a new performance timer
		/// </summary>
		/// <param name="writer">The writer to write performance information to</param>
		/// <param name="name">The name of the timer</param>
		public PerformanceTimer(TextWriter writer, string name, Clock clock)
		{
			this.clock = clock;

			if (name == null)
			{
				name = new StackFrame(1).GetMethod().Name;
			}

			this.Name = name;
			this.Writer = writer;
			this.startMilliseconds = clock.CurrentMilliseconds;

			if (this.Writer != null)
			{
				this.Writer.WriteLine("PerformanceTimer({0}).Start", this.Name);
			}
		}

		/// <summary>
		/// Disposes the timer and writes performance information to the timer's writer.
		/// </summary>
		public virtual void Dispose()
		{
			this.endMilliseconds = this.clock.CurrentMilliseconds;

			if (this.Writer != null)
			{
				this.Writer.WriteLine("PerformanceTimer({0}).End: {1}", this.Name, TimeSpan.FromMilliseconds(this.endMilliseconds.Value - this.startMilliseconds));
			}
		}
	}
}
