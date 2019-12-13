using System;
using System.Net;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	[Serializable]
	public class ClientState
	{
		public bool SimulationClient;

		public bool Connecting;
		public bool Connected;
		public bool Trusted;

		public bool Listening;
		public bool Transmitting;

		public bool Disconnecting;
		public bool Disconnected;
	}
}
