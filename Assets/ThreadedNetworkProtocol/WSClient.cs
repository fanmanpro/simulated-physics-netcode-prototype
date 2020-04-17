using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ThreadedNetworkProtocol;
using UnityEngine;

public class WSClient : IClient
{
	private ClientWebSocket clientWebSocket;
	private ClientEndPoint remoteEndPoint;

	private ClientState clientState;

	public WSClient(ClientEndPoint c, ClientState cs)
	{
		clientWebSocket = new ClientWebSocket();

		remoteEndPoint = c;
		clientState = cs;
	}

	public async Task<ILog> Connect()
	{
		if (remoteEndPoint == null) return new Error("[WS] Client remote end point not provided");
		if (clientState.Connected) return new Error("[WS] Client already connected");
		if (clientState.Connecting) return new Error("[WS] Client already busy connecting");
		if (clientWebSocket.State == WebSocketState.Open) return new Error("[WS] Client socket open but state not connected");

		using (CancellationTokenSource cts = new CancellationTokenSource())
		{
			clientState.Connecting = true;
			try
			{
				await clientWebSocket.ConnectAsync(new System.Uri(string.Format("ws://{0}/", remoteEndPoint)), cts.Token);
				clientState.Connecting = false;
				if (clientWebSocket.State != WebSocketState.Open) return new Error("[WS] Client connected but the socket state is not open");
				// all good
				clientState.Connected = true;
				return null;
			}
			catch
			{
				clientState.Connecting = false;
				return new Error("[WS] Client failed to connect to remote server");
			}
		}
	}

	public ILog Disconnect()
	{
		throw new System.NotImplementedException();
	}

	//public async Error Send(Packet packet)
	public async Task<ILog> Send(Serializable.Context3D packet)
	{
		ArraySegment<byte> sendBuffer = new ArraySegment<byte>(packet.ToByteArray());
		//if (debug) Debug.Log(string.Format("WS - {0} {1}", "Sending packet", packet.Header.OpCode));
		using (CancellationTokenSource cts = new CancellationTokenSource())
		{
			try
			{
				await clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, cts.Token);
				return null;
			}
			catch
			{
				return new Error("[WS] Client failed to send packet");
			}
		}
	}

	public async Task<ILog> Listen()
	{
		if (clientWebSocket.State != WebSocketState.Open)
		{
			return new Error("[WS] Client tried to listen for packets but socket is closed");
		}
		try
		{
			while (clientWebSocket.State == WebSocketState.Open)
			{

				byte[] rcvBytes = new byte[1024];
				ArraySegment<byte> rcvBuffer = new ArraySegment<byte>(rcvBytes);
				WebSocketReceiveResult rcvResult = await clientWebSocket.ReceiveAsync(rcvBuffer, CancellationToken.None);
				if (rcvResult.Count <= 0) break;
				//if (debug) Debug.Log(string.Format("WS - {0}", "Receiving packet"));
				if (rcvBytes.Length < rcvResult.Count)
				{
					Debug.LogError("Incoming message aborted because to big. Decrease message size or increase buffer length.");
					continue;
				}
				if (rcvResult.EndOfMessage)
				{
					byte[] msgBytes = rcvBuffer/*.Skip(rcvBuffer.Offset)*/.Take(rcvResult.Count).ToArray();
					Debug.Log(Encoding.UTF8.GetString(msgBytes, 0, msgBytes.Length));
					//Packet packet = Packet.Parser.ParseFrom(msgBytes);
					//HandlePacket(packet);
				}
			}
		}
		catch
		{
			clientState.Connecting = false;
			clientState.Connected = false;
			return new Error("[WS] Client connection was forcibly closed");
		}
		return null;
	}

	public Task<ILog> Send(IMessage p)
	{
		throw new NotImplementedException();
	}

	public Task<ILog> Send(byte[] p)
	{
		throw new NotImplementedException();
	}
}
