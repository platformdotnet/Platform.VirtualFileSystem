using System;
using System.IO;
using System.Threading;

namespace Platform.IO
{
	/// <summary>
	/// Wraps an inner <see cref="Stream"/> class and delegates all calls to the inner class.
	/// Use this as a base class for stream wrappers and override only the methods that need
	/// to be intercepted.
	/// </summary>
	public abstract class StreamWrapper
		: Stream, IWrapperObject<Stream>
	{
		private readonly Stream wrappee;

		/// <summary>
		/// The wrapped stream
		/// </summary>
		public virtual Stream Wrappee
		{
			get
			{
				return wrappee;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="StreamWrapper"/>
		/// </summary>
		/// <param name="wrappee">The inner/wrapped stream</param>
		protected StreamWrapper(Stream wrappee)
		{
			this.wrappee = wrappee;
		}

		private Func<byte[], int, int, int> readFunction;
		private Action<byte[], int, int> writeFunction;

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{		
			if (!this.CanRead)
			{
				throw new NotSupportedException();
			}

			if (readFunction == null)
			{
				lock (this)
				{
					Func<byte[], int, int, int> localReadFunction = this.Read;

					System.Threading.Thread.MemoryBarrier();

					this.readFunction = localReadFunction;
				}
			}

			return readFunction.BeginInvoke(buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!this.CanWrite)
			{
				throw new NotSupportedException();
			}

			if (writeFunction == null)
			{
				lock (this)
				{
					Action<Byte[], int, int> localWriteFunction = this.Write;

					System.Threading.Thread.MemoryBarrier();

					this.writeFunction = localWriteFunction;
				}
			}

			return writeFunction.BeginInvoke(buffer, offset, count, callback, state);
		}

		public override bool CanRead
		{
			get
			{
				return this.Wrappee.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this.Wrappee.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.Wrappee.CanWrite;
			}
		}

		public override void Close()
		{
			this.Wrappee.Close();
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			return readFunction.EndInvoke(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			writeFunction.EndInvoke(asyncResult);
		}

		public override bool Equals(object obj)
		{
			return this.Wrappee.Equals(obj);
		}

		public override void Flush()
		{
			this.Wrappee.Flush();	
		}

		public override int GetHashCode()
		{
			return this.Wrappee.GetHashCode ();
		}

		public override long Length
		{
			get
			{
				return this.Wrappee.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.Wrappee.Position;
			}
			set
			{
				this.Wrappee.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.Wrappee.Read(buffer, offset, count);
		}

		public override int ReadByte()
		{
			return this.Wrappee.ReadByte();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.Wrappee.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.Wrappee.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.Wrappee.Write(buffer, offset, count);			
		}

		public override void WriteByte(byte value)
		{
			this.Wrappee.WriteByte(value);
		}
	}
}
