using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Platform.IO
{
	/// <summary>
	/// A <see cref="StreamWrapper"/> that is able to monitor (meter) traffic
	/// as it is sent and received. 
	/// </summary>
	public class MeteringStream
		: StreamWrapper, IStreamWithEvents
	{
		public virtual event EventHandler BeforeClose;

		public virtual void OnBeforeClose(EventArgs eventArgs)
		{
			if (BeforeClose != null)
			{
				BeforeClose(this, eventArgs);
			}
		}

		public virtual event EventHandler AfterClose;

		public virtual void OnAfterClose(EventArgs eventArgs)
		{
			if (AfterClose != null)
			{
				AfterClose(this, eventArgs);
			}
		}
	
	
		public virtual IMeter ReadMeter
		{
			get
			{
				if (readMeter == null)
				{
					throw new NotSupportedException();
				}

				return readMeter;
			}
		}
		private readonly MutableMeter readMeter;

		private long readBytes = 0;

		public virtual IMeter WriteMeter
		{
			get
			{
				if (writeMeter == null)
				{
					throw new NotSupportedException();
				}

				return writeMeter;
			}
		}
		private readonly MutableMeter writeMeter;

		private long writeBytes = 0;
		
		/// <summary>
		/// Constructs a new <see cref="MeteringStream"/> that will meter the given
		/// stream.
		/// </summary>
		/// <param name="stream">The inner stream this meter will delegate to</param>
		public MeteringStream(Stream stream)
			: base(stream)
		{
			if (this.CanRead)
			{
				readMeter = new MutableMeter();
			}

			if (this.CanWrite)
			{
				writeMeter = new MutableMeter();
			}

			if (readMeter != null)
			{
				readMeter.SetMinimumValue(0L);
				readMeter.SetCurrentValue(0L);
			}

			if (writeMeter != null)
			{
				writeMeter.SetCurrentValue(0L);
				writeMeter.SetMinimumValue(0L);
			}

			if (this.CanSeek)
			{
				if (readMeter != null)
				{
					readMeter.SetMaximumValue(this.Length);
				}

				if (writeMeter != null)
				{
					writeMeter.SetMaximumValue(this.Length);
				}
			}
			else
			{
				if (readMeter != null)
				{
					readMeter.SetMaximumValue(Int64.MaxValue);
				}

				if (writeMeter != null)
				{
					writeMeter.SetMaximumValue(Int64.MaxValue);
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int retval;
			
			retval = base.Read(buffer, offset, count);

			if (retval != 0 && readMeter != null)
			{
				readBytes += retval;
				readMeter.SetCurrentValue(readBytes);
			}

			return retval;
		}

		public override int ReadByte()
		{
			var retval = base.ReadByte();

			if (retval != -1 && readMeter != null)
			{
				readBytes++;
				readMeter.SetCurrentValue(readBytes);
			}

			return retval;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			base.Write(buffer, offset, count);

			if (writeMeter != null)
			{
				writeBytes += count;
				writeMeter.SetCurrentValue(writeBytes);
			}
		}

		public override void WriteByte(byte value)
		{
			base.WriteByte(value);

			if (writeMeter != null)
			{
				writeBytes++;
				writeMeter.SetCurrentValue(writeBytes);
			}
		}

		private bool alreadyClosed = false;

		public override void Close()
		{
			try
			{
				lock (this)
				{
					if (!alreadyClosed)
					{
						OnBeforeClose(EventArgs.Empty);
					}
				}

				base.Close();
			}
			finally
			{
				lock (this)
				{
					if (!alreadyClosed)
					{
						OnAfterClose(EventArgs.Empty);

						alreadyClosed = true;
					}
				}
			}
		}
	}
}
