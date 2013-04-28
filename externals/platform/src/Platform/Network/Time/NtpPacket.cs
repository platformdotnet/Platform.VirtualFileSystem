using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Platform.Runtime.Interop;

namespace Platform.Network.Time
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct NtpByteOctuple
	{
		[FieldOffset(0)]
		public BigEndianByteQuad FirstQuad;

		[FieldOffset(4)]
		public BigEndianByteQuad SecondQuad;

		public static NtpByteOctuple FromTimeSpan(TimeSpan timeSpan)
		{
		    var retval = new NtpByteOctuple();
			var milliseconds = (ulong)timeSpan.TotalMilliseconds;
			var intpart = (uint)(milliseconds / 1000L);
			var fractpart = (uint)(((milliseconds % 1000L) * 0x100000000L) / 1000L);

			retval.FirstQuad = BigEndianByteQuad.FromUInt32(intpart);
			retval.SecondQuad = BigEndianByteQuad.FromUInt32(fractpart);

			retval.ToTimeSpan();

			return retval;
		}

		public static NtpByteOctuple FromDateTime(DateTime dateTime)
		{
		    var referenceTime = new DateTime(1900, 1, 1, 0, 0, 0);
			var timeSpan = dateTime.ToUniversalTime() - referenceTime;

			return FromTimeSpan(timeSpan);
		}

		public TimeSpan ToTimeSpan()
		{
			ulong intpart = 0;
			ulong fractpart = 0;

		    intpart = FirstQuad.ToUInt32();
			fractpart = SecondQuad.ToUInt32();

			var milliseconds = intpart * 1000 + (fractpart * 1000) / 0x100000000UL;

			return TimeSpan.FromMilliseconds((double)milliseconds);
		}

		public DateTime ToDateTime()
		{
		    var time = new DateTime(1900, 1, 1, 0, 0, 0);

			time += ToTimeSpan();
			time = DateTime.SpecifyKind(time, DateTimeKind.Utc);

			return time;
		}
	}

	public enum NtpLeapIndicator
	{
		NoWarning = 0,		// 0 - No warning
		LastMinute61 = 1,	// 1 - Last minute has 61 seconds
		LastMinute59 = 2,	// 2 - Last minute has 59 seconds
		Alarm = 3			// 3 - Alarm condition (clock not synchronized)
	}

	//Mode field values
	public enum NtpMode
	{
		SymmetricActive = 1,	// 1 - Symmetric active
		SymmetricPassive = 2,	// 2 - Symmetric pasive
		Client = 3,				// 3 - Client
		Server = 4,				// 4 - Server
		Broadcast = 5,			// 5 - Broadcast
		Unknown = 6				// 0, 6, 7 - Reserved
	}

	// Stratum field values
	public enum NtpStratum
	{
		Unspecified = 0,			// 0 - unspecified or unavailable
		PrimaryReference = 1,		// 1 - primary reference (e.g. radio-clock)
		SecondaryReference,		// 2-15 - secondary reference (via NTP or SNTP)
		Reserved				// 16-255 - reserved
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct NtpPacket
	{
		public const int DefaultPort = 123;

		public static TimeSpan CalculateRoundTripDelay(NtpPacket resultPacket, DateTime resultTime)
		{
			return (resultTime - resultPacket.OriginateTimeSpamp) - (resultPacket.ReceiveTimeSpamp - resultPacket.TransmitTimeSpamp);
		}

		public static TimeSpan CalculateLocalClockOffset(NtpPacket resultPacket, DateTime resultTime)
		{
			return TimeSpan.FromTicks(((resultPacket.ReceiveTimeSpamp - resultPacket.OriginateTimeSpamp) + (resultPacket.TransmitTimeSpamp - resultTime)).Ticks / 2);
		}

		[FieldOffset(0)]
		private byte headerByte1;
		
		/// <summary>
		/// LeapIndicator
		/// </summary>
		public NtpLeapIndicator LeapIndicator
		{
			get
			{
			    var x = (byte)(headerByte1 >> 6);

				if (x > 3)
				{
					x = 3;
				}

				return (NtpLeapIndicator)x;
			}
			set
			{
			    var x = (byte)(((byte)value) << 6);

			    headerByte1 = (byte)((headerByte1 & 0x3F) | x);
			}
		}

		/// <summary>
		/// VersionNumber
		/// </summary>
		public int VersionNumber
		{
			get
			{
				byte x;
				
				// Bits 3 - 5

				x = (byte)((headerByte1 & 0x38) >> 3);

				return x;
			}
			set
			{
				byte x;

				x = (byte)(((byte)value) << 3);

				headerByte1 = (byte)((headerByte1 & 0xC7) | x);
			}
		}

		/// <summary>
		/// Mode
		/// </summary>
		public NtpMode Mode
		{
			get
			{
			    var x = (byte)(headerByte1 & 0x7);

				if (x > 5)
				{
					x = 5;
				}

				return (NtpMode)x;
			}
			set
			{
			    var x = (byte)value;

			    headerByte1 = (byte)((headerByte1 & 0xF8) | x);
			}
		}

		/// <summary>
		/// Stratum
		/// </summary>
		public NtpStratum Stratum
		{
			get
			{
				return (NtpStratum)stratum;
			}
			set
			{
				stratum = (byte)value;
			}
		}
		/// <summary>
		/// <see cref="Stratum"/>
		/// </summary>
		[FieldOffset(1)]
		private byte stratum;

		/// <summary>
		/// Poll
		/// </summary>
		public int PollInterval
		{
			get
			{
				return (int)pollInterval;
			}
			set
			{
				pollInterval = (sbyte)value;
			}
		}
		/// <summary>
		/// <see cref="PollInterval"/>
		/// </summary>
		[FieldOffset(2)]
		private sbyte pollInterval;

		/// <summary>
		/// Precision
		/// </summary>
		public int Precision
		{
			get
			{
				return (int)precision;
			}
			set
			{
				precision = (sbyte)value;
			}
		}
		/// <summary>
		/// <see cref="Precision"/>
		/// </summary>
		[FieldOffset(3)]
		private sbyte precision;

		/// <summary>
		/// RootDelay
		/// </summary>
		public TimeSpan RootDelay
		{
			get
			{
				return rootDelay.ToTimeSpan();
			}
			set
			{
				rootDelay.SetFrom(value);
			}
		}
		[FieldOffset(4)]
		private BigEndianByteQuad rootDelay;

		/// <summary>
		/// RootDelay
		/// </summary>
		public TimeSpan RootDisperson
		{
			get
			{
				var temp = 0;

				temp = (((((rootDispersion.Byte1 << 8) + rootDispersion.Byte2) << 8) + rootDispersion.Byte3) << 8) + rootDispersion.Byte4;

				return TimeSpan.FromMilliseconds(1000 * (((double)temp) / 0x10000));
			}
			set
			{
			    var milliseconds = value.TotalMilliseconds;

				milliseconds /= 1000;

				milliseconds *= 0x10000;

				var x = (int)milliseconds;

				rootDispersion.Byte4 = (byte)(x & 0xf);
				rootDispersion.Byte3 = (byte)((x >> 8) & 0xf);
				rootDispersion.Byte2 = (byte)((x >> 16) & 0xf);
				rootDispersion.Byte1 = (byte)((x >> 24) & 0xf);
			}
		}
		[FieldOffset(8)]
		private BigEndianByteQuad rootDispersion;

		/// <summary>
		/// ReferenceId
		/// </summary>
		public string ReferenceId
		{
			get
			{
				string val = "";

				switch (this.Stratum)
				{
					case NtpStratum.Unspecified:
						goto case NtpStratum.PrimaryReference;
					case NtpStratum.PrimaryReference:

						val = new StringBuilder().Append((char)referenceId.Byte1)
							.Append((char)referenceId.Byte2)
							.Append((char)referenceId.Byte3)
							.Append((char)referenceId.Byte4).ToString();

						break;
					case NtpStratum.SecondaryReference:
						switch (VersionNumber)
						{
							case 3:	// Version 3, Reference ID is an IPv4 address

						        var address = String.Format
						        (
									"{0}.{1}.{2}.{3}",
									referenceId.Byte1,
									referenceId.Byte2,
									referenceId.Byte3,
									referenceId.Byte4
						        );

								try
								{
									var host = Dns.GetHostEntry(address);
									val = host.HostName + " (" + address + ")";
								}
								catch (Exception)
								{
									val = "N/A";
								}

								break;
							case 4: // Version 4, Reference ID is the timestamp of last update
								/*DateTime time = ComputeDate(GetMilliSeconds(offReferenceID));
								// Take care of the time zone
								TimeSpan offspan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
								
								val = (time + offspan).ToString();
								 * */
								val = "";

								break;
							default:
								val = "N/A";
								break;
						}
						break;
				}

				return val;
			}
		}
		[FieldOffset(12)]
		private BigEndianByteQuad referenceId;

		/// <summary>
		/// ReferenceTimeSpamp
		/// </summary>
		public DateTime ReferenceTimeSpamp
		{
			get
			{
				return referenceTimeSpamp.ToDateTime().ToLocalTime();
			}
			set
			{
				referenceTimeSpamp = NtpByteOctuple.FromDateTime(value);
			}
		}
		/// <summary>
		/// <see cref="ReferenceTimeSpamp"/>
		/// </summary>
		[FieldOffset(16)]
		private NtpByteOctuple referenceTimeSpamp;

		/// <summary>
		/// OriginateTimeSpamp
		/// </summary>
		public DateTime OriginateTimeSpamp
		{
			get
			{
				return originateTimeSpamp.ToDateTime().ToLocalTime();
			}
			set
			{
				originateTimeSpamp = NtpByteOctuple.FromDateTime(value);
			}
		}
		/// <summary>
		/// <see cref="OriginateTimeSpamp"/>
		/// </summary>
		[FieldOffset(24)]
		private NtpByteOctuple originateTimeSpamp;

		/// <summary>
		/// ReceiveTimeSpamp
		/// </summary>
		public DateTime ReceiveTimeSpamp
		{
			get
			{
				return receiveTimeSpamp.ToDateTime().ToLocalTime();
			}
			set
			{
				receiveTimeSpamp = NtpByteOctuple.FromDateTime(value);
			}
		}
		/// <summary>
		/// <see cref="ReceiveTimeSpamp"/>
		/// </summary>
		[FieldOffset(32)]
		private NtpByteOctuple receiveTimeSpamp;

		/// <summary>
		/// TransmitTimeSpamp
		/// </summary>
		public DateTime TransmitTimeSpamp
		{
			get
			{
				return transmitTimeSpamp.ToDateTime().ToLocalTime();
			}
			set
			{
				transmitTimeSpamp = NtpByteOctuple.FromDateTime(value);
			}
		}
		/// <summary>
		/// <see cref="TransmitTimeSpamp"/>
		/// </summary>
		[FieldOffset(40)]
		private NtpByteOctuple transmitTimeSpamp;
	
		public byte[] ToRawByteArray()
		{
			return MarshalUtils.RawSerialize(this);
		}

		public static NtpPacket ParseRawByteArray(byte[] array)
		{
			var retval = MarshalUtils.RawDeserialize<NtpPacket>(array);

			return retval;
		}

		public static NtpPacket ReadCurrentTime(string serverName)
		{
			return ReadPacket(serverName, DefaultPort);
		}

		public static NtpPacket ReadPacket(string serverName, int port)
		{
			IPAddress ipAddress;

			var udpClient = new UdpClient();

			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

			if (IPAddress.TryParse(serverName, out ipAddress))
			{
				udpClient.Connect(ipAddress, port);
			}
			else
			{
				udpClient.Connect(serverName, port);
			}

			using (udpClient)
			{
				return ReadPacket(udpClient);
			}
		}

		public static NtpPacket ReadPacket(UdpClient udpClient)
		{
			var packet = new NtpPacket();

			packet.VersionNumber = 4;
			packet.Mode = NtpMode.Client;
			packet.TransmitTimeSpamp = DateTime.Now;

			return ReadPacket(udpClient, packet);
		}

		public static NtpPacket ReadPacket(UdpClient udpClient, NtpPacket packet)
		{
			var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

			byte[] data = null;

			for (int i = 0; i < 5; i++)
			{
				data = packet.ToRawByteArray();
				udpClient.Send(data, data.Length);

				try
				{
					data = udpClient.Receive(ref remoteEndPoint);

					break;
				}
				catch (SocketException)
				{
					continue;
				}
			}

			if (data == null)
			{
				throw new TimeoutException();
			}

			var resultPacket = NtpPacket.ParseRawByteArray(data);

			return resultPacket;
		}
	}
}
