using System;
using System.IO;
using System.Threading;

namespace Platform.IO
{
	/// <summary>
	/// A class that supports copying from one stream into another stream.
	/// </summary>
	public class StreamPump
		: AbstractTask
	{
		public static readonly int DefaultBufferSize = 8192 * 8;

		#region Types

		public interface IStreamProvider
		{
			Stream GetSourceStream();
			Stream GetDestinationStream();

			long GetSourceLength();
			long GetDestinationLength();
		}

		public class StaticStreamProvider
			: IStreamProvider
		{
			private readonly Stream source;
			private readonly Stream destination;

			public StaticStreamProvider(Stream source, Stream destination)
			{
				this.source = source;
				this.destination = destination;
			}

			public virtual Stream GetSourceStream()
			{
				return source;
			}

			public virtual Stream GetDestinationStream()
			{
				return destination;
			}

			public virtual long GetSourceLength()
			{
				if (source.CanSeek)
				{
					return source.Length;
				}
				else
				{
					return Int64.MaxValue;
				}
			}

			public virtual long GetDestinationLength()
			{
				if (destination.CanSeek)
				{
					return destination.Length;
				}
				else
				{
					return Int64.MaxValue;
				}
			}
		}

		protected class BytesReadMeter
			: Meter
		{
			public BytesReadMeter(StreamPump pump)
				: base(pump)
			{
			}

			public override object CurrentValue
			{
				get
				{
					lock (pump)
					{
						return pump.bytesRead;
					}
				}
			}

			public override object MaximumValue
			{
				get
				{
					lock (pump)
					{
						return pump.sourceLength;
					}
				}
			}
		}

		protected class BytesWrittenMeter
			: Meter
		{
			public BytesWrittenMeter(StreamPump pump)
				: base(pump)
			{
			}

			public override object CurrentValue
			{
				get
				{
					lock (pump)
					{
						return pump.bytesWritten;
					}
				}
			}

			public override object MaximumValue
			{
				get
				{
					lock (pump)
					{
						return pump.streamProvider.GetSourceLength();
					}
				}
			}
		}

		protected abstract class Meter
			: AbstractMeter
		{
			protected StreamPump pump;

			protected Meter(StreamPump pump)
			{
				this.pump = pump;
			}

			public override object Owner
			{
				get
				{
					return pump;
				}
			}

			public override object MinimumValue
			{
				get
				{
					return 0;
				}
			}

			public override string Units
			{
				get
				{
					return "bytes";
				}
			}

			public virtual void RaiseValueChanged(long oldValue)
			{
				OnValueChanged(oldValue, this.CurrentValue);
			}

			public virtual void RaiseMajorChange()
			{
				OnValueChanged(this.CurrentValue, this.CurrentValue);
				OnMajorChange();
			}
		}

		#endregion

		private readonly byte[] buffer;
		private long bytesRead = 0;
		private long bytesWritten = 0;
		private long sourceLength = -1;
		private Meter bytesReadMeter;
		private Meter bytesWrittenMeter;
		private readonly bool autoCloseSource;
		private readonly bool autoCloseDestination;
		private IStreamProvider streamProvider;

		protected IStreamProvider StreamProvider
		{
			get
			{
				return streamProvider;
			}
			set
			{
				streamProvider = value;
			}
		}

		public StreamPump(Stream source, Stream destination)
			: this(source, destination, true, true)
		{
		}

		public StreamPump(Stream source, Stream destination, int bufferSize)
			: this(source, destination, true, true, bufferSize)
		{
		}

		public StreamPump(Stream source, Stream destination, bool autoCloseSource, bool autoCloseDestination)
			: this(source, destination, autoCloseSource, autoCloseDestination, -1)
		{
		}

		public StreamPump(Stream source, Stream destination, bool autoCloseSource, bool autoCloseDestination, int bufferSize)
			: this(new StaticStreamProvider(source, destination), autoCloseSource, autoCloseDestination, bufferSize)
		{	
		}

		public StreamPump(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination)
			: this(streamProvider, autoCloseSource, autoCloseDestination, -1)
		{	
		}

		public StreamPump(IStreamProvider streamProvider, bool autoCloseSource, bool autoCloseDestination, int bufferSize)
		{
			this.streamProvider = streamProvider;

			this.autoCloseSource = autoCloseSource;
			this.autoCloseDestination = autoCloseDestination;
			buffer = new byte[bufferSize > 0 ? bufferSize : DefaultBufferSize];

			bytesReadMeter = new BytesReadMeter(this);
			bytesWrittenMeter = new BytesReadMeter(this);

			sourceLength = this.streamProvider.GetSourceLength();
		}

		protected StreamPump(bool autoCloseSource, bool autoCloseDestination, int bufferSize)
		{
			streamProvider = (IStreamProvider)this;

			this.autoCloseSource = autoCloseSource;
			this.autoCloseDestination = autoCloseDestination;
			buffer = new byte[bufferSize > 0 ? bufferSize : DefaultBufferSize];
		}

		protected virtual void InitializePump()
		{
			bytesReadMeter = new BytesReadMeter(this);
			bytesWrittenMeter = new BytesReadMeter(this);

			sourceLength = streamProvider.GetSourceLength();
		}

		public override IMeter Progress
		{
			get
			{
				return bytesWrittenMeter;
			}
		}

		public virtual IMeter WriteProgress
		{
			get
			{
				return bytesWrittenMeter;
			}
		}

		public virtual IMeter ReadProgress
		{
			get
			{
				return bytesReadMeter;
			}
		}

		public override void DoRun()
		{
			int read;
			bool finished = false;
			Stream source, destination;
						
			ProcessTaskStateRequest();

			source = streamProvider.GetSourceStream();
			destination = streamProvider.GetDestinationStream();

			try
			{
				try
				{
					bytesReadMeter.RaiseValueChanged(0L);

					ProcessTaskStateRequest();

					bytesWrittenMeter.RaiseValueChanged(0L);

					ProcessTaskStateRequest();

					while (true)
					{
						read = source.Read(buffer, 0, buffer.Length);

						if (read == 0)
						{
							if (sourceLength != bytesRead)
							{
								sourceLength = bytesRead;
								bytesReadMeter.RaiseMajorChange();
								bytesWrittenMeter.RaiseMajorChange();
							}

							break;
						}

						lock (this)
						{
							bytesRead += read;
						}

						if (bytesRead > sourceLength)
						{
							sourceLength = bytesRead;
						}

						bytesReadMeter.RaiseValueChanged(bytesRead - read);

						ProcessTaskStateRequest();

						destination.Write(buffer, 0, read);

						lock (this)
						{
							bytesWritten += read;
						}

						bytesWrittenMeter.RaiseValueChanged(bytesWritten - read);

						ProcessTaskStateRequest();
					}

					finished = true;
				}
				catch (StopRequestedException)
				{
					SetTaskState(TaskState.Stopped);
				}
			}
			finally
			{
				ActionUtils.IgnoreExceptions(destination.Flush);					

				if (autoCloseSource)
				{
					ActionUtils.IgnoreExceptions(source.Close);
				}

				if (autoCloseDestination)
				{
					ActionUtils.IgnoreExceptions(destination.Close);
				}

				if (finished)
				{
					SetTaskState(TaskState.Finished);
				}
				else
				{
					SetTaskState(TaskState.Stopped);
				}
			}
		}

		public static void Copy(Stream inStream, Stream outStream)
		{
			new StreamPump(inStream, outStream, true, true).Run();
		}
	}
}
