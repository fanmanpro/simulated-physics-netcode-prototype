using System;
using System.Net;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	[Serializable]
	public class ClientEndPoint
	{
		public string IPAddress = "127.0.0.1";
		public int Port = 1000;
		public IPEndPoint IPEndPoint;// { get; private set; }

		public Error Initialize()
		{
			IPAddress ipAddress;
			if (System.Net.IPAddress.TryParse(IPAddress, out ipAddress))
			{
				IPEndPoint = new IPEndPoint(ipAddress, Port);
				return null;
			}
			return new Error("Invalid IP address: {0}", IPAddress);
		}
		public override string ToString()
		{
			return IPEndPoint.ToString();
		}
	}
}
