#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

#endregion

namespace Platform.IO
{
	public class FifoStream
		: Stream
	{		
		private int end;
		private int start;
		private int length;
		private byte[] buffer;

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public virtual int Available
		{
			get
			{
				lock (this)
				{
					return length;
				}
			}
		}

		public virtual int Capacity
		{
			get
			{
				lock (this)
				{
					return buffer.Length;
				}
			}
		}

		private int BytesFree
		{
			get
			{
				lock (this)
				{
					return Capacity - length;
				}
			}
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public FifoStream(int bufferSize)
		{
			end = 0;
			start = 0;

			length = 0;
			buffer = new byte[bufferSize];
		}

		public override void Flush()
		{			
		}
		
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected virtual void Rebuild(int minimumSize)
		{
			int x;
			int newCapacity;			
			byte[] newBuffer;

			newCapacity = buffer.Length * 2;

			while (newCapacity < minimumSize)
			{
				newCapacity *= 2;
			}

			newBuffer = new byte[newCapacity];

			x = 0;

			while (length > 0)
			{
				x += Read(newBuffer, x, length);
			}

			buffer = newBuffer;
			start = 0;
			end = x;
			length = x;
		}

		public override int ReadByte()
		{
		    lock (this)
			{
				for (;;)
				{
					if (length == 0)
					{
						Monitor.Wait(this);

						continue;
					}

					var retval = buffer[start];

					start++;
					start %= Capacity;
					length--;

					return retval;
				}
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
		    lock (this)
			{
				while (true)
				{
					if (length == 0)
					{
						Monitor.Wait(this);

						continue;
					}

				    int bytesAfterStart;

				    if (end > start)
					{
						bytesAfterStart = end - start;
					}
					else
					{
						bytesAfterStart = Capacity - start;
					}

					var x = Math.Min(bytesAfterStart, count);

					Array.Copy(this.buffer, start, buffer, offset, x);

					start += x;
					start %= Capacity;
					length -= x;

					return x;
				}
			}
		}

		public virtual int ReadToStream(Stream dest, int count)
		{
		    lock (this)
			{
				for (;;)
				{
					if (length == 0)
					{
						Monitor.Wait(this);

						continue;
					}

					if (count > length)
					{
						count = length;
					}

				    int bytesAfterStart;
				    if (end > start)
					{
						bytesAfterStart = end - start;
					}
					else
					{
						bytesAfterStart = Capacity - start;
					}

					var x = Math.Min(bytesAfterStart, count);

					dest.Write(buffer, start, x);

					start += x;
					start %= Capacity;
					length -= x;

					return x;
				}
			}
		}

		public virtual int WriteFromStream(Stream source, int maxCount)
		{
		    lock (this)
			{
				if (BytesFree < maxCount)
				{
					Rebuild(Capacity + maxCount - BytesFree);
				}

			    int freeSpaceAfterEnd;

			    if (end >= start)
				{
					freeSpaceAfterEnd = Capacity - end;
				}
				else
				{
					freeSpaceAfterEnd = start - end;
				}

				var x = Math.Min(maxCount, freeSpaceAfterEnd);

				x = source.Read(buffer, end, x);

				end += x;
				end %= Capacity;
				length += x;

				Monitor.PulseAll(this);

				return x;
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
		    lock (this)
			{
				if (BytesFree < count)
				{
					Rebuild(Capacity + count - BytesFree);
				}

			    int freeSpaceAfterEnd;

			    if (end >= start)
				{
					freeSpaceAfterEnd = Capacity - end;
				}
				else
				{
					freeSpaceAfterEnd = start - end;
				}

				var x = Math.Min(count, freeSpaceAfterEnd);

				Array.Copy(buffer, offset, this.buffer, end, x);

				if (count - x > 0)
				{
					Array.Copy(buffer, offset + freeSpaceAfterEnd, this.buffer, 0, count - x);
				}

				end += count;
				end %= Capacity;
				length += count;

				Monitor.PulseAll(this);
			}
		}
	}
}
