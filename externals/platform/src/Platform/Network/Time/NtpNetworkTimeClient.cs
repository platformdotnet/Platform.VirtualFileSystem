using System;
using System.Net;
using System.Net.Sockets;

namespace Platform.Network.Time
{
	/// <summary>
	/// A <see cref="NetworkTimeClient"/> that can retrieve the time form an NTP server.
	/// </summary>
	public class NtpNetworkTimeClient
		: NetworkTimeClient
	{
		/// <summary>
		/// Provides progress information for the <see cref="NtpNetworkTimeClient"/>
		/// </summary>
		public override IMeter Progress
		{
			get
			{
				return progress;
			}
		}
		private readonly ProgressMeter progress;

		/// <summary>
		/// A meter for the progress.
		/// </summary>
		private class ProgressMeter
			: MutableMeter
		{
			private string state;

			public ProgressMeter(object owner, object minimumValue, object maximumValue, object currentValue, string units)
				: base(minimumValue, maximumValue, currentValue, units)
			{
				SetOwner(owner);
			}

			public virtual void SetState(int currentValue, string state)
			{				
				this.state = state;
				SetCurrentValue(currentValue);
			}
				
			public override string ToString()
			{
				return String.Format("{0} ({1:0}%)", state, this.Percentage * 100);
			}
		}

		public const int DefaultPort = 123;

		/// <summary>
		/// Constructs a new <see cref="NtpNetworkTimeClient"/>
		/// </summary>
		/// <param name="serverName">The name or IP address of the NTP server</param>
		public NtpNetworkTimeClient(string serverName)
			: this(serverName, DefaultPort)
		{
		}

		/// <summary>
		/// Constructs a new <see cref="NtpNetworkTimeClient"/>
		/// </summary>
		/// <param name="serverName">The name or IP address of the NTP server</param>
		/// <param name="port">The UDP port number for the NTP server</param>
		public NtpNetworkTimeClient(string serverName, int port)
			: base(serverName, port)
		{
			progress = new ProgressMeter(this, 0, 4, 0, "");
		}

		private UdpClient CreateUdpClient()
		{
			IPAddress ipAddress;

			var udpClient = new UdpClient();

			udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

			if (IPAddress.TryParse(this.ServerName, out ipAddress))
			{
				udpClient.Connect(ipAddress, this.Port);
			}
			else
			{
				udpClient.Connect(this.ServerName, this.Port);
			}

			return udpClient;
		}

		/// <summary>
		/// <see cref="NetworkTimeClient.Value"/>
		/// </summary>
		public override TimeSpan? Value
		{
			get
			{
				return value;
			}
		}
		private TimeSpan? value;

		/// <summary>
		/// Clears the current value.
		/// </summary>
		public virtual void Clear()
		{
			value = null;
		}

		public override void DoRun()
		{
			UdpClient udpClient;

			SetTaskState(TaskState.Running);

			try
			{
				progress.SetState(1, "Connecting");

				System.Threading.Thread.Sleep(1000);

				udpClient = CreateUdpClient();

				progress.SetState(2, "Connected");

				var packet = new NtpPacket();

				packet.VersionNumber = 4;
				packet.Mode = NtpMode.Client;
				packet.TransmitTimeSpamp = DateTime.Now;

				var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

				var outdata = packet.ToRawByteArray();

				byte[] data = null;

				for (int i = 0; i < 5; i++)
				{
					data = null;

					progress.SetState(3, String.Format("Requesting Time (Attempt {0})", i + 1));

					udpClient.Send(outdata, outdata.Length);

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

				var now = DateTime.Now;

				if (data == null)
				{
					throw new TimeoutException();
				}

				var resultPacket = NtpPacket.ParseRawByteArray(data);
								
				var localOffset = NtpPacket.CalculateLocalClockOffset(resultPacket, now);

				value = localOffset;

				SetTaskState(TaskState.Finished);
				
				progress.SetState(4, "Finished");
			}
			finally
			{
				if (this.TaskState != TaskState.Finished)
				{
					SetTaskState(TaskState.Stopped);

					progress.SetState(4, "Stopped (Error)");
				}
			}
		}

		protected override void OnTaskException(TaskExceptionEventArgs eventArgs)
		{
			progress.SetState(4, String.Format("Stopped (Error {0})", eventArgs.Exception.ToString()));

			base.OnTaskException(eventArgs);
		}
	}
}
