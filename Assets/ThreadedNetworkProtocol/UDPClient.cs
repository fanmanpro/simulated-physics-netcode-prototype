using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using ThreadedNetworkProtocol;
using UnityEngine;

public class UDPClient : IClient
{

	private int lastTick;

	private Socket socket;

	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private IContextHandler contextHandler;

	public bool Active => clientState.Connected;

	public UDPClient(bool sim, int port, ClientEndPoint c, ClientState cs, IContextHandler cb)
	{
		// socket = new UdpClient(port);
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		// socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
		// socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		// socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

		if (sim) socket.Bind(new IPEndPoint(IPAddress.Any, port));
		else socket.Bind(new IPEndPoint(IPAddress.Any, 1888));

		contextHandler = cb;

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<ILog> Connect()
	{
		if (remoteEndPoint == null) return new Error("[UDP] Client remote end point not provided");
		if (clientState.Connected) return new Error("[UDP] Client already connected");
		if (clientState.Connecting) return new Error("[UDP] Client already busy connecting");
		if (socket.Connected) return new Error("[UDP] Client socket open but state not connected");

		try
		{
			// using (CancellationTokenSource cts = new CancellationTokenSource())
			// {
			clientState.Connecting = true;

			// Doesn't really do anything. Just establishes a default remote host using the specified network endpoint.
			await socket.ConnectAsync(remoteEndPoint.IPEndPoint);
			clientState.Connecting = false;
			if (!socket.Connected)
				return new Error("[UDP] Client connected but weird that the socket state is stil not connected");

			// all good
			clientState.Connected = true;
			return null;
			// }
		}
		catch (Exception n)
		{
			clientState.Connecting = false;
			return new Error("[TCP] Client failed to connect to remote server: " + n);
		}
	}

	public ILog Disconnect()
	{
		clientState.Connected = false;
		clientState.Connecting = false;
		clientState.Disconnected = false;
		clientState.Disconnecting = false;
		clientState.Listening = false;
		clientState.Transmitting = false;
		clientState.Trusted = false;

		Debug.Log("[UDP] disconnecting");
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		socket.Dispose();
		socket = null;
		return null;
	}

	//public async Error Send(Packet packet)
	public async Task<ILog> Send(byte[] bytes)
	{
		ArraySegment<byte> sendBuffer = new ArraySegment<byte>(bytes);

		//if (debug) Debug.Log(string.Format("WS - {0} {1}", "Sending packet", packet.Header.OpCode));
		try
		{
			int sent = await socket.SendAsync(sendBuffer, SocketFlags.None);
			// Debug.Log("sent " + sent + " bytes");

			//await clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Text, true, cts.Token);
			return null;
		}
		catch
		{
			return new Error("[UDP] Client failed to send packet");
		}
	}

	// public async Task<Error> Send(byte[] data)
	// {
	//Debug.Log(context.ToByteArray().Length);
	// 	return await Send(data);
	// }

	public async Task<ILog> Listen()
	{
		// await Send(new Serializable.Context3D { Tick = 10 }.ToByteArray());
		// while (socket != null && socket.Connected)
		while (true)
		{
			var buffer = new ArraySegment<byte>(new byte[64 * 1024 * 1024]);
			int read = await socket.ReceiveAsync(buffer, SocketFlags.None);

			Serializable.Context3D context = Serializable.Context3D.Parser.ParseFrom(buffer.Take(read).ToArray());
			Debug.Log(context.Client);
			if (context.Client)
			{
				contextHandler.HandleContext(context);
			}
			else
			{
				contextHandler.HandleContext(context);
				contextHandler.SendContext(context.Tick);
			}
			clientState.Trusted = true;
		}
		clientState.Listening = false;
		return null;
	}
}
