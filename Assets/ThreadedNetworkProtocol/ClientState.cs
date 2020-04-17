using System;
using System.Net;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	[Serializable]
	public class ClientState
	{
		public bool Connecting;
		public bool Connected;
		public bool Disconnecting;
		public bool Disconnected;
	}
}
