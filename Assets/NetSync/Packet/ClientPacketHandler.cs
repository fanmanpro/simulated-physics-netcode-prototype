using Serializable;
using ThreadedNetworkProtocol;
using UnityEngine;

public class ClientPacketHandler : MonoBehaviour, IPacketHandler
{
	public Client client;
	public SeatManager seatManager;
	public ClientContextManager clientContextManager;

	public void HandlePacket(Packet p)
	{
		switch (p.OpCode)
		{
			case Serializable.Packet.Types.OpCode.Seat:
				{
					Debug.Log("Client received seat packet");
					Serializable.Seat seat;
					if (!p.Data.TryUnpack(out seat)) return;

					seatManager.AssignSeatByGUID(seat.GUID);

					clientContextManager.RegisterPlayerOwned(seat.GUID);

					client.UDPTryConnect();

					clientContextManager.gameObject.SetActive(true);
				}
				break;
		}
	}
}
