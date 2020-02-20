using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using ThreadedNetworkProtocol;
using UnityEngine;

public class UDPClient : IClient
{
    private UdpClient udpClient;

    private ClientEndPoint remoteEndPoint;

    private ClientState clientState;

    private PacketHandler packetHandler;

    public bool Active => clientState.Connected && clientState.Trusted;

    public UDPClient(
        int port,
        ClientEndPoint c,
        ClientState cs,
        PacketHandler p
    )
    {
        udpClient = new UdpClient(port);
        packetHandler = p;

        remoteEndPoint = c;
        clientState = cs;
    }

    public async Task<Error> Connect()
    {
        if (remoteEndPoint == null)
            return new Error("[UDP] Client remote end point not provided");
        if (clientState.Connected)
            return new Error("[UDP] Client already connected");
        if (clientState.Connecting)
            return new Error("[UDP] Client already busy connecting");
        if (udpClient.Client.Connected)
            return new Error("[UDP] Client socket open but state not connected");

        using (CancellationTokenSource cts = new CancellationTokenSource())
        {
            clientState.Connecting = true;

            // Doesn't really do anything. Just establishes a default remote host using the specified network endpoint.
            udpClient.Connect(remoteEndPoint.IPEndPoint);
            clientState.Connecting = false;
            if (!udpClient.Client.Connected)
                return new Error("[UDP] Client connected but weird that the socket state is stil not connected");

            // all good
            clientState.Connected = true;
            return null;
        }
    }

    public Error Disconnect()
    {
        clientState.Connected = false;
        clientState.Connecting = false;
        clientState.Disconnected = false;
        clientState.Disconnecting = false;
        clientState.Listening = false;
        clientState.Transmitting = false;
        clientState.Trusted = false;

        udpClient.Close();
        return null;
    }

    //public async Error Send(Packet packet)
    public async Task<Error> Send(byte[] bytes)
    {
        ArraySegment<byte> sendBuffer = new ArraySegment<byte>(bytes);

        //if (debug) Debug.Log(string.Format("WS - {0} {1}", "Sending packet", packet.Header.OpCode));
        using (CancellationTokenSource cts = new CancellationTokenSource())
        {
            try
            {
                await udpClient.SendAsync(sendBuffer.Array, sendBuffer.Count);

                //await clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts.Token);
                return null;
            }
            catch
            {
                return new Error("[UDP] Client failed to send packet");
            }
        }
    }

    public async Task<Error> Send(Gamedata.Packet packet)
    {
        return await Send(packet.ToByteArray());
    }

    public async Task<Error> Listen()
    {
        //if (clientWebSocket.State != WebSocketState.Open)
        //{
        //	return new Error("[UDP] Client tried to listen for packets but socket is closed");
        //}
        try
        {
            Debug.Log("receiving");
            while (udpClient.Client.Connected)
            {
                UdpReceiveResult receiveBytes = await udpClient.ReceiveAsync();

                //Gamedata.Packet packet =
                //    Gamedata.Packet.Parser.ParseFrom(receiveBytes.Buffer);
                //if (packet.OpCode != Gamedata.Header.Types.OpCode.Invalid)
                //{
                clientState.Trusted = true;
                packetHandler.Handle(null);
                //}
                //	clientState.Listening = true;

                //	byte[] rcvBytes = new byte[1024];
                //	ArraySegment<byte> rcvBuffer = new ArraySegment<byte>(rcvBytes);
                //	WebSocketReceiveResult rcvResult = await clientWebSocket.ReceiveAsync(rcvBuffer, CancellationToken.None);
                //	if (rcvResult.Count <= 0) break;
                //	//if (debug) Debug.Log(string.Format("WS - {0}", "Receiving packet"));
                //	if (rcvBytes.Length < rcvResult.Count)
                //	{
                //		Debug.LogError("Incoming message aborted because to big. Decrease message size or increase buffer length.");
                //		continue;
                //	}
                //	if (rcvResult.EndOfMessage)
                //	{
                //		byte[] msgBytes = rcvBuffer/*.Skip(rcvBuffer.Offset)*/.Take(rcvResult.Count).ToArray();
                //		Debug.Log(Encoding.UTF8.GetString(msgBytes, 0, msgBytes.Length));
                //		//Packet packet = Packet.Parser.ParseFrom(msgBytes);
                //		//HandlePacket(packet);
                //	}
            }
        }
        catch (Exception n)
        {
            clientState.Connecting = false;
            clientState.Connected = false;
            clientState.Listening = false;
            return new Error("[UDP] Client connection was forcibly closed: " +
                n);
        }
        clientState.Listening = false;
        return null;
    }
}
