using System;
using System.Net;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	[Serializable]
	public class ClientEndPoint
	{
		public string RemoteIPAddress = "127.0.0.1";
		public int RemotePort = 1000;
		public int LocalPort = 1000;
		public IPEndPoint IPEndPoint { get; private set; }

		public Error Initialize()
		{
			IPAddress ipAddress;
			if (System.Net.IPAddress.TryParse(RemoteIPAddress, out ipAddress))
			{
				IPEndPoint = new IPEndPoint(ipAddress, RemotePort);
				return null;
			}
			return new Error("Invalid IP address: {0}", RemoteIPAddress);
		}
		public override string ToString()
		{
			return IPEndPoint.ToString();
		}
	}
}
