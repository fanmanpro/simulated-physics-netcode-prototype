using System.Collections;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	public class PacketHandler
	{
		private Client client;

		private SeatManager seatManager;

		public delegate void ClientTrustHandler(Gamedata.Packet p);
		public event ClientTrustHandler ClientTrust;

		public delegate void ClientSeatHandler(Gamedata.Packet p);
		public event ClientSeatHandler ClientSeat;

		public delegate void ContextHandler(Gamedata.Packet p);
		public event ContextHandler Context;

		public PacketHandler(SeatManager s, Client c)
		{
			client = c;
			seatManager = s;
			ClientTrust += clientTrust;
			ClientSeat += clientSeat;
			Context += context;
		}

		public void Handle(Gamedata.Packet p)
		{
			switch (p.OpCode)
			{
				case Gamedata.Header.Types.OpCode.ClientTrust: ClientTrust?.Invoke(p); return;
				case Gamedata.Header.Types.OpCode.ClientSeat: ClientSeat?.Invoke(p); return;
				case Gamedata.Header.Types.OpCode.Context: Context?.Invoke(p); return;
			}
		}

		private void clientTrust(Gamedata.Packet packet)
		{
			if (client.SimulationClient)
				Debug.Log("[SIM] [ClientTrust]");
			else
				Debug.Log("[GAM] [ClientTrust]");
			client.TCPClientState.Trusted = true;
			client.UDPTryConnect();
			client.StartCoroutine(awaitClientDatagramAddressRetry());
		}

		private IEnumerator awaitClientDatagramAddressRetry()
		{
			while (true)
			{
				client.Send(PacketType.Unreliable, new Gamedata.Packet
				{
					OpCode = Gamedata.Header.Types.OpCode.ClientDatagramAddress
				});
				yield return new WaitForSeconds(1);
				//if (client.UDPClientState.Trusted)
					break;
			}
		}

		private void clientSeat(Gamedata.Packet packet)
		{
			if (client.SimulationClient)
			{
				Debug.Log("[SIM] [ClientSeat]");
				seatManager.AssignSeatByAvailability(packet.Cid);
			}
			else
			{
				Debug.Log("[GAM] [ClientSeat]");
				Gamedata.ClientSeat clientSeat;
				if (!packet.Data.TryUnpack(out clientSeat))
				{
					Debug.Log("[GAM] [ClientSeat] Failed");
					return;
				}
				seatManager.AssignSeatByGUID(clientSeat.Owner, clientSeat.Guid);
			}
		}
		private void context(Gamedata.Packet packet)
		{
			Debug.Log("[GAM] [Context]");
			//Gamedata.Conte clientSeat;
			//if (!packet.Data.TryUnpack(out clientSeat))
			//{
			//	Debug.Log("[GAM] [ClientSeat] Failed");
			//	return;
			//}
			//seatManager.AssignSeatByGUID(clientSeat.Owner, clientSeat.Guid);
		}
	}
}
