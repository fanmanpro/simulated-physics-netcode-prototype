using System;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
  public class Client : MonoBehaviour
  {
    // when done prototyping and extensively tested everything, remove the below variable and replace the contents of it's IF checks with
    // preprocessor directives. even better, move it into a separated library/dll for modularity with the actual game
    public bool SimulationClient = false;

    public SeatManager SeatManager;
    public ContextManager ContextManager;
    public string ClientID;

    public int Port;

    public ClientEndPoint WSRemoteEndPoint;
    public ClientEndPoint TCPRemoteEndPoint;
    public ClientEndPoint UDPRemoteEndPoint;

    public ClientState WSClientState;
    private WSClient wsClient;
    public ClientState TCPClientState;
    private TCPClient tcpClient;
    public ClientState UDPClientState;
    private UDPClient udpClient;

    private PacketHandler packetHandler;

    private void Awake()
    {
      packetHandler = new PacketHandler(SeatManager, ContextManager, this);
      WSClientState = new ClientState();
      TCPClientState = new ClientState();
      UDPClientState = new ClientState();
      //WSTryConnect();

      // somehow add this in the build command
#if SIMULATION
			SimulationClient = true;
#endif
      if (SimulationClient)
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
    }

    public async void WSTryConnect()
    {
      var err = WSRemoteEndPoint.Initialize();
      if (err != null) { Debug.LogError(err); return; }

      wsClient = new WSClient(WSRemoteEndPoint, WSClientState);
      // this blocks but the main thread still goes on so Unity continues as normal
      err = await wsClient.Connect();
      if (err != null) { Debug.LogError(err); return; }

      err = await wsClient.Listen();
      if (err != null) { Debug.LogError(err); return; }
    }

    public async void TCPTryConnect()
    {
      var err = TCPRemoteEndPoint.Initialize();
      if (err != null) { Debug.LogError(err); return; }

      tcpClient = new TCPClient(Port, TCPRemoteEndPoint, TCPClientState, packetHandler);
      // this blocks but the main thread still goes on so Unity continues as normal
      err = await tcpClient.Connect();
      if (err != null) { Debug.LogError(err); return; }

      err = await tcpClient.Send(new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.ClientAppeal, Cid = ClientID });
      if (err != null) { Debug.LogError(err); return; }

      err = await tcpClient.Listen();
      if (err != null) { Debug.LogError(err); return; }
    }

    public async void UDPTryConnect()
    {
      var err = UDPRemoteEndPoint.Initialize();
      if (err != null) { Debug.LogError(err); return; }

      udpClient = new UDPClient(Port, UDPRemoteEndPoint, UDPClientState, packetHandler);
      // this blocks but the main thread still goes on so Unity continues as normal
      err = await udpClient.Connect();
      if (err != null) { Debug.LogError(err); return; }

      err = await udpClient.Listen();
      if (err != null) { Debug.LogError(err); return; }
    }

    public async void Send(PacketType type, Gamedata.Packet packet)
    {
      switch (type)
      {
        case PacketType.Session:
          {
            //await wsClient.Send("Measure twice");
            Debug.Log("[WS] Sent packet");
          }
          break;
        case PacketType.Important:
          {
            if (!tcpClient.Active)
            {
              return;
            }
            packet.Cid = ClientID;
            await tcpClient.Send(packet);
            Debug.Log("[TCP] Sent packet");
          }
          break;
        case PacketType.Unreliable:
          {
            if (udpClient?.Active ?? true)
            {
              return;
            }
            packet.Cid = ClientID;
            await udpClient.Send(packet);
            //Debug.Log("[UDP] Sent packet");
            //Debug.Log("[UDP] TODO TODO TODO TODO Sent test packet");
          }
          break;
      }
    }

    public async void SendTestPacket(int i)
    {
      PacketType t = (PacketType)i;
      switch (t)
      {
        case PacketType.Session:
          {
            await wsClient.Send(new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.Invalid });
            Debug.Log("[WS] Sent test packet");
          }
          break;
        case PacketType.Important:
          {
            await tcpClient.Send(new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.ClientDisconnect });
            Debug.Log("[TCP] Sent test packet");
          }
          break;
        case PacketType.Unreliable:
          {
            await udpClient.Send(new Gamedata.Packet { OpCode = Gamedata.Header.Types.OpCode.Context });
            Debug.Log("[UDP] Sent test packet");
          }
          break;
      }
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
