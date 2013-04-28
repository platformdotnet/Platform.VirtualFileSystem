using System;
using System.IO;
using System.Security.Cryptography;
using Platform.IO;

namespace Platform.Security.Cryptography
{
	/// <summary>
	/// A stream wrapper implementation that implements support cryptography
	/// that supports tream flushing even if the current cryptographic buffer
	/// has not yet been filled.
	/// </summary>
	/// <remarks>
	/// Cryptographic blocks that have not yet been filled when <see cref="Flush()"/>
	/// is called will be filled with random data.  Cryptographic blocks are marked
	/// with the length of the real data.
	/// </remarks>
	public class InteractiveCryptoStream
		: StreamWrapper
	{
		private readonly byte[] readOneByte = new byte[1];
		private readonly byte[] writeOneByte = new byte[1];
		private readonly byte[] readPreBuffer;
		private readonly byte[] readBuffer;
		private int readBufferCount;
		private int readBufferIndex;
		private readonly byte[] writeBuffer;
		private readonly byte[] writeBlockBuffer;
		private int writeBufferCount;
		private readonly ICryptoTransform transform;
		private readonly CryptoStreamMode mode;

		public override bool CanRead
		{
			get
			{
				return mode == CryptoStreamMode.Read;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return mode == CryptoStreamMode.Write;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
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

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public InteractiveCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
			: this(stream, transform, mode, 255)
		{
			
		}

		public InteractiveCryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode, int bufferSizeInBlocks)
			: base(stream)
		{
			if (bufferSizeInBlocks < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSizeInBlocks", bufferSizeInBlocks, "BufferSize can't be less than 0");
			}

			if (bufferSizeInBlocks > 255)
			{
				bufferSizeInBlocks = 255;
			}

			this.mode = mode;

			this.transform = transform;
			
			writeBuffer = new byte[2 + transform.InputBlockSize * bufferSizeInBlocks];
			writeBlockBuffer = new byte[transform.OutputBlockSize];

			readPreBuffer = new byte[transform.OutputBlockSize * 5];
			readBuffer = new byte[transform.InputBlockSize * 5];

			writeBufferCount = 2;
			readBufferCount = 0;
			readBufferIndex = 0;
		}

		public override void WriteByte(byte value)
		{
			writeOneByte[0] = value;

			Write(writeOneByte, 0, 1);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			int length;

			if (!CanWrite)
			{
				throw new NotSupportedException("Reading not supported");
			}

			if (count == 0)
			{
				return;
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("offset and length exceed buffer size");
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", offset, "can not be negative");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", count, "can not be negative");
			}

			while (count > 0)
			{
				length = Math.Min(count, writeBuffer.Length - writeBufferCount);

				if (length == 0)
				{
					Flush();
	
					continue;
				}

				Array.Copy(buffer, offset, writeBuffer, writeBufferCount, length);

				writeBufferCount += length;
				count -= length;
			}
		}

		private int bytesLeftInSuperBlock = 0;
		private int bytesPaddingSuperBlock = 0;

		public override int ReadByte()
		{
			int x;

			x = Read(readOneByte, 0, 1);

			if (x == -1)
			{
				return -1;
			}

			return readOneByte[0];
		}

		private int ReadAndConvertBlocks()
		{
			int x, read;

			read = 0;

			for (;;)
			{
				x = base.Read(readPreBuffer, 0, readPreBuffer.Length - read);

				if (x == 0)
				{
					return -1;
				}

				read += x;

				if (read % transform.OutputBlockSize == 0)
				{
					break;
				}
			}

			if (transform.CanTransformMultipleBlocks)
			{
				return transform.TransformBlock(readPreBuffer, 0, read, readBuffer, 0);
			}
			else
			{
				x = 0;

				for (int i = 0; i < read / transform.OutputBlockSize; i++)
				{
					x += transform.TransformBlock(readPreBuffer,
						i * transform.OutputBlockSize,
						transform.OutputBlockSize,
					                                readBuffer,
						i * transform.InputBlockSize);
				}
				
				return x;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int x, length;

			if (!CanRead)
			{
				throw new NotSupportedException("Reading not supported");
			}

			if (count == 0)
			{
				return 0;
			}

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("offset and length exceed buffer size");
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", offset, "can not be negative");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", count, "can not be negative");
			}

			for (;;)
			{
				if (readBufferCount == 0)
				{
					for (;;)
					{
						x = ReadAndConvertBlocks();

						if (x == 0)
						{
							continue;
						}
						else if (x == -1)
						{
							readBufferIndex = 0;
							readBufferCount = 0;

							return 0;
						}

						readBufferIndex = 0;
						readBufferCount = x;

						break;
					}
				}

				if (bytesLeftInSuperBlock == 0)
				{
					bytesLeftInSuperBlock = readBuffer[readBufferIndex];
					bytesLeftInSuperBlock |= readBuffer[readBufferIndex + 1] << 8;

					bytesLeftInSuperBlock += 2;
					
					if (bytesLeftInSuperBlock == 2)
					{
						bytesLeftInSuperBlock = 0;
						readBufferIndex += transform.InputBlockSize;
						readBufferCount -= transform.InputBlockSize;

						continue;
					}

					readBufferCount -= 2;
					readBufferIndex += 2;
					
					bytesPaddingSuperBlock = (((bytesLeftInSuperBlock + 15) / 16) * 16) - bytesLeftInSuperBlock;

					bytesLeftInSuperBlock -= 2;
				}

				length = MathUtils.Min(bytesLeftInSuperBlock, readBufferCount, count);

				Array.Copy(readBuffer, readBufferIndex, buffer, offset, length);

				bytesLeftInSuperBlock -= length;
				readBufferCount -= length;
				readBufferIndex += length;

				if (bytesLeftInSuperBlock == 0)
				{
					readBufferIndex += bytesPaddingSuperBlock;
					readBufferCount -= bytesPaddingSuperBlock;
				}

				return length;
			}
		}

