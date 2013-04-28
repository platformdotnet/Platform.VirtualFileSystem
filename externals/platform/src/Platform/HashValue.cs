using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Platform.Text;

namespace Platform
{
	/// <summary>
	/// A structure that holds a hash code.
	/// </summary>
	public struct HashValue
		: IValued<byte[]>
	{
	    /// <summary>
	    /// The name of the algorithm used to perform the hash
	    /// </summary>
	    public string AlgorithmName
	    {
	        get;
	        private set;
	    }

	    /// <summary>
	    /// The offset in the data that was hashed
	    /// </summary>
	    public long Offset
	    {
	        get;
	        private set;
	    }

	    /// <summary>
	    /// The length of the data that was hashed
	    /// </summary>
	    public long Length
	    {
	        get;
	        private set;
	    }

	    /// <summary>
	    /// The hash
	    /// </summary>
	    public byte[] Value
	    {
	        get;
	        private set;
	    }

	    /// <summary>
		/// A text version of the hash in text hexadecimal notation
		/// </summary>
		public string TextValue
		{
			get
			{
				return TextConversion.ToHexString(this.Value, true);
			}
		}

		/// <summary>
		/// A text version of the hash in base64 encoding
		/// </summary>
		public string Base64TextValue
		{
			get
			{
				return TextConversion.ToBase64String(this.Value);
			}
		}

		/// <summary>
		/// A text version of the hash in base32 encoding
		/// </summary>
		public string Base32TextValue
		{
			get
			{
				return TextConversion.ToBase32String(this.Value);
			}
		}

		/// <summary>
		/// The hash value (a byte array)
		/// </summary>
		object IValued.Value
		{
			get
			{
				return this.Value;
			}
		}

		/// <summary>
		/// Constructs a new <see cref="HashValue"/>
		/// </summary>
		/// <param name="value">The hash as a byte array</param>
		/// <param name="algorithm">The algorithm used to create the hash</param>
		/// <param name="offset">The offset in the data (in bytes) where the hashing started</param>
		/// <param name="length">The length of the data (in bytes) that was hashed</param>
		public HashValue(byte[] value, string algorithm, long offset, long length)
		    : this()
		{
			Value = value;
			AlgorithmName = algorithm;
			Offset = offset;
			Length = length;
		}

		/// <summary>
		/// Gets a hashcode for the current object
		/// </summary>
		/// <returns>
		/// A hashcode
		/// </returns>
		public override int GetHashCode()
		{
			int hash = 0;
			int offset = 0;

			for (int i = 0; i < Value.Length; i++)
			{
				hash ^= Value[i] >> (offset * 8);

				offset++;
				offset %= 4;
			}

			return hash;
		}

		/// <summary>
		/// Writes the hash (byte array) to the given <see cref="Stream"/>
		/// </summary>
		/// <param name="stream">The <see cref="Stream"/> to write the hash to</param>
		public void WriteTo(Stream stream)
		{
			stream.Write(this.Value, 0, this.Value.Length);
		}

		/// <summary>
		/// Writes the hash (byte array) to the given <see cref="BinaryWriter"/>
		/// </summary>
		/// <param name="writer">The <see cref="BinaryWriter"/> to write the hash to</param>
		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(this.Value, 0, this.Value.Length);
		}

		/// <summary>
		/// Writes the hash (hexadecimal text value) to the given <see cref="TextWriter"/>
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> to write the hash to</param>
		public void WriteTo(TextWriter writer)
		{
			writer.Write(this.TextValue);
		}

		/// <summary>
		/// Returns the <see cref="TextValue"/> of the current <see cref="HashValue"/>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return this.TextValue;
		}
		
		/// <summary>
		/// Returns True if the current <see cref="HashValue"/> contains the same hash as the
		/// given <see cref="obj"/>.
		/// </summary>
		/// <param name="obj">A <see cref="HashValue"/> object to compare to the current object</param>
		/// <returns>True if the current object matches the given <see cref="obj"/></returns>
		public override bool Equals(object obj)
		{
		    var compareObj = obj as HashValue?;

			if (compareObj == null)
			{
				return false;
			}
            
			if (this.Value != compareObj.Value.Value)
			{
				if (!this.Value.ElementsAreEqual(compareObj.Value.Value))
				{
					return false;
				}
			}
			
			if (Length != compareObj.Value.Length)
			{
				return false;
			}

			if (Offset != compareObj.Value.Offset)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Compares two <see cref="HashValue"/> objects for equality (compares hash byte arrays).
		/// </summary>
		public static bool operator ==(HashValue result1, HashValue result2)
		{
			return result1.Equals(result2);
		}

		/// <summary>
		/// Compares two <see cref="HashValue"/> objects for equality (compares hash byte arrays).
		/// </summary>
		public static bool operator !=(HashValue result1, HashValue result2)
		{
			return !result1.Equals(result2);
		}
	}
}
