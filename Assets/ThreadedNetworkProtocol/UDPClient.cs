using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using ThreadedNetworkProtocol;
using UnityEngine;

public class UDPClient : IClient
{

	private int lastTick;

	private UdpClient udpClient;

	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private IContextHandler contextHandler;

	public bool Active => clientState.Connected;

	public UDPClient(int port, ClientEndPoint c, ClientState cs, IContextHandler cb)
	{
		udpClient = new UdpClient(port);
		contextHandler = cb;

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<Error> Connect()
	{
		if (remoteEndPoint == null) return new Error("[UDP] Client remote end point not provided");
		if (clientState.Connected) return new Error("[UDP] Client already connected");
		if (clientState.Connecting) return new Error("[UDP] Client already busy connecting");
		if (udpClient.Client.Connected) return new Error("[UDP] Client socket open but state not connected");

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

	// public async Task<Error> Send(byte[] data)
	// {
	//Debug.Log(context.ToByteArray().Length);
	// 	return await Send(data);
	// }

	public async Task<Error> Listen()
	{
		// await Send(new Serializable.Context3D { Tick = 10 }.ToByteArray());
		while (udpClient != null && udpClient.Client != null && udpClient.Client.Connected)
		{
			UdpReceiveResult receiveBytes = await udpClient.ReceiveAsync();

			Serializable.Context3D context = Serializable.Context3D.Parser.ParseFrom(receiveBytes.Buffer);
			//Debug.Log("[" + remoteEndPoint.Port + "] Reading: " + context.Tick);
			contextHandler.HandleContext(context);
			contextHandler.SendContext(context.Tick);
			clientState.Trusted = true;
		}
		clientState.Listening = false;
		return null;
	}
}
