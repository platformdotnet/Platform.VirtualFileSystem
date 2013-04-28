using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Platform.Runtime.Interop
{
	/// <summary>
	/// Provides extension methods and static utility methods for the <see cref="Marshal"/> class.
	/// </summary>
	public class MarshalUtils
	{
		/// <summary>
		/// Serializes a structure into raw bytes
		/// </summary>
		/// <param name="structure">The struture  to serialize</param>
		/// <returns></returns>
		public static byte[] RawSerialize<T>(T structure)
			where T : struct
		{
			return RawSerialize((object)structure);
		}

		/// <summary>
		/// Serializes a structure into raw bytes
		/// </summary>
		/// <param name="structure">The struture  to serialize</param>
		/// <returns></returns>
		public static byte[] RawSerialize(object structure)
		{
		    var rawsize = Marshal.SizeOf(structure);
			var buffer = Marshal.AllocHGlobal(rawsize);

			Marshal.StructureToPtr(structure, buffer, false);
			var rawdata = new byte[rawsize];
			Marshal.Copy(buffer, rawdata, 0, rawsize);
			Marshal.FreeHGlobal(buffer);

			return rawdata;
		}

		/// <summary>
		/// Deserialize a structure from bytes
		/// </summary>
		/// <typeparam name="T">The type of the structure to deserialize</typeparam>
		/// <param name="rawdata">The data that makes up the structure</param>
		/// <returns>The deserialized structure</returns>
		public static T RawDeserialize<T>(byte[] rawdata)
			where T : struct 
		{
			return (T)RawDeserialize(rawdata, typeof(T));
		}

		/// <summary>
		/// Deserialize a structure from bytes
		/// </summary>
		/// <param name="type">The type of the structure to deserialize</typeparam>
		/// <param name="rawdata">The data that makes up the structure</param>
		/// <returns>The deserialized structure</returns>
		public static object RawDeserialize(byte[] rawdata, Type type)
		{
		    var rawsize = Marshal.SizeOf(type);

			if (rawsize > rawdata.Length)
			{
				return null;
			}
			
			var buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.Copy(rawdata, 0, buffer, rawsize);
			var retval = Marshal.PtrToStructure(buffer, type);
			Marshal.FreeHGlobal(buffer);

			return retval;
		}
	}
}
