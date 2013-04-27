// DeflaterOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams
{

    /// <summary>
    /// A special stream deflating or compressing the bytes that are
    /// written to it.  It uses a Deflater to perform actual deflating.<br/>
    /// Authors of the original java version : Tom Tromey, Jochen Hoenicke 
    /// </summary>
    public class InteractiveDeflaterOutputStream : Stream
    {
        /// <summary>
        /// Generate a table for a byte-wise 32-bit CRC calculation on the polynomial:
        /// x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x+1.
        ///
        /// Polynomials over GF(2) are represented in binary, one bit per coefficient,
        /// with the lowest powers in the most significant bit.  Then adding polynomials
        /// is just exclusive-or, and multiplying a polynomial by x is a right shift by
        /// one.  If we call the above polynomial p, and represent a byte as the
        /// polynomial q, also with the lowest power in the most significant bit (so the
        /// byte 0xb1 is the polynomial x^7+x^3+x+1), then the CRC is (q*x^32) mod p,
        /// where a mod b means the remainder after dividing a by b.
        ///
        /// This calculation is done using the shift-register method of multiplying and
        /// taking the remainder.  The register is initialized to zero, and for each
        /// incoming bit, x^32 is added mod p to the register if the bit is a one (where
        /// x^32 mod p is p+x^32 = x^26+...+1), and the register is multiplied mod p by
        /// x (which is shifting right by one and adding x^32 mod p if the bit shifted
        /// out is a one).  We start with the highest power (least significant bit) of
        /// q and repeat for all eight bits of q.
        ///
        /// The table is simply the CRC of all possible eight bit values.  This is all
        /// the information needed to generate CRC's on data a byte at a time for all
        /// combinations of CRC register values and incoming bytes.
        /// </summary>
        public sealed class Crc32 : IChecksum
        {
            readonly static uint CrcSeed = 0xFFFFFFFF;

            readonly static uint[] CrcTable = new uint[] {
			0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419,
			0x706AF48F, 0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4,
			0xE0D5E91E, 0x97D2D988, 0x09B64C2B, 0x7EB17CBD, 0xE7B82D07,
			0x90BF1D91, 0x1DB71064, 0x6AB020F2, 0xF3B97148, 0x84BE41DE,
			0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7, 0x136C9856,
			0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9,
			0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4,
			0xA2677172, 0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B,
			0x35B5A8FA, 0x42B2986C, 0xDBBBC9D6, 0xACBCF940, 0x32D86CE3,
			0x45DF5C75, 0xDCD60DCF, 0xABD13D59, 0x26D930AC, 0x51DE003A,
			0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423, 0xCFBA9599,
			0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
			0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190,
			0x01DB7106, 0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F,
			0x9FBFE4A5, 0xE8B8D433, 0x7807C9A2, 0x0F00F934, 0x9609A88E,
			0xE10E9818, 0x7F6A0DBB, 0x086D3D2D, 0x91646C97, 0xE6635C01,
			0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E, 0x6C0695ED,
			0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
			0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3,
			0xFBD44C65, 0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2,
			0x4ADFA541, 0x3DD895D7, 0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A,
			0x346ED9FC, 0xAD678846, 0xDA60B8D0, 0x44042D73, 0x33031DE5,
			0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA, 0xBE0B1010,
			0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
			0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17,
			0x2EB40D81, 0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6,
			0x03B6E20C, 0x74B1D29A, 0xEAD54739, 0x9DD277AF, 0x04DB2615,
			0x73DC1683, 0xE3630B12, 0x94643B84, 0x0D6D6A3E, 0x7A6A5AA8,
			0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1, 0xF00F9344,
			0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB,
			0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A,
			0x67DD4ACC, 0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5,
			0xD6D6A3E8, 0xA1D1937E, 0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1,
			0xA6BC5767, 0x3FB506DD, 0x48B2364B, 0xD80D2BDA, 0xAF0A1B4C,
			0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55, 0x316E8EEF,
			0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
			0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE,
			0xB2BD0B28, 0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31,
			0x2CD99E8B, 0x5BDEAE1D, 0x9B64C2B0, 0xEC63F226, 0x756AA39C,
			0x026D930A, 0x9C0906A9, 0xEB0E363F, 0x72076785, 0x05005713,
			0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38, 0x92D28E9B,
			0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242,
			0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1,
			0x18B74777, 0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C,
			0x8F659EFF, 0xF862AE69, 0x616BFFD3, 0x166CCF45, 0xA00AE278,
			0xD70DD2EE, 0x4E048354, 0x3903B3C2, 0xA7672661, 0xD06016F7,
			0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC, 0x40DF0B66,
			0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
			0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605,
			0xCDD70693, 0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8,
			0x5D681B02, 0x2A6F2B94, 0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B,
			0x2D02EF8D
		};

            internal static uint ComputeCrc32(uint oldCrc, byte bval)
            {
                return (uint)(Crc32.CrcTable[(oldCrc ^ bval) & 0xFF] ^ (oldCrc >> 8));
            }

            /// <summary>
            /// The crc data checksum so far.
            /// </summary>
            uint crc = 0;

            /// <summary>
            /// Returns the CRC32 data checksum computed so far.
            /// </summary>
            public long Value
            {
                get
                {
                    return (long)crc;
                }
                set
                {
                    crc = (uint)value;
                }
            }

            /// <summary>
            /// Resets the CRC32 data checksum as if no update was ever called.
            /// </summary>
            public void Reset()
            {
                crc = 0;
            }

            /// <summary>
            /// Updates the checksum with the int bval.
            /// </summary>
            /// <param name = "bval">
            /// the byte is taken as the lower 8 bits of bval
            /// </param>
            public void Update(int bval)
            {
                crc ^= CrcSeed;
                crc = CrcTable[(crc ^ bval) & 0xFF] ^ (crc >> 8);
                crc ^= CrcSeed;
            }

            /// <summary>
            /// Updates the checksum with the bytes taken from the array.
            /// </summary>
            /// <param name="buffer">
            /// buffer an array of bytes
            /// </param>
            public void Update(byte[] buffer)
            {
                Update(buffer, 0, buffer.Length);
            }

            /// <summary>
            /// Adds the byte array to the data checksum.
            /// </summary>
            /// <param name = "buf">
            /// the buffer which contains the data
            /// </param>
            /// <param name = "off">
            /// the offset in the buffer where the data starts
            /// </param>
            /// <param name = "len">
            /// the length of the data
            /// </param>
            public void Update(byte[] buf, int off, int len)
            {
                if (buf == null)
                {
                    throw new ArgumentNullException("buf");
                }

                if (off < 0 || len < 0 || off + len > buf.Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                crc ^= CrcSeed;

                while (--len >= 0)
                {
                    crc = CrcTable[(crc ^ buf[off++]) & 0xFF] ^ (crc >> 8);
                }

                crc ^= CrcSeed;
            }
        }

        /// <summary>
        /// This buffer is used temporarily to retrieve the bytes from the
        /// deflater and write them to the underlying output stream.
        /// </summary>
        protected byte[] buf;

        /// <summary>
        /// The deflater which is used to deflate the stream.
        /// </summary>
        protected Deflater def;

        /// <summary>
        /// Base stream the deflater depends on.
        /// </summary>
        protected Stream baseOutputStream;

        bool isClosed = false;
        bool isStreamOwner = true;

        /// <summary>
        /// Get/set flag indicating ownership of underlying stream.
        /// When the flag is true <see cref="Close"></see> will close the underlying stream also.
        /// </summary>
        public bool IsStreamOwner
        {
            get { return isStreamOwner; }
            set { isStreamOwner = value; }
        }

        ///	<summary>
        /// Allows client to determine if an entry can be patched after its added
        /// </summary>
        public bool CanPatchEntries
        {
            get
            {
                return baseOutputStream.CanSeek;
            }
        }

        /// <summary>
        /// Gets value indicating stream can be read from
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return baseOutputStream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating if seeking is supported for this stream
        /// This property always returns false
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Get value indicating if this stream supports writing
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return baseOutputStream.CanWrite;
            }
        }

        /// <summary>
        /// Get current length of stream
        /// </summary>
        public override long Length
        {
            get
            {
                return baseOutputStream.Length;
            }
        }

        /// <summary>
        /// The current position within the stream.
        /// Always throws a NotSupportedExceptionNotSupportedException
        /// </summary>
        /// <exception cref="NotSupportedException">Any attempt to set position</exception>
        public override long Position
        {
            get
            {
                return baseOutputStream.Position;
            }
            set
            {
                throw new NotSupportedException("DefalterOutputStream Position not supported");
            }
        }

        /// <summary>
        /// Sets the current position of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("DeflaterOutputStream Seek not supported");
        }

        /// <summary>
        /// Sets the length of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void SetLength(long val)
        {
            throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
        }

        /// <summary>
        /// Read a byte from stream advancing position by one
        /// </summary>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int ReadByte()
        {
            throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
        }

        /// <summary>
        /// Read a block of bytes from stream
        /// </summary>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int Read(byte[] b, int off, int len)
        {
            throw new NotSupportedException("DeflaterOutputStream Read not supported");
        }

        /// <summary>
        /// Asynchronous reads are not supported a NotSupportedException is always thrown
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
        }

        /// <summary>
        /// Asynchronous writes arent supported, a NotSupportedException is always thrown
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("DeflaterOutputStream BeginWrite not currently supported");
        }

        /// <summary>
        /// Deflates everything in the input buffers.  This will call
        /// <code>def.deflate()</code> until all bytes from the input buffers
        /// are processed.
        /// </summary>
        protected void Deflate()
        {
            for (; ; )
            {
                int len = def.Deflate(buf, 0, buf.Length);

                if (len <= 0)
                {
                    break;
                }

                if (this.keys != null)
                {
                    this.EncryptBlock(buf, 0, len);
                }

                baseOutputStream.Write(buf, 0, len);
            }

            if (!def.IsNeedingInput)
            {
                throw new SharpZipBaseException("DeflaterOutputStream can't deflate all input?");
            }
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with a default Deflater and default buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream where deflated output should be written.
        /// </param>
        public InteractiveDeflaterOutputStream(Stream baseOutputStream)
            : this(baseOutputStream, new Deflater(), 512)
        {
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with the given Deflater and
        /// default buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream where deflated output should be written.
        /// </param>
        /// <param name="defl">
        /// the underlying deflater.
        /// </param>
        public InteractiveDeflaterOutputStream(Stream baseOutputStream, Deflater defl)
            : this(baseOutputStream, defl, 512)
        {
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with the given Deflater and
        /// buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// The output stream where deflated output is written.
        /// </param>
        /// <param name="deflater">
        /// The underlying deflater to use
        /// </param>
        /// <param name="bufsize">
        /// The buffer size to use when deflating
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// bufsize is less than or equal to zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// baseOutputStream does not support writing
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// deflater instance is null
        /// </exception>
        public InteractiveDeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufsize)
        {
            if (baseOutputStream.CanWrite == false)
            {
                throw new ArgumentException("baseOutputStream", "must support writing");
            }

            if (deflater == null)
            {
                throw new ArgumentNullException("deflater");
            }

            if (bufsize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufsize");
            }

            this.baseOutputStream = baseOutputStream;
            buf = new byte[bufsize];
            def = deflater;
        }

        /// <summary>
        /// Flushes the stream by calling flush() on the deflater and then
        /// on the underlying stream.  This ensures that all bytes are
        /// flushed.
        /// </summary>
        public override void Flush()
        {            
            if (m_PossibleBytesPending)
            {
                def.SetInput(new byte[0]);

                def.Flush();
                Deflate();

                m_PossibleBytesPending = false;
            }

            baseOutputStream.Flush();
        }

        /// <summary>
        /// Finishes the stream by calling finish() on the deflater. 
        /// </summary>
        /// <exception cref="SharpZipBaseException">
        /// Not all input is deflated
        /// </exception>
        public virtual void Finish()
        {
            def.Finish();
            while (!def.IsFinished)
            {
                int len = def.Deflate(buf, 0, buf.Length);
                if (len <= 0)
                {
                    break;
                }

                if (this.keys != null)
                {
                    this.EncryptBlock(buf, 0, len);
                }

                baseOutputStream.Write(buf, 0, len);
            }
            if (!def.IsFinished)
            {
                throw new SharpZipBaseException("Can't deflate all input?");
            }
            baseOutputStream.Flush();
            keys = null;
        }

        /// <summary>
        /// Calls finish() and closes the underlying
        /// stream when <see cref="IsStreamOwner"></see> is true.
        /// </summary>
        public override void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                Finish();
                if (isStreamOwner)
                {
                    baseOutputStream.Close();
                }
            }
        }

        /// <summary>
        /// Writes a single byte to the compressed output stream.
        /// </summary>
        /// <param name="bval">
        /// The byte value.
        /// </param>
        public override void WriteByte(byte bval)
        {
            byte[] b = new byte[1];
            b[0] = bval;
            Write(b, 0, 1);
        }

        private bool m_PossibleBytesPending = false;

        /// <summary>
        /// Writes bytes from an array to the compressed stream.
        /// </summary>
        /// <param name="buf">
        /// The byte array
        /// </param>
        /// <param name="off">
        /// The offset into the byte array where to start.
        /// </param>
        /// <param name="len">
        /// The number of bytes to write.
        /// </param>
        public override void Write(byte[] buf, int off, int len)
        {
            def.SetInput(buf, off, len);
            Deflate();

            m_PossibleBytesPending = true;
        }

        #region Encryption

        // TODO:  Refactor this code.  The presence of Zip specific code in this low level class is wrong
        string password = null;
        uint[] keys = null;

        /// <summary>
        /// Get/set the password used for encryption.  When null no encryption is performed
        /// </summary>
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                if (value != null && value.Length == 0)
                {
                    password = null;
                }
                else
                {
                    password = value;
                }
            }
        }


        /// <summary>
        /// Encrypt a single byte 
        /// </summary>
        /// <returns>
        /// The encrypted value
        /// </returns>
        protected byte EncryptByte()
        {
            uint temp = ((keys[2] & 0xFFFF) | 2);
            return (byte)((temp * (temp ^ 1)) >> 8);
        }


        /// <summary>
        /// Encrypt a block of data
        /// </summary>
        /// <param name="buffer">
        /// Data to encrypt.  NOTE the original contents of the buffer are lost
        /// </param>
        /// <param name="offset">
        /// Offset of first byte in buffer to encrypt
        /// </param>
        /// <param name="length">
        /// Number of bytes in buffer to encrypt
        /// </param>
        protected void EncryptBlock(byte[] buffer, int offset, int length)
        {
            // TODO: refactor to use crypto transform
            for (int i = offset; i < offset + length; ++i)
            {
                byte oldbyte = buffer[i];
                buffer[i] ^= EncryptByte();
                UpdateKeys(oldbyte);
            }
        }

        /// <summary>
        /// Initializes encryption keys based on given password
        /// </summary>
        protected void InitializePassword(string password)
        {
            keys = new uint[] {
				0x12345678,
				0x23456789,
				0x34567890
			};

            for (int i = 0; i < password.Length; ++i)
            {
                UpdateKeys((byte)password[i]);
            }
        }

        /// <summary>
        /// Update encryption keys 
        /// </summary>		
        protected void UpdateKeys(byte ch)
        {
            keys[0] = Crc32.ComputeCrc32(keys[0], ch);
            keys[1] = keys[1] + (byte)keys[0];
            keys[1] = keys[1] * 134775813 + 1;
            keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
        }
        #endregion
    }
}
