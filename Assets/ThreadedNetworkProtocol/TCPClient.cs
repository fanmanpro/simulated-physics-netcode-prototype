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
	// private TcpClient tcpClient;
	// NetworkStream seems to be the best approach for good async support.
	// private NetworkStream tcpStream;

	private CancellationTokenSource readToken;
	// private CancellationTokenSource writeToken;

	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private IPacketHandler packetHandler;
	private IConnectionHandler connectionHandler;

	public bool Active => clientState.Connected && clientState.Trusted;

	public TCPClient(bool sim, int port, ClientEndPoint c, ClientState cs, IConnectionHandler ch, IPacketHandler ph)
	{
		Debug.Log("constructing TCPClient");
		readToken = new CancellationTokenSource();
		// readToken.Token.Register(() => { tcpStream.Close();});
		// writeToken = new CancellationTokenSource();

		// IPEndPoint ipe = new IPEndPoint(address, port);
		socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
		socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);

		// socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName., false);
		// socket.ReceiveTimeout = 10;
		// socket.SendTimeout = 10;
		// socket.ExclusiveAddressUse = false;
		// socket.LingerState = new LingerOption(true, 0);
		if (sim) socket.Bind(new IPEndPoint(IPAddress.Any, port));
		// socket.RemoteEndPoint = c.IPEndPoint;
		// socketAsyncEventArgs = new SocketAsyncEventArgs();
		// socketAsyncEventArgs.RemoteEndPoint = c.IPEndPoint;
		// socketAsyncEventArgs.SetBuffer(new byte[1024], 0, 1024);

		// tcpClient = new TcpClient(new IPEndPoint(new IPAddress(0), port));
		// tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

		connectionHandler = ch;
		packetHandler = ph;

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<ILog> Connect()
	{
		if (remoteEndPoint == null) return new Error("[TCP] Client remote end point not provided");
		if (clientState.Connected) return new Error("[TCP] Client already connected");
		if (clientState.Connecting) return new Error("[TCP] Client already busy connecting");
		//if (clientWebSocket.State == WebSocketState.Open) return new Error("[TCP] Client socket open but state not connected");

		clientState.Connecting = true;
		try
		{
			await socket.ConnectAsync(remoteEndPoint.IPEndPoint.Address, remoteEndPoint.IPEndPoint.Port);
			Debug.LogFormat("[TCP] {0} -> {1}", socket.LocalEndPoint, socket.RemoteEndPoint);
			// Debug.Log("connected from " + socket.LocalEndPoint);
			// Debug.Log("connected to " + socket.RemoteEndPoint);
			clientState.Connecting = false;
			if (!socket.Connected) return new Error("[TCP] Client connected but weird that the socket state is stil not connected");
			// all good
			clientState.Connected = true;
			clientState.Trusted = true;
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
		if (!socket.Connected) return new Error("[TCP] Client tried to disconnect but connection was already closed.");

		clientState.Connected = false;
		clientState.Connecting = false;
		clientState.Disconnected = false;
		clientState.Disconnecting = false;
		clientState.Listening = false;
		clientState.Transmitting = false;
		clientState.Trusted = false;


		Debug.Log("[TCP] disconnecting");
		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
		socket.Dispose();
		socket = null;
		// tcpStream.Close();
		// tcpStream.Flush();
		// tcpStream.Dispose();
		// tcpStream = null;

		// Debug.Log(tcpClient.Available);
		// Debug.Log(tcpClient.Connected);
		// Debug.Log(tcpClient.LingerState);
		// tcpClient.Close();
		// tcpClient = null;

		// readToken.Cancel();
		// readToken.Dispose();
		// readToken = null;
		// tcpClient.Dispose();
		// tcpClient.Close();
		connectionHandler.HandleDisconnect();

		return null;
	}

	public async Task<ILog> Send(byte[] packet)
	{
		try
		{
			var buffer = new ArraySegment<byte>(packet);
			// SocketAsyncEventArgs e = new SocketAsyncEventArgs();
			// e.SetBuffer(packet, 0, packet.Length);

			int sent = await socket.SendAsync(buffer, SocketFlags.None);
			Debug.LogFormat("[TCP] Wrote {0} bytes for {1}", packet.Length, socket.RemoteEndPoint.ToString());
			return null;
		}
		catch
		{
			return new Error("[TCP] Client failed to send packet");
		}
	}

	private bool running;
	public async Task<ILog> Listen()
	{
		running = true;
		ILog error = null;
		// await Task.Run(() =>
		// {
		// using (NetworkStream tcpStream = tcpClient.GetStream())
		// {
		// Task<int> readTask = new Task<int>(() =>
		// {
		// 	return 0;
		// });
		// using (readToken.Token.Register(() =>
		// {
		// Debug.Log(readTask.Status);
		// socket.Shutdown(SocketShutdown.Both);
		// socket.Recive
		// socket.Client.Disconnect(true);
		// socket.Close();
		// }))
		// {
		// readToken.Token.ThrowIfCancellationRequested();
		while (running)
		{
			var buffer = new ArraySegment<byte>(new byte[64 * 1024 * 1024]);
			// byte[] buffer = new byte[1024];
			// Debug.Log("Reading");
			try
			{
				Debug.Log("[TCP] Reading");
				// int readTask = await socket.ReceiveAsync(buffer, 0, buffer.Length);//.ConfigureAwait(false);
				int readTask = await socket.ReceiveAsync(buffer, SocketFlags.None);

				//.ConfigureAwait(false);
				// await readTask;
				// if (readTask.IsCanceled)
				// {
				// 	error = new Log("[TCP] Disconnected NEW");
				// 	break;
				// }
				if (readTask > 0)
				{
					Serializable.Packet packet = Serializable.Packet.Parser.ParseFrom(buffer.Take(readTask).ToArray());
					MainThreadContext.RunOnMainThread(() => packetHandler.HandlePacket(packet));
				}
			}
			// catch (IOException)
			// {
			// 	error = new Log("[TCP] Disconnected");
			// 	break;
			// }
			catch (Exception e)
			{
				running = false;
				Debug.LogError(e);
				// error = new Error("[TCP] " + e);
				return error;
			}
		}
		return null;
		// }
		// }
		// tcpClient.Close();
		// });
		// tsk.Wait();
		// Debug.Log(tsk.Status);

		// catch (OperationCanceledException)
		// {
		// 	return new Error("[TCP] Disconnected");
		// }
		// catch (Exception e)
		// {
		// 	return new Error("[TCP] " + e);
		// }
	}
}