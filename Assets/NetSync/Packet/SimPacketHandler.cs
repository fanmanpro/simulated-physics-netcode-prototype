using Serializable;
using ThreadedNetworkProtocol;
using UnityEngine;

public class SimPacketHandler : MonoBehaviour, IPacketHandler
{
	public Client client;
	public void HandlePacket(Packet p)
	{
		switch (p.OpCode)
		{
			case Serializable.Packet.Types.OpCode.RunSimulation:
				{
					Debug.Log("Sim received run simulation packet");
					client.UDPTryConnect();
				}
				break;
		}
	}
}
