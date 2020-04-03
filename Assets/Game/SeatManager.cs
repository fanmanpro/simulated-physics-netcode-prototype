using System.Collections.Generic;
using UnityEngine;

namespace ThreadedNetworkProtocol
{
	public class SeatManager : MonoBehaviour
	{
		public Client client;
		public List<Seat> seats;
		private Dictionary<string, Seat> seatsByGUID;
		//private Dictionary<string, Seat> seatsByCID;

		private void Awake()
		{
			seatsByGUID = new Dictionary<string, Seat>();
			//seatsByCID = new Dictionary<string, Seat>();
			foreach (Seat seat in seats)
			{
				NetSync ns = seat.GetComponent<NetSync>();
				seatsByGUID.Add(ns.GUID, seat);
			}
		}

		// public void AssignSeatByAvailability(string cid)
		// {
		// 	foreach (Seat seat in seats)
		// 	{
		// 		if (!seat.Assigned)
		// 		{
		// 			seatsByCID.Add(cid, seat);
		// 			seat.Assigned = true;
		// 			//client.Send(PacketType.Important, new Gamedata.Packet
		// 			//{
		// 			//	OpCode = Gamedata.Header.Types.OpCode.ClientSeat,
		// 			//	Data = Any.Pack(new Gamedata.ClientSeat
		// 			//	{
		// 			//		Owner = cid,
		// 			//		Guid = seat.GUID,
		// 			//	})
		// 			//});
		// 			return;
		// 		}
		// 	}
		// }

		public void AssignSeatByGUID(string guid)
		{
			Seat seat;
			if (!seatsByGUID.TryGetValue(guid, out seat))
			{
				return;
			}

			if (!seat.Assigned)
			{
				//seatsByCID.Add(cid, seat);
				seat.Assigned = true;
				// if (client.SimulationClient)
				// {
				//client.Send(PacketType.Important, new Gamedata.Packet
				//{
				//	OpCode = Gamedata.Header.Types.OpCode.ClientSeat,
				//	Data = Any.Pack(new Gamedata.ClientSeat
				//	{
				//		Owner = cid,
				//		Guid = guid,
				//	})
				//});
				// }
				// else
				// {
				// 	if (client.ClientID == cid)
				// 	{
				seat.gameObject.AddComponent(seat.LocalPlayerComponent);
				//seat.AddComponent<client.Local
				// 	}
				// }
			};
		}
	}
}