		private readonly Random random = new Random();

		public override void Flush()
		{
			PrivateFlush();

			// The ICryptoTransform always keeps the most recent block
			// in memory so we have to write an empty block to get the
			// result of the encryption of the last useful block.

			writeBuffer[0] = 0;
			writeBuffer[1] = 0;

			for (int i = 2; i < transform.InputBlockSize; i++)
			{
				writeBuffer[i] = (byte)random.Next(0, 255);
			}
																	   
			transform.TransformBlock(writeBuffer, 0, transform.InputBlockSize,  writeBlockBuffer, 0);

			base.Write(writeBlockBuffer, 0, writeBlockBuffer.Length);
			base.Flush();
		}

		private void PrivateFlush()
		{
			int x;
			int length;
			int numberOfBlocks;
			
			if (writeBufferCount == 2)
			{
				return;
			}

			numberOfBlocks = ((writeBufferCount - 1) / transform.InputBlockSize) + 1;

			if (numberOfBlocks == 0)
			{
				return;
			}
			
			writeBuffer[0] = ((byte)((writeBufferCount - 2) & 0xff));
			writeBuffer[1] = ((byte)(((writeBufferCount - 2) & 0xff00) >> 8));

			for (int i = 0; i < numberOfBlocks; i++)
			{
				length = transform.InputBlockSize;
				
				if (i == numberOfBlocks - 1)
				{
					if (writeBuffer.Length - writeBufferCount < transform.InputBlockSize)
					{
						Array.Clear(writeBuffer, writeBufferCount, writeBuffer.Length - writeBufferCount);

						length = transform.InputBlockSize - (writeBuffer.Length - writeBufferCount);
					}
				}

				x = transform.TransformBlock(writeBuffer, i * transform.InputBlockSize, transform.InputBlockSize,  writeBlockBuffer, 0);

				if (x != transform.InputBlockSize)
				{
					throw new Exception();
				}

				base.Write(writeBlockBuffer, 0, writeBlockBuffer.Length);
			}

			writeBufferCount = 2;
		}
	}
}
