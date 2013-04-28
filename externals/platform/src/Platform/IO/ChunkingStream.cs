#region Using directives

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;

#endregion

namespace Platform.IO
{
	/// <summary>
	/// A stream that maintains an internal buffer and is capable of returning
	/// data read in chunks separated by bytes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When a <c>ChunkingBuffer</c> reads from a stream it will make sure that
	/// every atomic read operation will return a buffer that contains at the most,
	/// one occurance of the chunkingMarker.
	/// </para>
	/// A read operation will return one of three things:
	/// <list type="number">
	///		<item>
	///			<description>
	///				A block of data not containing the chunkingMarker
	///			</description>
	///		</item>
	///		<item>
	///			<description>
	///				A block of data containing a single chunkingMarker at the end
	///			</description>
	///		</item>
	///		<item>
	///			<description>
	///				A block of data containing only a single chunkingMarker and 
	///				nothing else or, if the last bytes of the previous block(s) contained 
	///				the first part of the <c>chunkingMarker</c>, a block of data
	///				containing the last bytes of the chunkingMarker.
	///			</description>
	///		</item>
	/// </list>
	/// <para>
	/// This stream is useful when mixing <c>binary and <code>text</code></c> data within
	/// a single stream.  Typically, a TextReader will perform buffering so a call to 
	/// StreamReader.ReadLine() will read more than one line's data from the underlying stream.
	/// This can be a problem if the immediate contents after the line is expected to be
	/// binary data.
	/// 
	/// A typical solution would be to mark the byte length of all text contents and
	/// manually read in a byte buffer and decode it to text but that may not be appropriate
	/// for all protocols.  An easier solution would be to wrap the stream inside a
	/// ChunkingStream using the bytes that make up the new line character as a
	/// <c>chunkingMarker</c>.  Because ChunkingStreams perform buffering, all accesses from the
	/// future on should be through the ChunkingStream instance.
	/// </para>
	/// </remarks>
	public class ChunkingStream
		: MeteringStream
	{
		public static readonly int DefaultBufferSize = 4096;

		private readonly byte[] byteBuffer;
		private int byteBufferCapacity;
		private int byteBufferCount;
		private int chunkSearchStart;
		private readonly byte[] chunkingMarker;

		public ChunkingStream(Stream stream, params byte[] chunkingMarker)
			: this(stream, DefaultBufferSize, chunkingMarker)
		{
		}

		public ChunkingStream(Stream stream, int bufferSize, params byte[] chunkingMarker)
			: base(stream)
		{
			if (chunkingMarker.Length == 0)
			{
				throw new ArgumentException("chunkingMarker");
			}
						
			byteBuffer = new byte[Math.Max(DefaultBufferSize, bufferSize)];

			byteBufferCount = 0;
			byteBufferCapacity = 0;
			chunkSearchStart = 0;
			ChunkingEnabled = true;
			this.chunkingMarker = chunkingMarker;			
		}

	    public virtual bool ChunkingEnabled
	    {
	        get;
	        set;
	    }

	    public override long Seek(long offset, SeekOrigin origin)
		{
			chunkSearchStart = 0;

			return base.Seek(offset, origin);
		}

		public override long Position
		{
			get
			{
				return base.Position;
			}
			set
			{
				chunkSearchStart = 0;

				base.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
		    if (buffer == null)
			{
				throw new ArgumentNullException("array");
			}
			
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}

			if ((buffer.Length - offset) < count)
			{
				throw new ArgumentException("offset");
			}

			if (count == 0)
			{
				return 0;
			}

			for (;;)
			{
				var localByteBufferCount = this.byteBufferCount;
				var byteBufferStartIndex = byteBufferCapacity - this.byteBufferCount;

				if (this.byteBufferCount > 0)
				{
					if (ChunkingEnabled)
					{
					    int chunkMarkerIndex;
					    if (chunkingMarker.Length == 1)
						{
							try
							{
								chunkMarkerIndex = -1;

								int j = 0;

								for (int i = byteBufferStartIndex; i < byteBufferStartIndex + this.byteBufferCount; i++)
								{
									if (j > count)
									{
										break;
									}

									if (byteBuffer[i] == chunkingMarker[0])
									{
										chunkMarkerIndex = i;

										break;
									}

									j++;
								}
							}
							catch (Exception e)
							{
								chunkMarkerIndex = -1;
								Console.Error.WriteLine(e);
							}
						}
						else
						{
							chunkMarkerIndex = -1;

							var j = 0;

							for (var i = byteBufferStartIndex; i < byteBufferStartIndex + this.byteBufferCount; i++)
							{
								if (j > count)
								{
									break;
								}

								if (byteBuffer[i] != chunkingMarker[chunkSearchStart++])
								{
									chunkSearchStart = 0;

									continue;
								}
								else
								{
									if (chunkSearchStart >= chunkingMarker.Length)
									{
										chunkMarkerIndex = i;

										break;
									}
								}

								j++;
							}
						}

						if (chunkMarkerIndex >= 0)
						{
							localByteBufferCount = chunkMarkerIndex - byteBufferStartIndex + 1;
						}
					}

					var bytesToCopy = Math.Min(count, localByteBufferCount);

					Array.Copy(byteBuffer, byteBufferStartIndex, buffer, offset, bytesToCopy);

					this.byteBufferCount -= bytesToCopy;

					return bytesToCopy;
				}
				else
				{
					byteBufferCapacity = base.Read(byteBuffer, 0, byteBuffer.Length);

					if (byteBufferCapacity == 0)
					{
						return 0;
					}
					else
					{
						this.byteBufferCount = byteBufferCapacity;
					}
				}
			}
		}

		public override int ReadByte()
		{
			if (byteBufferCount > 0)
			{
				return byteBuffer[byteBufferCapacity - (byteBufferCount--)];
			}
			else
			{
				return base.ReadByte();
			}
		}
	}
}
