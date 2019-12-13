using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	public class SeatManager : MonoBehaviour
	{
		public Client client;
		public List<PlayerSeat> seats;
		private Dictionary<string, PlayerSeat> seatsByGUID;
		private Dictionary<string, PlayerSeat> seatsByCID;

		private void Awake()
		{
			seatsByGUID = new Dictionary<string, PlayerSeat>();
			seatsByCID = new Dictionary<string, PlayerSeat>();
			foreach (PlayerSeat seat in seats)
			{
				NetSync ns = seat.GetComponent<NetSync>();
				seatsByGUID.Add(ns.GUID, seat);
			}
		}

		public void AssignSeatByAvailability(string cid)
		{
			foreach (PlayerSeat seat in seats)
			{
				if (!seat.Assigned)
				{
					seatsByCID.Add(cid, seat);
					seat.Assigned = true;
					if (client.SimulationClient)
					{
						client.Send(PacketType.Important, new Gamedata.Packet
						{
							OpCode = Gamedata.Header.Types.OpCode.ClientSeat,
							Data = Any.Pack(new Gamedata.ClientSeat
							{
								Owner = cid,
								Guid = seat.GUID,
							})
						});
					}
				}
			}
		}

		public void AssignSeatByGUID(string cid, string guid)
		{
			PlayerSeat seat;
			if (!seatsByGUID.TryGetValue(guid, out seat))
			{
				return;
			}

			if (!seat.Assigned)
			{
				seatsByCID.Add(cid, seat);
				seat.Assigned = true;
#if SIMULATION
				client.Send(PacketType.Important, new Gamedata.Packet
				{
					OpCode = Gamedata.Header.Types.OpCode.ClientSeat,
					Data = Any.Pack(new Gamedata.ClientSeat
					{
						Owner = cid,
						Guid = guid,
					})
				});
#else
				if (client.ClientID == cid)
				{
					seat.gameObject.AddComponent(seat.LocalPlayerComponent);
					//seat.AddComponent<client.Local
				}
#endif
			};
		}
	}
}