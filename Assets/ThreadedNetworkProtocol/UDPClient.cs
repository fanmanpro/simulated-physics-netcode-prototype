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
			return new Error("[UDP] Client failed to connect to remote server: " + n);
		}
	}

	public ILog Disconnect()
	{
		if (socket == null) return new Error("[UDP] Client cannot disconnect without a socket defined");
		if (!socket.Connected) return new Error("[UDP] Client tried to disconnect but socket was already closed");
		if (clientState.Connecting) return new Error("[UDP] Client cannot disconnect while connecting");
		if (clientState.Disconnecting) return new Error("[UDP] Client already disconnecting");
		if (clientState.Disconnected) return new Error("[UDP] Client cannot disconnect when already disconnected");

		clientState.Connected = false;
		clientState.Disconnected = true;

		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		socket.Dispose();
		socket = null;

		return null;
	}

	public async Task<ILog> Send(byte[] bytes)
	{
		try
		{
			ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
			int sent = await socket.SendAsync(buffer, SocketFlags.None);
			return null;
		}
		catch
		{
			return new Error("[UDP] Client failed to send data");
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

				Serializable.Context3D context = Serializable.Context3D.Parser.ParseFrom(buffer.Take(read).ToArray());
				if (context.Client)
				{
					contextHandler.HandleContext(context);
				}
				else
				{
					contextHandler.HandleContext(context);
					contextHandler.SendContext(context.Tick);
				}
			}
			catch (Exception e)
			{
				clientState.Connected = false;
				if (socket == null)
				{
					return null;
				}
				return new Error("[UDP] " + e);
			}
		}
		return error;
	}
}
