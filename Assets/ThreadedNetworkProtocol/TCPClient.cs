using Google.Protobuf;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ThreadedNetworkProtocol;
using UnityEngine;

public class TCPClient : IClient
{
	private TcpClient tcpClient;
	private CancellationTokenSource cts;
	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private PacketHandler packetHandler;

	public bool Active => clientState.Connected && clientState.Trusted;

	public TCPClient(int port, ClientEndPoint c, ClientState cs, PacketHandler p)
	{
		tcpClient = new TcpClient(new IPEndPoint(new IPAddress(0), port));
		packetHandler = p;

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<Error> Connect()
	{
		if (remoteEndPoint == null) return new Error("[TCP] Client remote end point not provided");
		if (clientState.Connected) return new Error("[TCP] Client already connected");
		if (clientState.Connecting) return new Error("[TCP] Client already busy connecting");
		//if (clientWebSocket.State == WebSocketState.Open) return new Error("[TCP] Client socket open but state not connected");

		cts = new CancellationTokenSource();
		clientState.Connecting = true;
		try
		{
			await tcpClient.ConnectAsync(remoteEndPoint.IPEndPoint.Address, remoteEndPoint.IPEndPoint.Port);
			clientState.Connecting = false;
			if (!tcpClient.Connected) return new Error("[TCP] Client connected but weird that the socket state is stil not connected");
			// all good
			clientState.Connected = true;
			return null;
		}
		catch (Exception n)
		{
			clientState.Connecting = false;
			return new Error("[TCP] Client failed to connect to remote server: " + n);
		}
	}

	public Error Disconnect()
	{
		if (!tcpClient.Connected) return new Error("[TCP] Client tried to disconnect but connection was already closed.");
		
		clientState.Connected = false;
		clientState.Connecting = false;
		clientState.Disconnected = false;
		clientState.Disconnecting = false;
		clientState.Listening = false;
		clientState.Transmitting = false;
		clientState.Trusted = false;

		//tcpClient.Close();
		cts.Cancel();
		//tcpClient.Dispose();
		//tcpClient.Dispose();

		return null;
	}

	public async Task<Error> Send(Serializable.Context3D packet)
	{
		try
		{
			SocketAsyncEventArgs e = new SocketAsyncEventArgs();
			byte[] p = packet.ToByteArray();
			e.SetBuffer(p, 0, p.Length);

			if (tcpClient.Client.SendAsync(e))
			{
				Debug.LogFormat("[TCP] Wrote {0} bytes for {1}", p.Length, tcpClient.Client.RemoteEndPoint.ToString());
				return null;
			}
			else
			{
				return new Error("[TCP] Client failed to send packet");
			}
		}
		catch
		{
			return new Error("[TCP] Client failed to send packet");
		}
	}

	public async Task<Error> Listen()
	{
		//try
		//{
		NetworkStream stream = tcpClient.GetStream();

		while (!cts.IsCancellationRequested)
		{
			byte[] buffer = new byte[1024];
			int read = await stream?.ReadAsync(buffer, 0, buffer.Length);
			if (read > 0)
			{
				Serializable.Context3D packet = Serializable.Context3D.Parser.ParseFrom(buffer.Take(read).ToArray());
				//if (packet.OpCode != Gamedata.Header.Types.OpCode.Invalid)
				//{
				//	packetHandler.Handle(packet);
				//}
				//Debug.Log(packet.OpCode);
				// you have received a message, do something with it
			}
		}

		//}
		//catch (Exception n)
		//{
		//	clientState.Connecting = false;
		//	clientState.Connected = false;
		//	clientState.Listening = false;
		//	return new Error("[TCP] Client connection was forcibly closed.\n" + n.Message);
		//}
		clientState.Listening = false;
		return null;
	}
}
