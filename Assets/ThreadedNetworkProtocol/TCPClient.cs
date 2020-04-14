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
	private TcpClient tcpClient;
	// NetworkStream seems to be the best approach for good async support.
	private NetworkStream tcpStream;

	private CancellationTokenSource readToken;
	// private CancellationTokenSource writeToken;

	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private IPacketHandler packetHandler;
	private IConnectionHandler connectionHandler;

	public bool Active => clientState.Connected && clientState.Trusted;

	public TCPClient(int port, ClientEndPoint c, ClientState cs, IConnectionHandler ch, IPacketHandler ph)
	{
		readToken = new CancellationTokenSource();
		// readToken.Token.Register(() => { tcpStream.Close();});
		// writeToken = new CancellationTokenSource();

		tcpClient = new TcpClient(new IPEndPoint(new IPAddress(0), port));

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
			await tcpClient.ConnectAsync(remoteEndPoint.IPEndPoint.Address, remoteEndPoint.IPEndPoint.Port);
			clientState.Connecting = false;
			if (!tcpClient.Connected) return new Error("[TCP] Client connected but weird that the socket state is stil not connected");
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
		if (!tcpClient.Connected) return new Error("[TCP] Client tried to disconnect but connection was already closed.");

		clientState.Connected = false;
		clientState.Connecting = false;
		clientState.Disconnected = false;
		clientState.Disconnecting = false;
		clientState.Listening = false;
		clientState.Transmitting = false;
		clientState.Trusted = false;


		Debug.Log("Disconnecting");
		readToken.Cancel();
		tcpClient.Close();
		// tcpClient.Dispose();
		// tcpClient.Close();
		connectionHandler.HandleDisconnect();

		return null;
	}

	public async Task<ILog> Send(byte[] packet)
	{
		try
		{
			SocketAsyncEventArgs e = new SocketAsyncEventArgs();
			e.SetBuffer(packet, 0, packet.Length);

			if (tcpClient.Client.SendAsync(e))
			{
				Debug.LogFormat("[TCP] Wrote {0} bytes for {1}", packet.Length, tcpClient.Client.RemoteEndPoint.ToString());
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

	public async Task<ILog> Listen()
	{
		ILog error = null;
		await Task.Run(() =>
		{
			using (tcpStream = tcpClient.GetStream())
			{
				while (true)
				{
					byte[] buffer = new byte[1024];
					// Debug.Log("Reading");
					try
					{
						int read = tcpStream.Read(buffer, 0, buffer.Length);
						if (read > 0)
						{
							Serializable.Packet packet = Serializable.Packet.Parser.ParseFrom(buffer.Take(read).ToArray());
							MainThreadContext.RunOnMainThread(() => packetHandler.HandlePacket(packet));

							// MainThreadContext main = new MainThreadContext();
							// main.packetQueue.Enqueue();
						}
					}
					catch (IOException)
					{
						error = new Log("[TCP] Disconnected");
						break;
					}
					catch (Exception e)
					{
						error = new Error("[TCP] " + e);
						break;
					}
				}
			}
		}, readToken.Token);

		return error;
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