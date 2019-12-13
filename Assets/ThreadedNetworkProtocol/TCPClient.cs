using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadedNetworkProtocol;
using UnityEngine;

public class TCPClient : IClient
{
	private TcpClient tcpClient;
	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	private PacketHandler packetHandler;

	public TCPClient(ClientEndPoint c, ClientState cs, PacketHandler p)
	{
		tcpClient = new TcpClient();
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

		using (CancellationTokenSource cts = new CancellationTokenSource())
		{
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
			catch
			{
				clientState.Connecting = false;
				return new Error("[TCP] Client failed to connect to remote server");
			}
		}
	}

	public Error Disconnect()
	{
		throw new System.NotImplementedException();
	}

	public async Task<Error> Send(Gamedata.Packet packet)
	{
		using (CancellationTokenSource cts = new CancellationTokenSource())
		{
			try
			{
				SocketAsyncEventArgs e = new SocketAsyncEventArgs();
				byte[] p = packet.ToByteArray();
				e.SetBuffer(p, 0, p.Length);
				e.Completed += new EventHandler<SocketAsyncEventArgs>(SendCallback);

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
	}

	private void SendCallback(object sender, SocketAsyncEventArgs e)
	{
		if (e.SocketError == SocketError.Success)
		{
			// You may need to specify some type of state and 
			// pass it into the BeginSend method so you don't start
			// sending from scratch
			//BeginSend();
		}
		else
		{
			//Console.WriteLine("Socket Error: {0} when sending to {1}",
			//	   e.SocketError,
			//	   _asyncTask.Host);
		}
	}

	public async Task<Error> Listen()
	{
		try
		{
			using (NetworkStream stream = tcpClient.GetStream())
			{
				while (tcpClient.Connected)
				{
					byte[] buffer = new byte[1024];
					int read = await stream.ReadAsync(buffer, 0, buffer.Length);
					if (read > 0)
					{
						Gamedata.Packet packet = Gamedata.Packet.Parser.ParseFrom(buffer.Take(read).ToArray());
						if (packet.OpCode != Gamedata.Header.Types.OpCode.Invalid) {
							packetHandler.Handle(packet);
						}
						//Debug.Log(packet.OpCode);
						// you have received a message, do something with it
					}
				}
			}
		}
		catch (Exception n)
		{
			clientState.Connecting = false;
			clientState.Connected = false;
			clientState.Listening = false;
			Debug.LogError(n);
			return new Error("[TCP] Client connection was forcibly closed.\n" + n.Message);
		}
		clientState.Listening = false;
		return null;
	}
}
