using System;
using System.IO;

namespace Platform.IO
{
	/// <summary>
	/// A <see cref="StreamWrapper"/> that provides a partial view of a given stream.
	/// </summary>
	public class PartialStream
		: StreamWrapper
	{
		private readonly long offset;
		private readonly long length;

		/// <summary>
		/// Constructs a new <see cref="PartialStream"/> that provides a view of the given
		/// <see cref="stream"/> but starting at <see cref="offset"/>.
		/// </summary>
		/// <param name="stream">The inner stream</param>
		/// <param name="offset">The offset (in bytes) within the inner stream that the current stream will start at</param>
		public PartialStream(Stream stream, long offset)
			: this(stream, offset, -1)
		{
		}


		/// <summary>
		/// Constructs a new <see cref="PartialStream"/> that provides a view of the given
		/// <see cref="stream"/> but starting at <see cref="offset"/> and lasting for <see cref="length"/>
		/// </summary>
		/// <param name="stream">The inner stream</param>
		/// <param name="offset">The offset (in bytes) within the inner stream that the current stream will start at</param>
		/// <param name="length">
		/// The length (in bytes) that the current stream should last.  The actual length of the current stream will
		/// depend on the length of the inner stream.
		/// </param>
		public PartialStream(Stream stream, long offset, long length)
			: base(stream)
		{
			this.offset = offset;
			this.length = length;

			if (stream.CanSeek)
			{
				stream.Position = offset;
			}
			else
			{
				if (stream.CanRead)
				{
				    var buffer = new byte[255];

					var bytesLeft = (int)offset;

					while (bytesLeft > 0)
					{
						int read = stream.Read(buffer, 0, bytesLeft);

						if (read == 0)
						{
							break;
						}

						bytesLeft -= read;
					}
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (this.CanSeek)
			{
				if (length >= 0)
				{
					if (this.Position >= this.Length)
					{
						return 0;
					}

					if (this.Position + count >= this.Length)
					{
						count = checked((int)(this.Length - this.Position));
					}
				}
			}

			return base.Read(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			offset = (int)(this.offset + offset);

			if (length > 0)
			{
				count = Math.Min(count, (int)(this.offset + length - offset));

				if (count < 0)
				{
					count = 0;
					offset = 0;
				}
			}
			
			return base.BeginRead(buffer, offset, count, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			offset = (int)(this.offset + offset);

			if (length >= 0)
			{
				count = Math.Min(count, (int)(this.offset + length - offset));

				if (count < 0)
				{
					count = 0;
					offset = 0;
				}
			}

			return base.BeginWrite(buffer, (int)(this.offset + offset), count, callback, state);
		}

		public override long Length
		{
			get
			{
				return length != -1 ? length : base.Length - offset;
			}
		}

		public override long Position
		{
			get
			{
				return base.Position - offset;
			}
			set
			{
				base.Position = value + offset;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (this.CanSeek)
			{
				if (length >= 0)
				{
					if (this.Position >= this.Length)
					{
						return;
					}

					if (this.Position + count >= this.Length)
					{
						count = checked((int)(this.Length - this.Position));
					}
				}
			}
			
			base.Write(buffer, offset, count);	
		}
	}
}
