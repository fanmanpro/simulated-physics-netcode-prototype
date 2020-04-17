using Google.Protobuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ThreadedNetworkProtocol;
using UnityEngine;

public class TCPClient : IClient
{
	private Socket socket;
	private SocketAsyncEventArgs socketAsyncEventArgs;

	private CancellationTokenSource readToken;

	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private IPacketHandler packetHandler;
	private IConnectionHandler connectionHandler;

	public bool Active => clientState.Connected;

	public TCPClient(bool sim, int port, ClientEndPoint c, ClientState cs, IConnectionHandler ch, IPacketHandler ph)
	{
		readToken = new CancellationTokenSource();

		socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
		if (sim) socket.Bind(new IPEndPoint(IPAddress.Any, port));

		connectionHandler = ch;
		packetHandler = ph;

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<ILog> Connect()
	{
		if (socket == null) return new Error("[TCP] Client cannot connect without a socket defined");
		if (remoteEndPoint == null) return new Error("[TCP] Client remote end point not provided");
		if (clientState.Connected) return new Error("[TCP] Client already connected");
		if (clientState.Connecting) return new Error("[TCP] Client already busy connecting");

		clientState.Connecting = true;
		try
		{
			await socket.ConnectAsync(remoteEndPoint.IPEndPoint.Address, remoteEndPoint.IPEndPoint.Port);
			// Debug.LogFormat("[TCP] {0} -> {1}", socket.LocalEndPoint, socket.RemoteEndPoint);
			clientState.Connecting = false;
			if (!socket.Connected) return new Error("[TCP] Client connected but weird that the socket state is stil not connected");
			// all good
			clientState.Connected = true;
			connectionHandler.HandleConnect();
			return null;
		}
		catch (Exception n)
		{
			clientState.Connecting = false;
			return new Error("[TCP] Client failed to connect to remote server: " + n);
		}
	}

	public ILog Disconnect()
	{
		if (socket == null) return new Error("[TCP] Client cannot disconnect without a socket defined");
		if (!socket.Connected) return new Error("[TCP] Client tried to disconnect but socket was already closed");
		if (clientState.Connecting) return new Error("[TCP] Client cannot disconnect while connecting");
		if (clientState.Disconnecting) return new Error("[TCP] Client already disconnecting");
		if (clientState.Disconnected) return new Error("[TCP] Client cannot disconnect when already disconnected");

		clientState.Connected = false;
		clientState.Disconnected = true;

		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		socket.Dispose();
		socket = null;
		connectionHandler.HandleDisconnect();

		return null;
	}

	public async Task<ILog> Send(byte[] packet)
	{
		try
		{
			ArraySegment<byte> buffer = new ArraySegment<byte>(packet);
			int sent = await socket.SendAsync(buffer, SocketFlags.None);
			return null;
		}
		catch
		{
			return new Error("[TCP] Client failed to send data");
		}
	}

	public async Task<ILog> Listen()
	{
		ILog error = null;
		while (clientState.Connected)
		{
			try
			{
				var buffer = new ArraySegment<byte>(new byte[64 * 1024 * 1024]);
				int read = await socket.ReceiveAsync(buffer, SocketFlags.None);
				if (read > 0)
				{
					Serializable.Packet packet = Serializable.Packet.Parser.ParseFrom(buffer.Take(read).ToArray());
					packetHandler.HandlePacket(packet);
					// MainThreadContext.RunOnMainThread(() => packetHandler.HandlePacket(packet));
				}
			}
			catch (Exception e)
			{
				clientState.Connected = false;
				if (socket == null)
				{
					return null;
				}
				return new Error("[TCP] " + e);
			}
		}
		return error;
	}
}