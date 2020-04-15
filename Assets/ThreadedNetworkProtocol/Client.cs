using System;
using Google.Protobuf;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	public class Client : MonoBehaviour
	{
		// when done prototyping and extensively tested everything, remove the below variable and replace the contents of it's IF checks with
		// preprocessor directives. even better, move it into a separated library/dll for modularity with the actual game
		public bool SimulationClient = false;

		public IConnectionHandler ConnectionHandler;
		public IContextHandler ContextHandler;
		public IPacketHandler PacketHandler;

		public string ClientID;

		//public int Port;

		public ClientEndPoint WSRemoteEndPoint;
		public ClientEndPoint TCPRemoteEndPoint;
		public ClientEndPoint UDPRemoteEndPoint;

		private WSClient wsClient;
		public ClientState WSClientState;

		private TCPClient tcpClient;
		public ClientState TCPClientState;

		private UDPClient udpClient;
		public ClientState UDPClientState;

		private void Awake()
		{
			WSClientState = new ClientState();
			TCPClientState = new ClientState();
			UDPClientState = new ClientState();

			if (!gameObject.scene.GetComponentSingle<IConnectionHandler>(out ConnectionHandler))
			{
				Debug.LogError("[Client] Unable to find a connection handler.");
				return;
			}
			if (!gameObject.scene.GetComponentSingle<IContextHandler>(out ContextHandler))
			{
				Debug.LogError("[Client] Unable to find a context handler.");
				return;
			}
			if (!gameObject.scene.GetComponentSingle<IPacketHandler>(out PacketHandler))
			{
				Debug.LogError("[Client] Unable to find a packet handler.");
				return;
			}

			//WSTryConnect();
			// somehow add this in the build command
#if SIMULATION
            //SimulationClient = true;
#endif


			if (false/*SimulationClient*/)
			{
				ClientID = null;
				string[] args = Environment.GetCommandLineArgs();
				if (args.Length < 2)
				{
					Debug.LogError("Not enough command line args for simulation client");
					return;
				}
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] == "+clientID" && args.Length > i + 1)
					{
						ClientID = args[i + 1];
					}
				}
				if (string.IsNullOrEmpty(ClientID))
				{
#if UNITY_EDITOR
					ClientID = "sim";
#else
					Debug.LogError("No +clientID command line argument provided for simulation client");
					return;
#endif
				}
			}

			TCPTryConnect();
			// UDPTryConnect();
		}

		public async void WSTryConnect()
		{
			ILog rsp = WSRemoteEndPoint.Initialize();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}

			wsClient = new WSClient(WSRemoteEndPoint, WSClientState);

			// this blocks but the main thread still goes on so Unity continues as normal
			rsp = await wsClient.Connect();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}

			rsp = await wsClient.Listen();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}
		}

		public async void TCPTryDisconnect()
		{
			tcpClient.Disconnect();
		}

		public async void TCPTryConnect()
		{
			ILog rsp = TCPRemoteEndPoint.Initialize();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}


			tcpClient = new TCPClient(SimulationClient, TCPRemoteEndPoint.LocalPort, TCPRemoteEndPoint, TCPClientState, ConnectionHandler, PacketHandler);
			Debug.Log("[TCP] connecting");
			// this blocks but the main thread still goes on so Unity continues as normal
			rsp = await tcpClient.Connect();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}

			//err = await tcpClient.Send(new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.ClientAppeal, Cid = ClientID });
			//if (err != null)
			//{
			//	Debug.LogError(err);
			//	return;
			//}

			rsp = await tcpClient.Listen();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}
		}

		public async void UDPTryDisconnect()
		{
			udpClient?.Disconnect();
		}

		public async void UDPTryConnect()
		{
			ILog rsp = UDPRemoteEndPoint.Initialize();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}

			udpClient = new UDPClient(SimulationClient, UDPRemoteEndPoint.LocalPort, UDPRemoteEndPoint, UDPClientState, ContextHandler);
			Debug.Log("[UDP] connecting");

			// this blocks but the main thread still goes on so Unity continues as normal
			rsp = await udpClient.Connect();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}

			rsp = await udpClient.Listen();
			if (rsp != null)
			{
				rsp.Print();
				return;
			}
		}

		public async void Send(Serializable.Packet packet)
		{
			if (!tcpClient.Active)
			{
				Debug.Log("[TCP] Sending packet failed. TCP client not active.");
				return;
			}
			await tcpClient.Send(packet.ToByteArray());
		}
		public async void Send(Serializable.Context3D context)
		{
			if (!udpClient.Active)
			{
				Debug.Log("[UDP] Sending packet failed. UDP client not active.");
				return;
			}
			// Debug.Log("[UDP] Sent packet (" + context.RigidBodies.Count + ")");
			await udpClient.Send(context.ToByteArray());
			// Debug.Log("[UDP] Sent context.");
		}

		private void OnDestroy()
		{
			if (TCPClientState.Connected) tcpClient.Disconnect();
			if (UDPClientState.Connected) udpClient.Disconnect();
			if (WSClientState.Connected) wsClient.Disconnect();
		}
	}

	[Serializable]
	public enum PacketType
	{
		Important = 0, // TCP
		Unreliable = 1, // UDP
		Session = 2 // WS
	}
}
